// plugins/relationPicker.js
// Generic Relation Picker (complex + complexList)
// - Controller adı her zaman baseType'tan resolve edilir (field adına bakılmaz)
// - SINGLE: {Field}Id varsa onu yazar; yoksa dto[{Field}] = { id } yazar
// - MULTI : create'te gizlenir; edit'te seçilir.
//           Form gönderiminde nested alan DTO'dan kaldırılır.
//           Save sonrası seçilen id'ler child controller'a PUT/PATCH ile FK atanarak bağlanır.
// - Location/MediaAsset gibi özel tipleri es geçer

import { LinqDataTable } from "../ui/linqDataTable.js";

function isBlank(v) {
    return v == null || (typeof v === "string" && v.trim() === "");
}

/* ---------------- helpers ---------------- */
function esc(v) {
    return String(v ?? "")
        .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;").replace(/'/g, "&#039;");
}
const escAttr = esc;

function isExcludedBaseType(baseType) {
    const t = String(baseType || "").toLowerCase();
    return t === "location" || t === "mediaasset";
}

function stripNullable(t) {
    const s = String(t || "");
    const m = s.match(/^nullable<\s*([^>]+)\s*>$/i);
    return m ? m[1] : s;
}
function isNumericType(t0) {
    const t = stripNullable(t0).toLowerCase();
    return ["int", "int32", "int64", "long", "decimal", "double", "float", "number"].includes(t);
}
function isGuidType(t0) {
    const t = stripNullable(t0).toLowerCase();
    return t === "guid" || t === "uuid";
}

function findProp(props, name) {
    const n = String(name || "");
    return (props || []).find(p => String(p.name || p.Name) === n) || null;
}

function resolveControllerName(baseType) {
    const r = window.APP_CONFIG?.controllerNameResolver;
    if (typeof r === "function") {
        try { return r(baseType) || baseType; } catch { /* noop */ }
    }
    return baseType;
}

function getParentType(ctx) {
    // entity adı için olası kaynaklar
    return ctx?.controller?.cfg?.entityName
        || ctx?.meta?.entityName
        || ctx?.controller?.entityName
        || "";
}

// FK adı çöz: 1) propMeta.link.fk 2) global resolver 3) ParentType + "Id"
function resolveLinkFk(propMeta, parentType, fieldName, baseType) {
    if (propMeta?.link?.fk) return propMeta.link.fk;
    const res = window.APP_CONFIG?.linkFkResolver;
    if (typeof res === "function") {
        try {
            const v = res(parentType, fieldName, baseType);
            if (v) return v;
        } catch { /* noop */ }
    }
    return `${parentType}Id`;
}

function coerceIdByPropType(rawId, fkPropMeta) {
    if (!fkPropMeta) return isBlank(rawId) ? null : String(rawId ?? "");
    const t = fkPropMeta.type || fkPropMeta.baseType || "";

    if (isNumericType(t)) {
        if (isBlank(rawId)) return null;
        const n = Number(rawId);
        return Number.isFinite(n) ? n : null;
    }
    if (isGuidType(t)) {
        if (isBlank(rawId)) return null;
        const s = String(rawId).trim();
        return s.length ? s : null;
    }
    return isBlank(rawId) ? null : String(rawId);
}

function coerceIdByBaseType(rawId, baseType) {
    const res = window.APP_CONFIG?.relationIdTypeResolver;
    if (typeof res === "function") {
        try {
            const ty = (res(baseType) || "").toLowerCase();
            if (ty === "number") {
                if (isBlank(rawId)) return null;
                const n = Number(rawId);
                return Number.isFinite(n) ? n : null;
            }
            return isBlank(rawId) ? null : String(rawId ?? "");
        } catch { /* noop */ }
    }
    return isBlank(rawId) ? null : String(rawId ?? "");
}

