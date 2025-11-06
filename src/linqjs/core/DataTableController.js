// core/DataTableController.js (refactored)
// - Removes visual request inspector (no-op stub kept for BC)
// - Adds transactional state updates with automatic rollback on failure
// - Keeps existing public API intact (no breaking changes)
// - Centralizes error handling; never leaves controller in a broken state

import { Pager, Query } from "./models.js";

export class DataTableController {
    constructor({
        controller,
        apiClient,
        view,
        debugView,
        onError,
        onDataLoaded,
        features,
        customActions,
        registry
    }) {
        const feat = {
            selection: false,           // ❌ kapalı
            selectionMode: "single",    // varsayılan mod (ama kapalı)
            showSelectionColumn: false, // ❌ sütun yok
            ...(features || {})
        };
        this.cfg = {
            controller,         // backend controller adı (örn "userprofileLinq")
            apiClient,          // LinqApiClient instance
            debugView,          // DebugView instance
            onError,            // hata callback
            onDataLoaded,       // data load callback
            features: feat,
            customActions: customActions || [],
        };



        this.view = view || null;
        this.registry = registry;
        // core state
        this.pager = new Pager({ pageNumber: 1, pageSize: 20 });
        this.sortColumn = null;
        this.sortDesc = false;
        this.query = new Query();
        this.expandedRelations = new Map();

        this.rows = [];
        this.totalCount = 0;

        this.properties = [];      // /props dan gelen kolon metadataları
        this.columns = [];         // kolon isimleri sıralı
        this.visibleColumns = [];  // gösterilen kolonlar

        // tablo şu anda projection modunda mı? (groupBy / select varsa true)
        this.isProjected = false;

        // QueryBuilderView instance'ı (opsiyonel — dışarıdan set edilir)
        this.queryBuilderView = null;
        this.selectedIds = new Set();
        this.editingRowId = null;
        this.expandedRowIds = new Set();

        this.lastPayload = null;
        this.lastResponse = null;

        this.loading = false;
        this.groupBy = []; // başlangıçta boş

        // eski create/edit/save/delete davranışı için köprü
        this.formManagerAdapter = null;
    }

    // ========= INIT =========
    attachView(viewInstance) {
        this.view = viewInstance;
        if (this.view) this.view.controller = this;
    }

    _dbg(level, msg, data) {
        if (this.cfg.debugView) this.cfg.debugView.log(level, msg, data || {});
    }

    async init() {
        this._dbg("state", "init start");

        const props = await this._loadProperties();
        this.properties = props || [];

        this._initColumnsFromProperties(this.properties);
        this._dbg("state", "columns initialized", {
            allColumns: this.columns,
            visibleColumns: this.visibleColumns
        });

        await this.refreshData();

        this._dbg("state", "init done", {
            columns: this.columns,
            pageNumber: this.pager.pageNumber,
            pageSize: this.pager.pageSize
        });
    }

    async _loadProperties() {
        try {
            const props = await this.cfg.apiClient.getProperties(this.cfg.controller);
            this._dbg("info", "properties loaded", { count: props.length });
            return props;
        } catch (err) {
            this._handleError(err);
            return [];
        }
    }

    _initColumnsFromProperties(props) {
        const allColsRaw = (props || [])
            .map(p => p.name)
            .filter(Boolean);

        const filteredCols = this._filterFksForComplexLocations(props, allColsRaw);

        const idFirst = [];
        if (filteredCols.includes("id")) idFirst.push("id");
        for (const c of filteredCols) if (c !== "id") idFirst.push(c);

        this.columns = idFirst;
        this.visibleColumns = [...idFirst];
    }

    _buildQueryPayload() {
        const pagerState = {
            pageNumber: this.pager.pageNumber,
            pageSize: this.pager.pageSize
        };

        const sortColumnOverride = this.sortColumn ?? null;
        const sortDescOverride = (this.sortColumn != null) ? this.sortDesc : null;

        const payload = this.query.toPayload(
            pagerState,
            sortColumnOverride,
            sortDescOverride
        );

        this.lastPayload = payload;
        return payload;
    }

    // ========= DATA LOADING =========
    async refreshData() {
        this._dbg("state", "refreshData called", {
            pageNumber: this.pager.pageNumber,
            pageSize: this.pager.pageSize,
            sortColumn: this.sortColumn,
            sortDesc: this.sortDesc
        });
        return this._loadPage();
    }

