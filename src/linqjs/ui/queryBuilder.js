import { defaults, fetchProperties } from "./linqUtil.js";
import { Query, Pager } from "../core/models.js";

export class QueryBuilder {

    /**
     * @param {Object} options
     * @param {HTMLElement|string} options.container - UI container.
     * @param {StateManager} options.stateManager - Shared state manager.
     * @param {string} options.controller - Relevant controller name.
     * @param {string} [options.apiPrefix="/api"] - API prefix.
     * @param {Array} [options.properties=null] - List of properties if provided.
     */
    // QueryBuilder.js

    constructor({ container, stateManager, controller, apiPrefix = "/api", properties = null, onFilter = () => { } }) {
        this.containerElm = (typeof container === "string")
            ? document.querySelector(container)
            : container;

        if (!this.containerElm) {
            throw new Error("QueryBuilder: Geçersiz veya bulunamayan bir container belirtildi.");
        }

        this.stateManager = stateManager;
        this.controller = controller;
        this.apiPrefix = apiPrefix;
        this.onFilter = onFilter;
        this.properties = properties;

        // DÜZELTME 1: Tüm UI elemanlarını başlangıçta null olarak tanımla.
        // Bu, _render çalışmadan önce onlara erişilirse hata almayı engeller.
        this.filterInput = null;
        this.selectInput = null;
        this.groupByInput = null;
        this.orderByInput = null;
        this.descInput = null;
        this.applyBtn = null;

        // State'ten gelen başlangıç query'sini al
        this.query = this.stateManager.getState("query") || new Query(controller);

        // State değişikliklerini dinle
        this.stateManager.subscribe("query", (newQuery) => {
            if (newQuery) {
                this.query = newQuery;
                this._updateUI(); // Bu çağrı _render'dan önce gelebilir, bu yüzden _updateUI'da koruma olmalı.
            }
        });
    }

    // QueryBuilder.js içindeki _render metodunu bu yeni versiyonla değiştirin

    // QueryBuilder.js

    _render() {
        if (this.applyBtn) {
            this._updateUI();
            return;
        }

        const complexProps = (this.properties || []).filter(p => p.kind === 'complex' || p.kind === 'complexList');
        let includesHtml = '';
        if (complexProps.length > 0) {
            let optionsHtml = complexProps.map(p => `<option value="${p.name}">${p.name}</option>`).join('');
            includesHtml = `
                <div class="col-12">
                    <div class="form-floating">
                        <select class="form-select" data-qb="includesSelect" multiple style="height: 120px;"></select>
                        <label>İlişkisel Veri (Includes)</label>
                    </div>
                </div>
            `;
        }

        // Bootstrap 5'in modern form elemanları ve esnek grid yapısı ile yeni HTML
        this.containerElm.innerHTML = `
            <div class="row row-cols-1 row-cols-lg-2 g-3">
                <div class="col">
                    <div class="form-floating">
                        <input type="text" class="form-control form-control-sm" data-qb="filterInput" placeholder="Filter">
                        <label>Filter</label>
                    </div>
                </div>
                <div class="col">
                    <div class="form-floating">
                        <input type="text" class="form-control form-control-sm" data-qb="selectInput" placeholder="Select">
                        <label>Select</label>
                    </div>
                </div>
                <div class="col">
                    <div class="form-floating">
                        <input type="text" class="form-control form-control-sm" data-qb="groupByInput" placeholder="GroupBy">
                        <label>GroupBy</label>
                    </div>
                </div>
                <div class="col">
                    <div class="form-floating">
                        <input type="text" class="form-control form-control-sm" data-qb="orderByInput" placeholder="OrderBy">
                        <label>OrderBy</label>
                    </div>
                </div>
                ${includesHtml}
            </div>
            <div class="d-flex justify-content-between align-items-center mt-3">
                <div class="form-check form-switch">
                    <input class="form-check-input" type="checkbox" role="switch" data-qb="descInput" id="qb-desc-check">
                    <label class="form-check-label" for="qb-desc-check">Azalan Sırada (Desc)</label>
                </div>
                <button class="btn btn-primary" data-qb="applyBtn">
                    <i class="bi bi-funnel-fill me-2"></i>Filtreyi Uygula
                </button>
            </div>
        `;

        // Includes options'ı doldur (eğer varsa)
        if (this.containerElm.querySelector('[data-qb="includesSelect"]')) {
            this.containerElm.querySelector('[data-qb="includesSelect"]').innerHTML = complexProps.map(p => `<option value="${p.name}">${p.name}</option>`).join('');
        }

        // Elemanları class özelliklerine ata
        this.filterInput = this.containerElm.querySelector('[data-qb="filterInput"]');
        this.selectInput = this.containerElm.querySelector('[data-qb="selectInput"]');
        this.groupByInput = this.containerElm.querySelector('[data-qb="groupByInput"]');
        this.orderByInput = this.containerElm.querySelector('[data-qb="orderByInput"]');
        this.descInput = this.containerElm.querySelector('[data-qb="descInput"]');
        this.applyBtn = this.containerElm.querySelector('[data-qb="applyBtn"]');
        this.includesSelect = this.containerElm.querySelector('[data-qb="includesSelect"]');

        this.applyBtn.addEventListener("click", () => this._applyQuery());
        this._updateUI();
    }

    // _applyQuery metodunu stateManager yerine onFilter'ı çağıracak şekilde değiştirin:
    _applyQuery() {
        // DÜZELTME 3: Filtre ayarlarını doğrudan input değerlerinden al ve string olarak gönder.
        const filterString = this.filterInput.value || "1=1";
        const selectedIncludes = this.includesSelect && this.includesSelect.selectedOptions.length > 0
            ? Array.from(this.includesSelect.selectedOptions).map(opt => new Include(opt.value))
            : []; // Seçim yoksa boş dizi gönder
        const newFilterSettings = {
            filter: filterString,
            select: this.selectInput.value || "",
            groupBy: this.groupByInput.value || "",
            orderBy: this.orderByInput.value || "id",
            includes: selectedIncludes,
            desc: this.descInput.checked
        };

        // Değişiklikleri ana bileşene (LinqDataTable) bildir.
        this.onFilter(newFilterSettings);
    }

    _updateUI() {
        if (!this.filterInput) {
            this.filterInput 
        }
        // DÜZELTME 2: Arayüz elemanlarının varlığını kesin olarak kontrol et.
        // Bu, constructor'daki subscribe'ın _render'dan önce çalışması durumunda hata vermesini engeller.
        if (!this.filterInput || !this.applyBtn) {
            return;
        }

        // Query'nin bir Query sınıfı örneği olduğundan emin ol
        if (!(this.query instanceof Query)) {
            this.query = new Query(this.controller, this.query);
        }

        this.filterInput.value = this.query.filter.toString();
        this.selectInput.value = this.query.select || "";
        this.groupByInput.value = this.query.groupBy || "";
        this.orderByInput.value = this.query.orderBy || "id";
        this.descInput.checked = this.query.desc ?? true;
    }

    onQueryUpdated(newQuery) {
        // Optionally update the query manually if needed.
        this.query = newQuery;
        this.query.select = this.selectInput.value;
        this.query.groupBy = this.groupByInput.value;
        this.query.orderBy = this.orderByInput.value;
        this.query.desc = this.descInput.checked;
        this.query.pager = newQuery.Pager;
    }
}
