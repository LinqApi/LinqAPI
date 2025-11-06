// core/FormManagerAdapter.js
//
// Bu adapter 3 şeyi yapar:
// 1) Create New için modal açıp kaydetmek
// 2) Inline edit kaydetmek (controller.saveInlineEdit -> buraya gelir)
// 3) Delete çağrısı
//
// Ayrıca relations panelini DataTableView çiziyor ama burada ileride
// relation picker, upload widget, location picker, select2 vs. initialization
// gibi gelişmiş davranışlar yaşayacak.
//
// Not: apiClient.create/update/delete yoksa fallback fetch kullanıyoruz.

// ⚠️ Artık global FieldRegistry import ETMİYORUZ.
// Geriye uyumluluk için, eğer hiç registry verilmezse (ve controller.registry de yoksa)
// opsiyonel olarak globali deneyebiliriz:
let _globalRegistry = null;
try {
  // varsa yakala; yoksa sorun değil (scoped kullanımda olmayacak)
  // eslint-disable-next-line import/no-unresolved
  const maybe = await import("./FieldRegistry.js");
  _globalRegistry = maybe?.FieldRegistry || null;
} catch { /* no-op */ }

export class FormManagerAdapter {
  constructor({ controller, apiClient, debugView, registry }) {
    this.controller = controller;
    this.apiClient = apiClient;
    this.debugView = debugView;

    // 🔌 Instance-scoped registry (öncelik sırası):
    // 1) ctor ile gelen
    // 2) controller.registry (LinqDataTable DI)
    // 3) global fallback (eski mimari için)
    this.registry = registry || controller?.registry || _globalRegistry;

    if (!this.registry) {
      console.warn(
        "[FormManagerAdapter] No registry provided; falling back to basic behavior."
      );
    }

    this._modalEl = null;
  }

  // ------------- PUBLIC API -------------

  // CREATE MODAL
  openCreateForm() {
    const props = this.controller.properties || [];
    const mode = "create";
    const initialData = {};
    this._renderAndShowModal({ mode, props, initialData });
  }

  // EDIT MODAL (opsiyonel: toolbar fallback)
  openEditForm(rowData) {
    const props = this.controller.properties || [];
    const mode = "edit";
    const initialData = rowData || {};
    this._renderAndShowModal({ mode, props, initialData });
  }

  async saveInlineRowUpdate(rowId, dto) {
    // 0) Satır DOM'unu bul
    const rowEl = document.querySelector(
      `[data-row-id="${CSS.escape(String(rowId))}"]`
    );

    if (rowEl) {
      // 1) Plugin'lerin eklediği FK hidden'ları dto'ya merge et
      const fkInputs = rowEl.querySelectorAll(
        'input[type="hidden"][data-field-type^="fk-hidden"]'
      );
      fkInputs.forEach((el) => {
        if (!el.name || el.disabled) return;
        // varsa dolu değeri koru; yoksa/boşsa hidden'ı yaz
        if (!(el.name in dto) || dto[el.name] == null || dto[el.name] === "") {
          dto[el.name] = el.value === "" ? null : el.value;
        }
      });

      // 2) base -> baseId (sadece gerekli olanlara)
      (this.controller.properties || []).forEach((p) => {
        const base = p?.name;
        if (!base) return;
        const fk = `${base}Id`;
        if (!(fk in dto)) {
          const fkEl = rowEl.querySelector(`input[name="${CSS.escape(fk)}"]`);
          if (fkEl && !fkEl.disabled) {
            dto[fk] = fkEl.value === "" ? null : fkEl.value;
          }
        }
      });
    }

    // 3) postProcess (complex'i kaldır, FK'yi normalize et)
    const finalDto =
      this.registry?.postProcessDto
        ? this.registry.postProcessDto(dto, this.controller.properties || [])
        : dto;

    // 4) id normalize & gönder
    const effectiveId = finalDto.id ?? finalDto.Id ?? rowId;
    if (effectiveId == null) throw new Error("inline update missing id");

    await this.apiClient.updateRow(
      this.controller.cfg.controller,
      effectiveId,
      finalDto
    );

    this.debugView?.log("info", "inline update success", {
      rowId: effectiveId,
      dto: finalDto,
    });
  }

  async deleteRow(rowId) {
    await this.apiClient.deleteRow(this.controller.cfg.controller, rowId);
    this.debugView?.log("info", "row deleted", { rowId });
    return true;
  }