    _reorderColumnsForDisplay(cols) {
        if (!Array.isArray(cols)) return [];
        const hasId = cols.includes("id");
        if (!hasId) return cols;
        return ["id", ...cols.filter(c => c !== "id")];
    }

    async _loadPage() {
        this._setLoading(true);
        const payload = this._buildQueryPayload();
        this.lastPayload = payload;
        this._dbg("info", "fetch page", { payload });

        try {
            const pageResult = await this.cfg.apiClient.getPage(this.cfg.controller, payload);

            this.rows = pageResult.items || [];
            this.totalCount = pageResult.totalRecords ?? 0;
            this.lastResponse = pageResult;

            // projection modu
            this.isProjected = (
                (this.query.groupBy && this.query.groupBy.trim() !== "") ||
                (this.query.select && this.query.select.trim() !== "")
            );

            let runtimeCols = [];
            if (Array.isArray(pageResult.columns) && pageResult.columns.length > 0) {
                runtimeCols = pageResult.columns;
            } else if (this.rows.length > 0) {
                runtimeCols = Object.keys(this.rows[0]);
            } else {
                runtimeCols = this.columns || [];
            }

            runtimeCols = this._filterFksForComplexLocations(this.properties, runtimeCols);
            runtimeCols = this._reorderColumnsForDisplay(runtimeCols);
            this.columns = runtimeCols;

            if (!this.visibleColumns || this.visibleColumns.length === 0) {
                this.visibleColumns = [...runtimeCols];
            } else {
                this.visibleColumns = this.visibleColumns.filter(c => runtimeCols.includes(c));
                runtimeCols.forEach(c => {
                    if (!this.visibleColumns.includes(c)) this.visibleColumns.push(c);
                });
            }

            if (this.view) {
                this.view.columns = this.columns;
                this.view.visibleColumns = this.visibleColumns;
                this.view.renderTable(this.rows, this.columns, this.properties);
                this.view.renderPager(this.pager, this.totalCount);
                this.view.renderControls(this);
            }

            if (this.cfg.onDataLoaded) this.cfg.onDataLoaded(this.rows);

            this._dbg("state", "page loaded", {
                rows: this.rows.length,
                totalCount: this.totalCount,
                pager: { pageNumber: this.pager.pageNumber, pageSize: this.pager.pageSize },
                isProjected: this.isProjected,
                columns: this.columns
            });
        } catch (err) {
            // Hata burada yakalanıyor, ama artık state dışarıda rollback ile korunuyor
            this._handleError(err);
            throw err; // üstteki transactional wrapper revert edebilsin diye rethrow
        } finally {
            this._setLoading(false);
        }
    }

    // ========= TRANSACTIONAL STATE HELPERS =========
    _snapshotState() {
        // Query nesnesini referans olarak saklıyoruz (mutations yoksa güvenli)
        return {
            pager: { pageNumber: this.pager.pageNumber, pageSize: this.pager.pageSize },
            sortColumn: this.sortColumn,
            sortDesc: this.sortDesc,
            query: this.query,
            selectedIds: new Set(this.selectedIds),
            editingRowId: this.editingRowId,
            expandedRowIds: new Set(this.expandedRowIds)
        };
    }

    _restoreState(s) {
        if (!s) return;
        this.pager.pageNumber = s.pager.pageNumber;
        this.pager.pageSize = s.pager.pageSize;
        this.sortColumn = s.sortColumn;
        this.sortDesc = s.sortDesc;
        this.query = s.query;
        this.selectedIds = new Set(s.selectedIds);
        this.editingRowId = s.editingRowId;
        this.expandedRowIds = new Set(s.expandedRowIds);

        // Görünümü güncel tut (yeniden çizim)
        if (this.view) {
            this.view.renderTable(this.rows, this.columns, this.properties);
            this.view.renderPager(this.pager, this.totalCount);
            this.view.renderControls(this);
        }
    }

    async _runWithRollback(label, mutateFn) {
        const before = this._snapshotState();
        this._dbg("state", `${label}: mutate`, before);
        try {
            mutateFn();
            await this._loadPage();
        } catch (err) {
            this._dbg("warn", `${label}: rollback due to error`, { message: String(err?.message || err) });
            this._restoreState(before);
        }
    }