async function openRelationModal({ baseType, titleFieldName, controller, initialFilterId, mode, onChoose }) {
    // baseType -> controller
    const api = controller?.cfg?.apiClient;
    const apiPrefix = api?.apiPrefix || "";
    const ctrlSuffix = api?.controllerSuffix || "Linq";
    const ctrlName = resolveControllerName(baseType);

    const mountId = `relmodal_${Math.random().toString(36).slice(2)}`;
    const wrapper = document.createElement("div");
    wrapper.innerHTML = `
    <div class="linq-modal-backdrop" style="position:fixed;inset:0;background:rgba(0,0,0,.5);z-index:1050"></div>
    <div class="modal show" style="display:block;z-index:1051">
      <div class="modal-dialog modal-xl modal-dialog-scrollable">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">${esc(
        mode === "view" ? `${titleFieldName} Details`
            : mode === "pick-multi" ? `Pick ${titleFieldName} (multi)`
                : `Pick ${titleFieldName}`
    )}</h5>
            <button type="button" class="btn-close" data-close></button>
          </div>
          <div class="modal-body"><div id="${mountId}"></div></div>
          <div class="modal-footer">
            <div class="text-muted small me-auto" data-status></div>
            <button type="button" class="btn btn-outline-secondary btn-sm" data-close>Close</button>
            ${mode === "view" ? "" : `<button type="button" class="btn btn-primary btn-sm" data-choose>Choose</button>`}
          </div>
        </div>
      </div>
    </div>`;
    document.body.appendChild(wrapper);
    const close = () => wrapper.remove();
    wrapper.querySelectorAll("[data-close]").forEach(x => x.addEventListener("click", close));

    const enableSel = (mode === "pick-single" || mode === "pick-multi");
    const selMode = (mode === "pick-multi") ? "multi" : "single";

    const dt = new LinqDataTable({
        controller: ctrlName,
        container: wrapper.querySelector("#" + mountId),
        apiPrefix: apiPrefix,
        controllerSuffix: ctrlSuffix,
        select2Columns: true,
        features: { selection: enableSel, selectionMode: selMode, showSelectionColumn: enableSel },
        beforeSend: (p) => p,
        afterReceive: (r) => r,
        onError: (msg) => { try { wrapper.querySelector('[data-status]').textContent = msg; } catch { } }
    });
    await dt.init();

    if (initialFilterId != null && initialFilterId !== "") {
        try { dt.controller.applyQueryFromBuilder({ filter: `Id = ${String(initialFilterId)}`, groupBy: "", select: "", includes: [] }); } catch { }
    }

    if (enableSel) {
        wrapper.querySelector("[data-choose]")?.addEventListener("click", () => {
            const ids = dt.controller.getSelectedIds() || [];
            if (!ids.length) {
                const st = wrapper.querySelector('[data-status]'); if (st) st.textContent = "Please select at least one row."; return;
            }
            const rows = ids.map(id => ({
                id,
                row: (dt.controller.rows || []).find(r => String(r.id ?? r.Id) === String(id)) || null
            }));
            try { (mode === "pick-single" ? onChoose?.(rows[0]) : onChoose?.(rows)); } catch { }
            close();
        });
    }
}