  // ------------- MODAL RENDERING -------------

  _renderAndShowModal({ mode, props, initialData }) {
    if (this._modalEl) {
      this._modalEl.remove();
      this._modalEl = null;
    }

    const formHtml = props
      .filter((p) => !p.isPrimaryKey || mode === "edit")
      .map((p) => this._renderFieldEditor(p, initialData))
      .join("");

    const titleText = mode === "create" ? "Create New" : "Edit Row";

    // backdrop'ı biz ekleyeceğiz (fixed, yarı saydam)
    const modalHtml = `
        <div class="linq-modal-backdrop"
             style="
                position:fixed;
                inset:0;
                background:rgba(0,0,0,0.5);
                z-index:1050;
             "
             data-linq-backdrop>
        </div>

        <div class="modal show"
             tabindex="-1"
             role="dialog"
             style="
                display:block;
                z-index:1051;
             "
             data-linq-modal-root>
            <div class="modal-dialog modal-lg modal-dialog-scrollable">
                <div class="modal-content">

                    <div class="modal-header">
                        <h5 class="modal-title">${this._escape(titleText)}</h5>
                        <button type="button" class="btn-close" data-modal-close aria-label="Close"></button>
                    </div>

                   <div class="modal-body">
                      <form data-linq-form class="row g-3" novalidate>
                        ${formHtml}
                      </form>
                   </div>

                    <div class="modal-footer d-flex justify-content-between w-100">
                        <div class="text-muted small" data-save-status></div>
                        <div class="d-flex gap-2">
                            <button type="button" class="btn btn-outline-secondary btn-sm" data-modal-close>Cancel</button>
                            <button type="button" class="btn btn-primary btn-sm" data-modal-save>
                                Save
                            </button>
                        </div>
                    </div>

                </div>
            </div>
        </div>
    `;

    const wrapper = document.createElement("div");
    wrapper.innerHTML = modalHtml;

    // wrapper şu anda 2 root child içeriyor: backdrop ve modal.
    // bunları tek bir container içine almak iyi olur ki sonra kapatırken tek seferde silelim.
    const container = document.createElement("div");
    container.setAttribute("data-linq-modal-container", "1");
    container.style.position = "fixed";
    container.style.inset = "0";
    container.style.zIndex = "1050";
    container.append(...wrapper.children);

    this._modalEl = container;
    document.body.appendChild(container);

    // close
    this._modalEl.querySelectorAll("[data-modal-close]").forEach((btn) => {
      btn.addEventListener("click", () => this._closeModal());
    });
    this._modalEl
      .querySelector("[data-linq-backdrop]")
      ?.addEventListener("click", () => this._closeModal());

    // save
    const btnSave = this._modalEl.querySelector("[data-modal-save]");
    btnSave.addEventListener("click", async () => {
      await this._handleSave({ mode, props, initialData });
    });

    // 🔌 Plugin init: instance registry
    this.registry?.afterTableRender?.(this._modalEl, this.controller);
  }

  _closeModal() {
    if (this._modalEl) {
      this._modalEl.remove();
    }
    this._modalEl = null;
  }

  async _handleSave({ mode, props, initialData }) {
    if (!this._modalEl) return;

    const statusEl = this._modalEl.querySelector("[data-save-status]");
    const formEl = this._modalEl.querySelector("[data-linq-form]");
    // bu check, required ve maxlength vs attribute'lerine bakar
    if (!formEl.checkValidity()) {
      // bootstrap invalid style:
      formEl.querySelectorAll("input,select,textarea").forEach((el) => {
        if (!el.checkValidity()) {
          el.classList.add("is-invalid");
        } else {
          el.classList.remove("is-invalid");
        }
      });

      statusEl.textContent = "Please fix validation errors.";
      return;
    }
    const dtoRaw = this._readFormValues(formEl, props, initialData);
    const finalDto =
      this.registry?.postProcessDto
        ? this.registry.postProcessDto(dtoRaw, props)
        : dtoRaw;

    this.debugView?.log("info", "save payload", { mode, finalDto });

    try {
      if (mode === "create") {
        await this.apiClient.createRow(
          this.controller.cfg.controller,
          finalDto
        );
      } else {
        const effectiveId =
          finalDto.id ?? finalDto.Id ?? initialData.id ?? initialData.Id;

        if (effectiveId == null) {
          throw new Error("Cannot update row without id");
        }

        await this.apiClient.updateRow(
          this.controller.cfg.controller,
          effectiveId,
          finalDto
        );
      }

      statusEl.textContent = "Saved ✔";
      this.debugView?.log("info", "save success", { mode });

      await this.controller.refreshData();

      this._closeModal();
    } catch (err) {
      console.error("Save error:", err);
      this.debugView?.log("error", "save failed", { err: String(err) });
      statusEl.textContent = "Error: " + (err?.message || err);
    }
  }