    // ========= SELECTION =========
    toggleRowSelection(id) {
        if (!this.cfg.features?.selection) return;

        if (this.cfg.features?.selectionMode === "single") {
            this.selectedIds = new Set([id]);
        } else {
            if (this.selectedIds.has(id)) this.selectedIds.delete(id); else this.selectedIds.add(id);
        }

        this._dbg("state", "row selection changed", { selectedIds: Array.from(this.selectedIds) });
    }

    getSelectedIds() {
        if (Array.isArray(this.selectedIds)) return this.selectedIds;
        if (this.selectedIds instanceof Set) return Array.from(this.selectedIds);
        return [];
    }

    getSelectedRowData() {
        const ids = this.getSelectedIds();
        if (!ids.length) return null;
        const firstId = ids[0];
        return (this.rows || []).find(r => (r.id != null && r.id === firstId) || (r.Id != null && r.Id === firstId)) || null;
    }

    // ========= INLINE EDIT =========
    startInlineEdit(rowIdRaw) {
        const rowId = (rowIdRaw === null || rowIdRaw === undefined) ? "" : String(rowIdRaw);
        this.editingRowId = rowId;
        this._dbg("state", "row edit start", { rowId });
        if (rowId) this.selectedIds = new Set([rowId]);
        if (this.view) {
            this.view.renderTable(this.rows, this.columns, this.properties);
        }
    }

    cancelInlineEdit() {
        this._dbg("state", "row edit cancel", { rowId: this.editingRowId });
        this.editingRowId = null;
        if (this.view) this.view.renderTable(this.rows, this.columns, this.properties);
    }

    async saveInlineEdit(rowId, editedDto) {
        this._dbg("info", "row edit save", { rowId, editedDto });
        try {
            if (!this.formManagerAdapter || !this.formManagerAdapter.saveInlineRowUpdate) {
                throw new Error("formManagerAdapter.saveInlineRowUpdate missing");
            }
            await this.formManagerAdapter.saveInlineRowUpdate(rowId, editedDto);
            this.editingRowId = null;
            await this.refreshData();
        } catch (err) {
            this._handleError(err);
        }
    }

    // ========= NESTED RELATIONS =========
    toggleRelationsExpand(rowId) {
        if (this.expandedRowIds.has(rowId)) this.expandedRowIds.delete(rowId); else this.expandedRowIds.add(rowId);
        this._dbg("state", "row relations toggle", { expanded: Array.from(this.expandedRowIds) });
        if (this.view) this.view.renderTable(this.rows, this.columns, this.properties);
    }

    // ========= COLUMN VISIBILITY =========
    toggleColumn(colName, visible) {
        if (!colName) return;
        if (visible) {
            if (!this.visibleColumns.includes(colName)) this.visibleColumns.push(colName);
        } else {
            this.visibleColumns = this.visibleColumns.filter(c => c !== colName);
        }
        this._dbg("state", "column visibility changed", { visibleColumns: this.visibleColumns });
        if (this.view) {
            this.view.columns = this.columns;
            this.view.visibleColumns = this.visibleColumns;
            this.view.renderTable(this.rows, this.columns, this.properties);
            this.view.renderControls(this);
        }
    }

    _filterFksForComplexLocations(props, colNames) {
        if (!Array.isArray(colNames)) return [];
        const complexLocationNames = (props || [])
            .filter(p => p.kind === "complex" && ((p.baseType || p.type || "").toLowerCase() === "location"))
            .map(p => (p.name || "").toLowerCase());
        const filtered = colNames.filter(col => {
            const lower = (col || "").toLowerCase();
            const isFkOfLocation = complexLocationNames.some(base => lower === base + "id");
            return !isFkOfLocation;
        });
        return filtered;
    }

    // ========= SORTING / PAGING / QUERY (SAFE) =========
    sortBy(colName) {
        if (!colName || colName === "__actions") return;
        this._runWithRollback("sortBy", () => {
            if (this.sortColumn === colName) {
                this.sortDesc = !this.sortDesc;
            } else {
                this.sortColumn = colName;
                this.sortDesc = false;
            }
        });
    }

    goToPage(p) {
        this._runWithRollback("goToPage", () => {
            let pageNum = parseInt(p, 10);
            if (Number.isNaN(pageNum) || pageNum < 1) pageNum = 1;
            this.pager.pageNumber = pageNum;
        });
    }

