// core/RelationInlineGrid.js
import { LinqDataTable } from "./linqDataTable.js";

export class RelationInlineGrid {
    constructor({
        root, field, baseType, controllerName, multiple,
        apiClient,
        preselected = [],
        defaultQuery = {},
        // NEW:
        initialMode = "all",            // "all" | "selectedOnly" | "custom"
        idField = "id",                 // id kolonu adı (backend'ine göre "id" / "Id")
        buildInitialQuery = null        // (args) => queryPayload
    }) {
        this.root = root;
        this.field = field;
        this.baseType = baseType;
        this.controllerName = controllerName || baseType;
        this.multiple = !!multiple;
        this.apiClient = apiClient;
        this._sel = new Map(preselected.map(x => [String(x.id), x]));
        this._table = null;
        this._defaultQuery = defaultQuery || {};
        this._initialMode = initialMode;
        this._idField = idField;
        this._buildInitialQuery = buildInitialQuery;
    }

    mount() {
        // --- shell
        const host = document.createElement("div");
        host.className = "rig-host";
        const toolbar = document.createElement("div");
        toolbar.className = "rig-toolbar";
        toolbar.innerHTML = `
      <div class="d-flex align-items-center gap-2 mb-2">
        <strong>${this.controllerName}</strong>
        <input type="text" class="form-control form-control-sm rig-search" placeholder="Ara..." style="max-width:260px">
        <div class="ms-auto d-flex align-items-center gap-2">
          <label class="form-check form-switch m-0">
            <input class="form-check-input rig-toggle-selected" type="checkbox">
            <span class="small ms-1">Sadece seçili</span>
          </label>
          <div class="small text-muted">Seçili: <span class="rig-count">${this._sel.size}</span></div>
        </div>
      </div>
    `;
        this.root.innerHTML = "";
        this.root.appendChild(toolbar);
        this.root.appendChild(host);

        // --- hidden alanlar
        let hiddenBox;
        if (this.multiple) {
            hiddenBox = document.createElement("div");
            hiddenBox.className = "rig-hidden";
            this.root.appendChild(hiddenBox);
            this._refreshHidden(hiddenBox);
        } else {
            hiddenBox = document.createElement("input");
            hiddenBox.type = "hidden";
            hiddenBox.name = `${this.field}Id`;
            hiddenBox.setAttribute("data-field-type", "fk-hidden");
            const first = [...this._sel.values()][0];
            hiddenBox.value = first?.id ?? "";
            this.root.appendChild(hiddenBox);
        }

        this._table = new LinqDataTable({
            container: host,
            // LinqDataTable ayarları:
            controller: this.controllerName,
            // dıştan gelen apiClient'tan prefix/suffix'i al:
            apiPrefix: this.apiClient.apiPrefix,
            controllerSuffix: this.apiClient.controllerSuffix,
            // (gerekirse) token/headers passthrough:
            getAuthToken: this.apiClient.getAuthToken,
            staticHeaders: this.apiClient.staticHeaders,
            // sayfa yüklendiğinde satır click bind etmek için:
            onDataLoaded: () => onPageLoaded()
        });

          this._table.init();

        // --- initial query
        const selectedIds = [...this._sel.keys()];
        const initQ = this._resolveInitialQuery({ selectedIds, phase: "mount" });
        if (initQ) this._table.setQuery ? this._table.setQuery(initQ) : (this._table.defaultQuery = { ...this._table.defaultQuery, ...initQ });

        this._table.refresh();

        // --- search
        const search = toolbar.querySelector(".rig-search");
        if (search && this._table.setQuickSearch) {
            search.addEventListener("input", e => this._table.setQuickSearch(e.target.value || ""));
        }

        // --- toggle: sadece seçili
        const toggle = toolbar.querySelector(".rig-toggle-selected");
        if (toggle) {
            toggle.checked = (this._initialMode === "selectedOnly");
            toggle.addEventListener("change", () => {
                const ids = [...this._sel.keys()];
                const q = toggle.checked
                    ? this._queryForSelectedOnly(ids)
                    : { filters: [] }; // tümünü göster
                this._applyQueryDelta(q);
            });
        }

        // --- sayfa yüklendiğinde satır seçim davranışı
        const onPageLoaded = () => {
            const rows = host.querySelectorAll('tbody tr[data-row-id]');
            rows.forEach(tr => {
                const id = String(tr.getAttribute('data-row-id'));
                tr.style.cursor = "pointer";
                tr.addEventListener("click", () => {
                    const checked = !this._sel.has(id);
                    const row = this._table.getRowById ? this._table.getRowById(id) : { id };
                    const name = row?.name ?? row?.displayName ?? row?.title ?? row?.userName ?? id;
                    if (this.multiple) {
                        if (checked) this._sel.set(id, { id, name }); else this._sel.delete(id);
                    } else {
                        this._sel.clear();
                        if (checked) this._sel.set(id, { id, name });
                    }
                    this._syncSelectionUI(host);
                    if (this.multiple) this._refreshHidden(hiddenBox); else hiddenBox.value = [...this._sel.values()][0]?.id ?? "";
                    const c = toolbar.querySelector(".rig-count"); if (c) c.textContent = String(this._sel.size);
                });

                if (this._sel.has(id)) tr.classList.add("table-active");
            });
        };

        if (this._table.on) this._table.on("pageLoaded", onPageLoaded);
        else setTimeout(onPageLoaded, 50);
    }

    // --- initial query resolver
    _resolveInitialQuery({ selectedIds, phase }) {
        if (this._initialMode === "selectedOnly") {
            return this._queryForSelectedOnly(selectedIds);
        }
        if (this._initialMode === "custom" && typeof this._buildInitialQuery === "function") {
            return this._buildInitialQuery({ selectedIds, phase, idField: this._idField, field: this.field, baseType: this.baseType });
        }
        // "all"
        return null;
    }

    _queryForSelectedOnly(ids) {
        if (!ids || !ids.length) {
            return { filters: [{ field: this._idField, op: "in", value: [] }] }; // boş set (boş liste)
        }
        return { filters: [{ field: this._idField, op: "in", value: ids }] };
    }

    _applyQueryDelta(delta) {
        if (this._table.setQuery) {
            const q = { ...(this._table.getQuery ? this._table.getQuery() : {}), ...delta };
            this._table.setQuery(q);
        } else {
            this._table.defaultQuery = { ...this._table.defaultQuery, ...delta };
        }
        this._table.refresh();
    }

    _syncSelectionUI(host) {
        const rows = host.querySelectorAll('tbody tr[data-row-id]');
        rows.forEach(tr => {
            const id = String(tr.getAttribute('data-row-id'));
            tr.classList.toggle("table-active", this._sel.has(id));
        });
    }

    _refreshHidden(hiddenBox) {
        hiddenBox.innerHTML = "";
        for (const { id } of this._sel.values()) {
            const h = document.createElement("input");
            h.type = "hidden";
            h.name = this.field;        // FormManager postProcess => *Ids
            h.value = id;
            h.setAttribute("data-field-type", "fk-hidden");
            hiddenBox.appendChild(h);
        }
    }
}
