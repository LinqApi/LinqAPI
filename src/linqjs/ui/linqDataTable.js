// components/linqDataTable.js
//
// Public API (geri uyumlu):
//   const dt = new LinqDataTable(config);
//   dt.init();
//
// config:
//   - container (DOM element)
//   - apiPrefix
//   - controllerSuffix
//   - controller
//   - beforeSend / afterReceive / beforeError
//   - getAuthToken / staticHeaders
//   - onError(errMsg)
//   - onDataLoaded(rows)
//
// Bu sınıf içeride şu modülleri birbirine bağlıyor:
//   - LinqApiClient
//   - DataTableController
//   - DataTableView
//   - DebugView
//   - FormManagerAdapter  🔥 (yeni CRUD köprüsü)

import { LinqApiClient } from "../core/linqApiClient.js";
import { DataTableController } from "../core/DataTableController.js";
import { DataTableView } from "../core/DataTableView.js";
import { DebugView } from "../core/DebugView.js";
import { FormManagerAdapter } from "../core/FormManagerAdapter.js";
import { QueryBuilderView } from "../core/QueryBuilderView.js";
import { createFieldRegistry } from "../core/FieldRegistry.js";

export class LinqDataTable {
    constructor(config) {
        this.config = config || {};
        this.registry = this.config.registry || createFieldRegistry();
        this.containerEl = this.config.container;
        if (!this.containerEl) {
            console.error("LinqDataTable: container element is required!");
        }

        // --- DebugView mount ---
        const debugPanelEl = document.getElementById("debugPanel");
        this.debugView = debugPanelEl
            ? new DebugView({ targetEl: debugPanelEl, maxEntries: 200 })
            : null;

        // --- Api client ---
        this.apiClient = new LinqApiClient({
            apiPrefix: this.config.apiPrefix,
            controllerSuffix: this.config.controllerSuffix,
            beforeSend: this.config.beforeSend,
            afterReceive: this.config.afterReceive,
            beforeError: this.config.beforeError,
            getAuthToken: this.config.getAuthToken,
            staticHeaders: this.config.staticHeaders,
            debugView: this.debugView
        });

        // --- Controller ---
const features = {
  selection: false,
  selectionMode: "single",      // "single" | "multi"
  showSelectionColumn: false,
  ...(this.config?.features || {}),
};


        this.controller = new DataTableController({
            controller: this.config.controller,
            apiClient: this.apiClient,
            view: null, // view'i hemen sonra attach edeceğiz
            debugView: this.debugView,
            registry: this.registry,
            customActions: this.config.customActions || [],
            onError: (errMsg) => {
                if (this.config.onError) this.config.onError(errMsg);
                this.debugView?.log("error", "controller onError", { errMsg });
            },
            onDataLoaded: (rows) => {
                if (this.config.onDataLoaded) this.config.onDataLoaded(rows);
                this.debugView?.log("info", "onDataLoaded", { rows: rows.length });
            },
            features
        });
this.controller.queryBuilderView = new QueryBuilderView({ controller: this.controller });

        // --- View ---
        this.view = new DataTableView({
            targetEl: this.containerEl,
            controller: this.controller,
            registry: this.registry,
            // debugView view'e pas etmek zorunda değiliz; ihtiyaç yoksa vermiyoruz
        });

        // --- Cross-link ---
        // DataTableView constructor zaten controller.attachView(this) çağırıyor
        // ama biz yine de controller.view = view güvence altında olsun istiyoruz.
        this.controller.attachView(this.view);

        // --- FormManagerAdapter (YENİ CRUD KÖPRÜSÜ) ---
        // Bu adapter eski LinqDataTable'ın form davranışını (Create/Edit),
        // relation picker, enum select, location, upload gibi şeyleri
        // yeni mimariye bağlamak için kullanılıyor.
        this.formManagerAdapter = new FormManagerAdapter({
            controller: this.controller,
            apiClient: this.apiClient,
            debugView: this.debugView,
            registry: this.registry
        });

        // Controller bundan sonra create/edit çağrılarını adapter üzerinden yönetecek.
        this.controller.formManagerAdapter = this.formManagerAdapter;

        // Artık RowEditorModal'a ihtiyacımız yok. Tüm butonlar
        // DataTableView.renderControls -> controller.openCreateForm/openEditForm
        // -> controller.formManagerAdapter.openCreateForm/openEditForm
        // hattından gidiyor.

        this.initialized = false;
    }

    async init() {
        try {
            // 1) sadece boş card / skeleton çiz
            this.view.render();

            // 2) controller.init() -> props çek, kolonları hazırla, ilk sayfayı getir
            await this.controller.init();

            // 3) view state'i controller'dan sync et
            this.view.columns = this.controller.columns;
            this.view.visibleColumns = this.controller.visibleColumns || this.controller.columns;
            this.view.properties = this.controller.properties; // renderTable için gerekli

            // 4) şimdi gerçek içerikleri çiz
            this.view.renderControls(this.controller);
            this.view.renderTable(
                this.controller.rows,
                this.controller.columns,
                this.controller.properties
            );
            this.view.renderPager(
                this.controller.pager,
                this.controller.totalCount
            );

            this.initialized = true;

            this.debugView?.log("state", "datatable init complete", {
                columns: this.controller.columns,
                totalCount: this.controller.totalCount
            });
        } catch (err) {
            console.error("LinqDataTable.init error:", err);
            this.debugView?.log("error", "datatable init error", err);
        }
    }

    refresh() {
        return this.controller.refreshData();
    }

    reload() {
        return this.controller.refreshData();
    }

    destroy() {
        try {
            this.controller.destroy();
            this.containerEl.innerHTML = "";
            this.initialized = false;
            this.debugView?.log("state", "datatable destroyed");
        } catch (err) {
            console.error("LinqDataTable.destroy error:", err);
            this.debugView?.log("error", "datatable destroy error", err);
        }
    }

    getSelectedRows() {
        // eski public API ile uyum için
        return Array.from(this.controller.selectedIds || []);
    }

    getData() {
        // backward compat
        if (this.controller.getCurrentData) {
            return this.controller.getCurrentData();
        }
        return this.controller.rows || [];
    }

    getDebugInfo() {
        return this.controller.getDebugInfo();
    }

    // dışarıdan filter set etmek için
    setQuery(q) {
        this.controller.setQuery(q);
    }
}