    setPageSize(newSize) {
        this._runWithRollback("setPageSize", () => {
            const sizeNum = parseInt(newSize, 10);
            if (!Number.isNaN(sizeNum) && sizeNum > 0) {
                this.pager.pageSize = sizeNum;
                this.pager.pageNumber = 1;
            }
        });
    }

    setQuery(q) {
        this._runWithRollback("setQuery", () => {
            if (q) this.query = q;
            this.pager.pageNumber = 1;
        });
    }

    // ========= TOP TOOLBAR =========
    openCreateForm() {
        if (this.formManagerAdapter?.openCreateForm) {
            this.cfg.debugView?.log("info", "openCreateForm called");
            this.formManagerAdapter.openCreateForm();
        } else {
            this.cfg.debugView?.log("warn", "openCreateForm missing adapter");
            console.warn("formManagerAdapter.openCreateForm not wired yet");
        }
    }

    openEditForm(rowData) {
        if (!rowData) {
            this.cfg.debugView?.log("warn", "openEditForm with no rowData");
            return;
        }
        if (this.formManagerAdapter?.openEditForm) {
            this.cfg.debugView?.log("info", "openEditForm called", { id: rowData.id ?? rowData.Id });
            this.formManagerAdapter.openEditForm(rowData);
        } else {
            const rid = rowData.id ?? rowData.Id;
            this.startInlineEdit(rid);
        }
    }

    // ========= DEBUG / INSPECTOR =========
    // Visual inspector kaldırıldı. Backward-compat için no-op stub bırakıldı.
    _showDebugModal() { /* intentionally removed (no-op) */ }

    // getDebugInfo hala var; sadece state/debug amaçlı (UI modal yok).
    getDebugInfo() {
        return {
            payload: this.lastPayload,
            response: this.lastResponse,
            pager: this.pager,
            sortColumn: this.sortColumn,
            sortDesc: this.sortDesc,
            query: this.query,
            selectedIds: this.getSelectedIds(),
            editingRowId: this.editingRowId,
            expandedRowIds: Array.from(this.expandedRowIds)
        };
    }

    // ========= MISC =========
    _setLoading(isLoading) {
        this.loading = isLoading;
        this._dbg("state", "loading", { loading: this.loading });
        if (this.view?.setLoading) this.view.setLoading(isLoading);
    }

    _handleError(err) {
        const msg = (err && err.message) ? err.message : String(err);
        this._dbg("error", "controller error", { message: msg });
        if (this.cfg.onError) this.cfg.onError(msg);
        if (this.view?.showError) this.view.showError(err);
    }

    destroy() {
        this._dbg("state", "controller destroyed");
    }

    // ========= QUERY BUILDER =========
    openQueryBuilder() {
        if (!this.queryBuilderView) {
            this._dbg("warn", "openQueryBuilder called but no queryBuilderView wired");
            return;
        }
        this.queryBuilderView.show({
            properties: this.properties,
            current: {
                filter: this.query.filter,
                includes: this.query.includes,
                groupBy: this.query.groupBy,
                select: this.query.select
            }
        });
    }

    applyQueryFromBuilder(builderResult) {
        // builderResult -> { filter, includes, groupBy, select }
        this._runWithRollback("applyQueryFromBuilder", () => {
            this.query.setFilter(builderResult.filter);
            this.query.setIncludes(builderResult.includes);
            this.query.setGroupBy(builderResult.groupBy);
            this.query.setSelect(builderResult.select);

            if (builderResult.sortColumn != null) {
                this.sortColumn = builderResult.sortColumn || null;
            }
            if (typeof builderResult.sortDesc === "boolean") {
                this.sortDesc = builderResult.sortDesc;
            }
            this.isProjected = (
                (this.query.groupBy && this.query.groupBy.trim() !== "") ||
                (this.query.select && this.query.select.trim() !== "")
            );
            this.pager.pageNumber = 1;
        });
    }

    setSort(colName, desc) {
        this._runWithRollback("setSort", () => {
            this.sortColumn = colName || null;
            this.sortDesc = !!desc;
        });
    }

    addGroupBy(colName) {
        if (!this.groupBy.includes(colName)) {
            this.groupBy.push(colName);
            this._dbg("state", "groupBy changed", { groupBy: this.groupBy });
        }
    }
}