  _formatDisplayLabel(propMeta, fallbackFieldName) {
    const explicit =
      propMeta.display?.name ||
      propMeta.display?.label ||
      propMeta.display?.title ||
      propMeta.displayName ||
      propMeta.label ||
      propMeta.title;

    if (explicit && explicit.trim() !== "") return explicit;

    if (fallbackFieldName) {
      return fallbackFieldName
        .replace(/([A-Z])/g, " $1")
        .replace(/^./, (c) => c.toUpperCase());
    }

    return "(Unnamed)";
  }

  // ------------- FIELD RENDERING -------------

  _renderFieldEditor(propMeta, initialData) {
    const fieldName = propMeta.name || propMeta.Name;
    if (!fieldName) return "";

    // RULE: bazı alanlar UI'da ayrı field olarak gösterilmesin
    if (this._isInternalLinkingField(fieldName, propMeta)) {
      // burada yine de hidden input basmak isteyebilirsin ama:
      // - Location renderer zaten kendi hidden input'unu koyuyor.
      // - Eğer başka FK alanları da böyleyse onlar için de buraya hidden koyarsın.
      return "";
    }

    const currentValue = initialData[fieldName];
    const isRequired = !!propMeta.isRequired;
    const displayLabel = this._formatDisplayLabel(propMeta, fieldName);

    const placeholderText = isRequired ? `${displayLabel} *` : displayLabel;

    // 🔌 Instance registry ile render
    const html =
      this.registry?.renderEditor?.(currentValue, propMeta, {
        fieldName,
        multiple: propMeta.kind === "complexList",
        fullRow: initialData,
        placeholderText,
      }) ??
      `
        <input
          type="text"
          class="form-control form-control-sm"
          name="${this._escape(fieldName)}"
          data-field-type="text"
          value="${this._escape(currentValue ?? "")}"
        >
      `;

    return `
        <div class="col-12 col-md-6 col-xl-4">
          <div class="mb-3">
            ${html}
            <div class="invalid-feedback small">
              Please fill out this field.
            </div>
          </div>
        </div>
    `;
  }

_isInternalLinkingField(fieldName, propMeta) {
  if (!fieldName) return false;
  const lower = fieldName.toLowerCase();

  // bilinen özel FK alanlarını gizle
  if (lower === "locationid") return true;
  if (lower.endsWith("mediaassetid")) return true;

  // genel kural: {Base}Id + {Base} (complex/complexList) birlikteyse FK alanını çizme
  if (lower.endsWith("id")) {
    const base = fieldName.slice(0, -2); // "...Id" -> "..."
    const hasComplex = (this.controller.properties || []).some(p => {
      const pn = String(p.name || p.Name || "");
      if (pn !== base) return false;
      const k = String(p.kind || "").toLowerCase();
      return k === "complex" || k === "complexlist";
    });
    if (hasComplex) return true;
  }

  return false;
}


  _buildRelationSelectedOptions(value, isMultiple) {
    if (isMultiple) {
      const arr = Array.isArray(value) ? value : [];
      return arr
        .map((item) => {
          const { id, name, displayName, title, userName } = item || {};
          const label =
            displayName || name || title || userName || id || "(?)";
          return `<option value="${this._escape(id ?? label)}" selected>${this._escape(label)}</option>`;
        })
        .join("");
    }
    if (!value || typeof value !== "object") return "";
    const { id, name, displayName, title, userName } = value || {};
    const label = displayName || name || title || userName || id || "(?)";
    return `<option value="${this._escape(id ?? label)}" selected>${this._escape(label)}</option>`;
  }

  // ------------- FORM READ -------------