/* --------------- install ---------------- */
export function install(registry) {
    if (!registry?.registerKindHandler) { console.warn("[relationPicker] registry missing APIs"); return; }

    // ---------- SINGLE: kind=complex ----------
    const ComplexSingleRenderer = {
        shouldShowColumn() { return false; },

        renderInline(value, propMeta) {
            const fieldName = propMeta?.name || propMeta?.Name || "Relation";
            const baseType = propMeta?.baseType || propMeta?.type || fieldName;
            const label = (value && (value.displayName || value.name || value.title || value.userName))
                || (value && (value.id ?? value.Id)) || "View";
            const hostId = `relview-${Math.random().toString(36).slice(2)}`;
            return `
        <div class="d-inline-flex align-items-center gap-2"
             data-relview-host="${hostId}"
             data-rel-field="${escAttr(fieldName)}"
             data-rel-type="${escAttr(baseType)}">
          <span class="badge bg-light text-dark">${esc(label)}</span>
          <button type="button" class="btn btn-outline-secondary btn-sm" data-relview-btn="${hostId}" title="View">
            <i class="bi bi-box-arrow-up-right"></i>
          </button>
        </div>`;
        },

        renderEditor(value, propMeta, opts = {}) {
            const fieldName = opts.fieldName || propMeta?.name || propMeta?.Name || "Relation";
            const baseType = propMeta?.baseType || propMeta?.type || fieldName;
            if (isExcludedBaseType(baseType)) return "";

            const fkName = `${fieldName}Id`;
            const row = opts.fullRow || {};
            const fkVal = row[fkName] ?? "";

            const display = (value && (value.displayName || value.name || value.title || value.userName)) || "";
            const hostId = `relpick-${Math.random().toString(36).slice(2)}`;

            return `
        <div class="relpick" data-relpick-host="${hostId}"
             data-rel-field="${escAttr(fieldName)}"
             data-rel-type="${escAttr(baseType)}"
             data-rel-fk="${escAttr(fkName)}">
          <label class="form-label form-label-sm mb-1 small fw-semibold">${esc(fieldName)}</label>
          <div class="input-group input-group-sm">
            <input type="text" class="form-control" value="${escAttr(display)}" placeholder="${escAttr(fieldName)}" data-relpick-display readonly>
            <button class="btn btn-outline-primary" type="button" data-relpick-browse="${hostId}">
              <i class="bi bi-search"></i>
            </button>
          </div>
          <input type="hidden" name="${escAttr(fkName)}" data-field-type="fk-hidden-relation" value="${escAttr(fkVal)}">
          <div class="form-text small text-muted">Click browse to pick a ${esc(fieldName)}.</div>
        </div>`;
        },

        afterTableRender(rootEl, controller) {
            if (!rootEl || !controller) return;

            // READ (view)
            rootEl.querySelectorAll("[data-relview-btn]").forEach(btn => {
                if (btn.__wired) return; btn.__wired = true;
                btn.addEventListener("click", async () => {
                    const host = btn.closest("[data-relview-host]");
                    const fieldName = host?.getAttribute("data-rel-field") || "Relation";
                    const baseType = host?.getAttribute("data-rel-type") || fieldName;

                    // bulunduğu satırdaki mevcut ID’yi yakala (row[fieldName].Id)
                    let currentId = null;
                    try {
                        const tr = btn.closest("tr[data-row-id]");
                        const rid = tr?.getAttribute("data-row-id");
                        const row = (controller.rows || []).find(r => String(r.id ?? r.Id) === String(rid));
                        if (row && row[fieldName]) currentId = row[fieldName].id ?? row[fieldName].Id ?? null;
                    } catch { }

                    await openRelationModal({
                        baseType,
                        titleFieldName: fieldName,
                        controller,
                        initialFilterId: currentId,
                        mode: "view",
                        onChoose: null
                    });
                });
            });

            // EDIT (pick-single)
            rootEl.querySelectorAll("[data-relpick-browse]").forEach(btn => {
                if (btn.__wired) return; btn.__wired = true;
                btn.addEventListener("click", async () => {
                    const block = btn.closest("[data-relpick-host]");
                    if (!block) return;

                    const fieldName = block.getAttribute("data-rel-field") || "Relation";
                    const baseType = block.getAttribute("data-rel-type") || fieldName;
                    const fkName = block.getAttribute("data-rel-fk") || (fieldName + "Id");
                    const hiddenFk = block.querySelector(`input[name="${CSS.escape(fkName)}"]`);
                    const displayEl = block.querySelector("[data-relpick-display]");
                    const currentId = hiddenFk?.value || null;

                    await openRelationModal({
                        baseType,
                        titleFieldName: fieldName,
                        controller,
                        initialFilterId: currentId || null,
                        mode: "pick-single",
                        onChoose: (chosen) => {
                            const fkMeta = findProp(controller.properties, fkName);
                            const coerced = fkMeta
                                ? coerceIdByPropType(chosen?.id, fkMeta)
                                : coerceIdByBaseType(chosen?.id, baseType);

                            if (hiddenFk) hiddenFk.value = (coerced == null ? "" : String(coerced));
                            if (displayEl) {
                                const lbl = (chosen?.row && (chosen.row.displayName || chosen.row.name || chosen.row.title || chosen.row.userName)) || chosen?.id || "";
                                displayEl.value = String(lbl);
                            }
                        }
                    });
                });
            });
        },

        postProcessDto(dtoRaw, props) {
            const dto = { ...dtoRaw };

            // 1) complex alanlar: {Field}Id yoksa dto[Field] = { id }
            (props || []).filter(p => String(p.kind || "").toLowerCase() === "complex").forEach(p => {
                const field = String(p.name || p.Name || "");
                if (!field) return;
                const fk = `${field}Id`;

                const hasRealFkProp = !!findProp(props, fk);
                if (!hasRealFkProp && (fk in dto)) {
                    const baseType = p.baseType || p.type || field;
                    const coerced = coerceIdByBaseType(dto[fk], baseType);
                    if (coerced === "" || coerced == null) {
                        delete dto[fk];
                    } else {
                        dto[field] = { id: coerced };
                        delete dto[fk];
                    }
                }
            });

            // 2) GERÇEK FK prop’u VARSA tipini zorla
            (props || []).forEach(p => {
                const fk = `${p.name || p.Name}Id`;
                const fkMeta = findProp(props, fk);
                if (fkMeta && (fk in dto)) {
                    const coerced = coerceIdByPropType(dto[fk], fkMeta);
                    if (coerced === "" || coerced == null) {
                        delete dto[fk];
                    } else {
                        dto[fk] = coerced;
                    }
                }
            });

            return dto;
        }
    };

    // ---------- MULTI: kind=complexList ----------
    const ComplexListRenderer = {
        shouldShowColumn() { return false; },

        renderInline(value, propMeta) {
            const fieldName = propMeta?.name || propMeta?.Name || "RelationList";
            const baseType = propMeta?.baseType || propMeta?.type || fieldName;
            const count = Array.isArray(value) ? value.length : 0;
            const hostId = `relview-${Math.random().toString(36).slice(2)}`;
            return `
        <div class="d-inline-flex align-items-center gap-2"
             data-relview-host="${hostId}"
             data-rel-field="${escAttr(fieldName)}"
             data-rel-type="${escAttr(baseType)}">
          <span class="badge bg-light text-dark">${esc(fieldName)}: ${count}</span>
          <button type="button" class="btn btn-outline-secondary btn-sm" data-relview-btn="${hostId}" title="View">
            <i class="bi bi-box-arrow-up-right"></i>
          </button>
        </div>`;
        },

        renderEditor(value, propMeta, opts = {}) {
            const fieldName = opts.fieldName || propMeta?.name || propMeta?.Name || "RelationList";
            const baseType = propMeta?.baseType || propMeta?.type || fieldName;
            if (isExcludedBaseType(baseType)) return "";

            // CREATE modunda gizle; EDIT'te göster
            const row = opts.fullRow || {};
            const isCreate = !(row.id || row.Id);
            if (isCreate) return "";

            const selected = Array.isArray(value) ? value : [];
            const hostId = `relpickm-${Math.random().toString(36).slice(2)}`;
            return `
        <div class="relpickm" data-relpickm-host="${hostId}"
             data-rel-field="${escAttr(fieldName)}"
             data-rel-type="${escAttr(baseType)}">
          <label class="form-label form-label-sm mb-1 small fw-semibold">${esc(fieldName)}</label>
          <div class="d-flex align-items-center gap-2">
            <span class="badge bg-secondary-subtle text-dark" data-relpickm-badge>${selected.length} selected</span>
            <div class="btn-group btn-group-sm">
              <button class="btn btn-outline-primary" type="button" data-relpickm-browse="${hostId}">
                <i class="bi bi-search"></i> Browse
              </button>
              <button class="btn btn-outline-secondary" type="button" data-relpickm-clear="${hostId}">
                <i class="bi bi-x-lg"></i> Clear
              </button>
            </div>
          </div>
          <!-- Seçilen id'leri formda tut (DTO'ya konmayacak; afterSubmit'te kullanılacak) -->
          <input type="hidden" name="__${escAttr(fieldName)}Ids" data-field-type="fk-hidden-list"
                 value='${escAttr(JSON.stringify(selected.map(x => x?.id ?? x?.Id).filter(Boolean)))}'>
          <!-- Opsiyonel: id->order map (drag&drop ile doldurabilirsin) -->
          <input type="hidden" name="__${escAttr(fieldName)}OrderMap" value="{}">
          <div class="form-text small text-muted">Pick multiple ${esc(fieldName)} records. They will be linked after saving.</div>
        </div>`;
        },

        afterTableRender(rootEl, controller) {
            if (!rootEl || !controller) return;

            // READ
            rootEl.querySelectorAll("[data-relview-btn]").forEach(btn => {
                if (btn.__wired) return; btn.__wired = true;
                btn.addEventListener("click", async () => {
                    const host = btn.closest("[data-relview-host]");
                    const fieldName = host?.getAttribute("data-rel-field") || "RelationList";
                    const baseType = host?.getAttribute("data-rel-type") || fieldName;
                    await openRelationModal({ baseType, titleFieldName: fieldName, controller, initialFilterId: null, mode: "view", onChoose: null });
                });
            });

            // EDIT (pick-multi)
            rootEl.querySelectorAll("[data-relpickm-browse]").forEach(btn => {
                if (btn.__wired) return; btn.__wired = true;
                btn.addEventListener("click", async () => {
                    const block = btn.closest("[data-relpickm-host]");
                    if (!block) return;

                    const fieldName = block.getAttribute("data-rel-field") || "RelationList";
                    const baseType = block.getAttribute("data-rel-type") || fieldName;
                    const hidden = block.querySelector(`input[name="__${CSS.escape(fieldName)}Ids"]`);
                    const badge = block.querySelector('[data-relpickm-badge]');

                    await openRelationModal({
                        baseType,
                        titleFieldName: fieldName,
                        controller,
                        initialFilterId: null,
                        mode: "pick-multi",
                        onChoose: (chosenArr) => {
                            const ids = (chosenArr || [])
                                .map(x => coerceIdByBaseType(x?.id, baseType))
                                .filter(v => v != null && v !== "");
                            if (hidden) hidden.value = JSON.stringify(ids);
                            if (badge) badge.textContent = `${ids.length} selected`;
                        }
                    });
                });
            });

            // CLEAR
            rootEl.querySelectorAll("[data-relpickm-clear]").forEach(btn => {
                if (btn.__wired) return; btn.__wired = true;
                btn.addEventListener("click", () => {
                    const block = btn.closest("[data-relpickm-host]");
                    if (!block) return;
                    const fieldName = block.getAttribute("data-rel-field") || "RelationList";
                    const hidden = block.querySelector(`input[name="__${CSS.escape(fieldName)}Ids"]`);
                    const ordMap = block.querySelector(`input[name="__${CSS.escape(fieldName)}OrderMap"]`);
                    const badge = block.querySelector('[data-relpickm-badge]');
                    if (hidden) hidden.value = "[]";
                    if (ordMap) ordMap.value = "{}";
                    if (badge) badge.textContent = "0 selected";
                });
            });
        },

        // IMPORTANT: complexList alanlarını DTO'dan tamamen çıkar (nested insert/update yok)
        postProcessDto(dtoRaw, props) {
            const dto = { ...dtoRaw };

            // Tüm complexList alanlarını sil
            (props || []).forEach(p => {
                if (String(p.kind || "").toLowerCase() === "complexlist") {
                    const name = String(p.name || p.Name || "");
                    if (name in dto) delete dto[name];
                    // Eski mekanizmaya ait "*Ids" alanlarını da temizle (gönderim yapmayacağız)
                    const idsKey = `${name}Ids`;
                    if (idsKey in dto) delete dto[idsKey];
                }
            });

            return dto;
        }
    };

    registry.registerKindHandler("complex", ComplexSingleRenderer);
    registry.registerKindHandler("complexlist", ComplexListRenderer);

    // Zincir sonu post-process (multi + single) + after submit hook
    registry.registerCustomType("__relationpicker_post__", {
        postProcessDto(dto, props) {
            // Sıra: önce complexList temizliği, sonra complex mapping
            let x = ComplexListRenderer.postProcessDto(dto, props);
            x = ComplexSingleRenderer.postProcessDto(x, props);
            return x;
        },

        // Save başarıyla bittiğinde complexList link'lerini child controller'a gönder
        async afterSubmitSuccess(ctx) {
            try {
                const form = ctx?.formEl;
                const controller = ctx?.controller;
                const rowId = ctx?.result?.id ?? ctx?.result?.Id ?? ctx?.row?.id ?? ctx?.row?.Id;
                if (!form || !rowId) return;

                const parentType = getParentType(ctx);
                const props = controller?.properties || [];

                // Bu formdaki tüm complexList hostlarını bul ve sırayla işle
                const hosts = form.querySelectorAll("[data-relpickm-host]");
                for (const block of hosts) {
                    const fieldName = block.getAttribute("data-rel-field");
                    const baseType = block.getAttribute("data-rel-type");
                    const propMeta = props.find(p => String(p.name || p.Name) === fieldName) || {};

                    const fkName = resolveLinkFk(propMeta, parentType, fieldName, baseType);
                    const orderProp = propMeta?.link?.order || "Order";
                    const method = (propMeta?.link?.method || "PUT").toUpperCase();

                    const idsRaw = block.querySelector(`input[name="__${CSS.escape(fieldName)}Ids"]`)?.value || "[]";
                    const mapRaw = block.querySelector(`input[name="__${CSS.escape(fieldName)}OrderMap"]`)?.value || "{}";
                    const ids = JSON.parse(idsRaw);
                    const orderMap = JSON.parse(mapRaw);

                    if (!Array.isArray(ids) || ids.length === 0) continue;

                    // API config
                    const api = controller?.cfg?.apiClient;
                    const apiPrefix = api?.apiPrefix || "";
                    const ctrlSuffix = api?.controllerSuffix || "Linq";
                    const childCtrl = resolveControllerName(baseType);

                    // Her çocuk için generic UPDATE/PATCH
                    for (const id of ids) {
                        const payload = { id };
                        payload[fkName] = rowId; // örn. StoryId
                        if (orderMap && orderMap[id] != null) payload[orderProp] = orderMap[id];

                        await fetch(`${apiPrefix}/api/${childCtrl}${ctrlSuffix}/${id}`, {
                            method,
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify(payload)
                        });
                    }
                }

                controller.toast?.success?.("Relations linked.");
            } catch (e) {
                console.error("[relationPicker.afterSubmitSuccess] link error", e);
            }
        }
    });
}

export function uninstall(registry) {
    try { registry.unregisterKindHandler?.("complex"); } catch { }
    try { registry.unregisterKindHandler?.("complexlist"); } catch { }
}
