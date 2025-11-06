// core/RowEditorModal.js
import { FieldRegistry } from "./FieldRegistry.js";

export class RowEditorModal {
    constructor({ controller, debugView }) {
        this.controller = controller;
        this.debugView = debugView;
        this.modalEl = null;
    }

    _ensureModal() {
        if (this.modalEl) return;
        const wrapper = document.createElement("div");
        wrapper.className = "modal fade show"; // bootstrap-compatible
        wrapper.style.display = "none";
        wrapper.innerHTML = `
            <div class="modal-dialog modal-lg modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" data-rem-title>Edit</h5>
                        <button type="button" class="btn-close" data-rem-close></button>
                    </div>
                    <div class="modal-body">
                        <form data-rem-form class="row g-3"></form>
                    </div>
                    <div class="modal-footer">
                        <button class="btn btn-secondary btn-sm" data-rem-cancel>Cancel</button>
                        <button class="btn btn-primary btn-sm" data-rem-save>Save</button>
                    </div>
                </div>
            </div>
        `;
        document.body.appendChild(wrapper);
        this.modalEl = wrapper;

        // close buttons
        this.modalEl.querySelector("[data-rem-close]").addEventListener("click", () => this._hide());
        this.modalEl.querySelector("[data-rem-cancel]").addEventListener("click", () => this._hide());
    }

    _buildFormFields(rowDataOrNull, properties) {
        // properties: controller.properties (props endpoint çıktısı)
        // rowDataOrNull: seçilen satır (edit) veya null (create)
        return properties
            .map(propMeta => {
                const fieldName = propMeta.name;
                if (!fieldName) return "";

                // bazı alanları formda göstermek istemeyebiliriz:
                // id (PK), computed, read-only relation listesini istersen sonra skip'leyebilirsin.
                // şimdilik: göster ama "id" yi create modunda disable edebiliriz.
                const value = rowDataOrNull ? rowDataOrNull[fieldName] : null;

                // multiple flag: complexList gibi olanlar
                const multiple =
                    propMeta.kind === "complexList" ||
                    /complexlist/i.test(propMeta.kind || "") ||
                    propMeta.type?.startsWith("ICollection<");

                // FieldRegistry bizim için uygun input HTML'ini veriyor
                const editorHtml = FieldRegistry.renderEditor(value, propMeta, {
                    fieldName,
                    multiple
                });

                return `
                    <div class="col-12 col-md-6">
                        <label class="form-label small fw-semibold">${fieldName}</label>
                        ${editorHtml}
                    </div>
                `;
            })
            .join("");
    }

    openEdit(rowData, properties) {
        this._ensureModal();

        this.modalEl.querySelector("[data-rem-title]").textContent = "Edit";
        const formEl = this.modalEl.querySelector("[data-rem-form]");
        formEl.innerHTML = this._buildFormFields(rowData, properties);

        this._wireSave("update", rowData);
        this._show();

        // burada ileride select2 / map / upload enhancer bağlayacağız
        this._enhanceForm(formEl);
    }

    openCreate(properties) {
        this._ensureModal();

        this.modalEl.querySelector("[data-rem-title]").textContent = "Create";
        const formEl = this.modalEl.querySelector("[data-rem-form]");
        formEl.innerHTML = this._buildFormFields(null, properties);

        this._wireSave("create", null);
        this._show();

        // yine enhance
        this._enhanceForm(formEl);
    }

    _wireSave(mode, originalRow) {
        const saveBtn = this.modalEl.querySelector("[data-rem-save]");
        saveBtn.onclick = async () => {
            const dto = this._collectDto();

            this.debugView?.log("info", "rowEditor save dto", dto);

            try {
                if (mode === "update") {
                    // id'yi DTO'ya ekle (primary key alanı backend bekliyor)
                    if (originalRow && (originalRow.id != null || originalRow.Id != null)) {
                        dto.id = originalRow.id ?? originalRow.Id;
                    }
                    await this.controller.cfg.apiClient.update(this.controller.cfg.controller, dto);
                } else {
                    await this.controller.cfg.apiClient.create(this.controller.cfg.controller, dto);
                }

                // başarılıysa tabloyu yenile
                await this.controller.refreshData();
                this._hide();
            } catch (err) {
                this.debugView?.log("error", "rowEditor save error", err);
                alert("Save failed");
            }
        };
    }

    _collectDto() {
        const dto = {};
        const formEl = this.modalEl.querySelector("[data-rem-form]");

        // 1) Var olan alanlar (senin mevcut mantığın)
        this.controller.properties.forEach(propMeta => {
            const fieldName = propMeta.name;
            if (!fieldName) return;

            const multiple =
                propMeta.kind === "complexList" ||
                /complexlist/i.test(propMeta.kind || "") ||
                propMeta.type?.startsWith("ICollection<");

            const inputEl = formEl.querySelector(`[name="${CSS.escape(fieldName)}"]`);
            if (!inputEl) return;

            const value = FieldRegistry.readEditorValue(inputEl, propMeta, {
                fieldName,
                multiple
            });

            dto[fieldName] = value;
        });

        // 2) ✅ FK hidden alanlarını ayrıca topla (Location & MediaAsset için)
        //    Prefix bazlı yaptım ki "fk-hidden" ve "fk-hidden-media" gibi tipleri kapsasın.
        const fkInputs = formEl.querySelectorAll('input[type="hidden"][data-field-type^="fk-hidden"]');
        fkInputs.forEach(el => {
            if (!el.name || el.disabled) return;
            const v = el.value;

            // Eğer DTO’da aynı isim zaten varsa ve doluysa, onu koru.
            // Boşsa (null/""), hidden’daki değeri yaz.
            if (!(el.name in dto) || dto[el.name] == null || dto[el.name] === "") {
                dto[el.name] = v === "" ? null : v;
            }
        });

        // 3) ✅ Complex → FK normalize (MediaAsset/Location post-process)
        const finalDto = FieldRegistry.postProcessDto(dto, this.controller.properties);
        return finalDto;
    }


    _enhanceForm(formEl) {
        // burası ileriki adım: actions / linqselect2 / linqlocation gibi gelişmiş widget'ları bağlayacağımız yer.
        //
        // örnek:
        // - data-field-type="relation" olan <select>'lere select2 benzeri async search bağlarız
        // - data-field-type="location" olan <input>'e harita picker bağlarız
        // - data-field-type="upload" olan <input type="file">'e upload pipeline bağlarız
        //
        // şu sprintte boş bırakıyoruz ama hook noktası hazır.
    }

    _show() {
        this.modalEl.style.display = "block";
        this.modalEl.classList.add("show");
    }

    _hide() {
        this.modalEl.classList.remove("show");
        this.modalEl.style.display = "none";
    }
}