  _readFormValues(formEl, props, initialData) {
    const dto = { ...(initialData || {}) };

    // 1) Mevcut mantık: props'taki field'ları oku
    props.forEach((propMeta) => {
      const fieldName = propMeta.name || propMeta.Name;
      if (!fieldName) return;

      const inputEl = formEl.querySelector(
        `[name="${CSS.escape(fieldName)}"]`
      );
      if (!inputEl) return;

      const ft = inputEl.getAttribute("data-field-type");

      if (ft === "boolean") {
        dto[fieldName] = inputEl.checked ? true : false;
        return;
      }
      if (ft === "enum") {
        if (inputEl.multiple) {
          dto[fieldName] = Array.from(inputEl.selectedOptions).map((o) =>
            this._coerceOptionValue(o.value, propMeta)
          );
        } else {
          dto[fieldName] = this._coerceOptionValue(inputEl.value, propMeta);
        }
        return;
      }
      if (ft === "relation") {
        dto[fieldName] = inputEl.multiple
          ? Array.from(inputEl.selectedOptions).map((o) => o.value)
          : inputEl.value || null;
        return;
      }
      if (ft === "location") {
        const raw = inputEl.value.trim();
        if (!raw) dto[fieldName] = null;
        else {
          const parts = raw.split(",");
          if (parts.length === 2) {
            const lat = parseFloat(parts[0]);
            const lng = parseFloat(parts[1]);
            dto[fieldName] = { lat, lng };
          } else dto[fieldName] = raw;
        }
        return;
      }
      if (ft === "upload") {
        dto[fieldName] = inputEl.files ? Array.from(inputEl.files) : [];
        return;
      }

      // default text
      dto[fieldName] = inputEl.multiple
        ? Array.from(inputEl.selectedOptions).map((o) => o.value)
        : inputEl.value;
    });

    // 2) ✅ Plugin'lerin eklediği FK hidden'ları ek olarak topla
    // (örn. MediaAsset: data-field-type="fk-hidden-media", Location: "fk-hidden")
    const fkInputs = formEl.querySelectorAll(
      'input[type="hidden"][data-field-type^="fk-hidden"]'
    );
    fkInputs.forEach((el) => {
      if (!el.name || el.disabled) return;

      // Eğer props loop'unda zaten dolu yazdıysak, onu koru;
      // yoksa/girilmediyse hidden'ın değerini kullan.
      if (!(el.name in dto) || dto[el.name] == null || dto[el.name] === "") {
        dto[el.name] = el.value === "" ? null : el.value;
      }
    });

    return dto;
  }

  // ------------- API CALLS -------------

  async _apiCreate(dto) {
    if (this.apiClient.create) {
      return await this.apiClient.create(
        this.controller.cfg.controller,
        dto
      );
    }
    // fallback generic POST
    const url = this.apiClient.buildUrl
      ? this.apiClient.buildUrl(this.controller.cfg.controller, "")
      : `${this.apiClient.apiPrefix}/${this.controller.cfg.controller}${
          this.apiClient.controllerSuffix || ""
        }`;

    const res = await fetch(url, {
      method: "POST",
      headers: this.apiClient._buildHeaders
        ? this.apiClient._buildHeaders()
        : { "Content-Type": "application/json" },
      body: JSON.stringify(dto),
    });
    if (!res.ok) throw new Error("Create failed: " + res.status);
    return await res.json().catch(() => ({}));
  }

  async _apiUpdate(dto, initialData) {
    const id = dto.id ?? dto.Id ?? initialData.id ?? initialData.Id;
    if (id == null) {
      throw new Error("Cannot update row without id");
    }

    if (this.apiClient.update) {
      return await this.apiClient.update(
        this.controller.cfg.controller,
        id,
        dto
      );
    }
    const url = this.apiClient.buildUrl
      ? this.apiClient.buildUrl(
          this.controller.cfg.controller,
          "/" + encodeURIComponent(id)
        )
      : `${this.apiClient.apiPrefix}/${this.controller.cfg.controller}${
          this.apiClient.controllerSuffix || ""
        }/${encodeURIComponent(id)}`;

    const res = await fetch(url, {
      method: "PUT",
      headers: this.apiClient._buildHeaders
        ? this.apiClient._buildHeaders()
        : { "Content-Type": "application/json" },
      body: JSON.stringify(dto),
    });
    if (!res.ok) throw new Error("Update failed: " + res.status);
    return await res.json().catch(() => ({}));
  }

  // ------------- utils -------------

  _escape(v) {
    return String(v ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;");
  }

  _coerceOptionValue(val, propMeta) {
    // enum sayısal ise sayıya çevir, aksi halde string bırak
    const tryNum = Number(val);
    if (!Number.isNaN(tryNum)) return tryNum;
    // string enum / guid vs ise olduğu gibi
    return val;
  }
}
