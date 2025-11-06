// core/DataTableView.js
// Minimal, clean, open-source friendly DataTable view layer.
// - Toolbar (Refresh / Create / Columns / Query / Inspect) — icons only
// - Column chooser uses LinqSelect2 (checkbox multiselect) — no refetch
// - Table render (read / inline edit / actions)
// - Relations expand/collapse
// - Pager
//
// Strict principles:
// - Never refetch for column visibility changes; just re-render
// - Use visibleColumns if provided; otherwise fall back to server columns
// - Keep naming clean; comments concise
// - No local language strings in UI
//
// Dependencies:
//   this.registry: renderInline(value, propMeta), renderEditor(value, propMeta, opts), afterTableRender(tableEl, controller), shouldShowColumn(propMeta)
//   LinqSelect2:   new LinqSelect2({ container, renderMode:'checkbox', multiselect:true, localData, selectedItems, placeholder, onChange })
//
// ---------------------------------------------------------------------


import { LinqSelect2 } from "../ui/linqSelect2.js";

export class DataTableView {
    constructor({ targetEl, controller, registry }) {
        this.rootEl = targetEl;
        this.controller = controller;
        this.controller.attachView(this);
        this.registry = registry || controller?.registry;
        this.controlsEl = null;
        this.tableEl = null;
        this.pagerEl = null;
        this._uid = 'dtv_' + Math.random().toString(36).slice(2);
        // rowId -> Set(fieldName)
        this.expandedRelations = new Map();

        // populated by controller after init()
        this.columns = [];
        this.visibleColumns = []; // if empty => show all (except complex/hidden)
        this.properties = [];

        // column selector instance
        this._colSelect = null;
    }

    // skeleton
    render() {
        this.rootEl.innerHTML = `
      <div class="card shadow-sm">
        <div class="card-header d-flex flex-column gap-2" data-dt="controls"></div>
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-striped table-hover mb-0 align-middle" data-dt="table"></table>
          </div>
        </div>
        <div class="card-footer d-flex flex-wrap align-items-center justify-content-between gap-2" data-dt="pager"></div>
      </div>
    `;
        this.controlsEl = this.rootEl.querySelector('[data-dt="controls"]');
        this.tableEl = this.rootEl.querySelector('[data-dt="table"]');
        this.pagerEl = this.rootEl.querySelector('[data-dt="pager"]');
    }

    // -------------------------------------------------------------------
    // Toolbar + Inline Query (icons only)
    // -------------------------------------------------------------------
    renderControls(controller) {
        if (!this.controlsEl) return;

        const allCols = Array.isArray(this.columns) ? this.columns.filter(c => c !== "__actions") : [];
        const selectedCols = Array.isArray(this.visibleColumns) && this.visibleColumns.length
            ? this.visibleColumns
            : allCols;

        const queryBarHtml = `
      <div class="w-100" data-qb-inline style="display:none;">
        <div class="border rounded bg-light p-2">
          <div class="row g-2 align-items-end">
            <div class="col-12 col-md-3">
              <label class="form-label form-label-sm mb-1 small fw-semibold">Filter</label>
              <input type="text" class="form-control form-control-sm" data-qb-inline-filter placeholder="1=1">
            </div>
            <div class="col-12 col-md-3">
              <label class="form-label form-label-sm mb-1 small fw-semibold">Group By</label>
              <input type="text" class="form-control form-control-sm" data-qb-inline-groupby placeholder="">
            </div>
            <div class="col-12 col-md-3">
              <label class="form-label form-label-sm mb-1 small fw-semibold">Select</label>
              <input type="text" class="form-control form-control-sm" data-qb-inline-select placeholder="">
            </div>
            <div class="col-12 col-md-3">
              <label class="form-label form-label-sm mb-1 small fw-semibold">Includes</label>
              <input type="text" class="form-control form-control-sm" data-qb-inline-includes placeholder="relationA, relationB">
              <div class="form-text small text-muted">comma-separated relation props</div>
            </div>
                        <div class="col-12 col-md-2">
              <label class="form-label form-label-sm mb-1 small fw-semibold">Order By</label>
              <input type="text" class="form-control form-control-sm" data-qb-inline-orderby placeholder="">
            </div>
            <div class="col-12 col-md-1">
              <label class="form-label form-label-sm mb-1 small fw-semibold">DESC</label>
              <div class="form-check">
                <input class="form-check-input" type="checkbox" data-qb-inline-desc>
              </div>
            </div>
            <div class="col-12 d-flex justify-content-end flex-wrap gap-2 mt-2">
              <button class="btn btn-sm btn-outline-secondary" data-qb-inline-cancel>Close</button>
              <button class="btn btn-sm btn-primary" data-qb-inline-apply>Apply</button>
            </div>
          </div>
        </div>
      </div>
    `;

        this.controlsEl.innerHTML = `
      <div class="d-flex flex-wrap align-items-center gap-2">
        <button class="btn btn-sm btn-outline-secondary" data-action="refresh" title="Refresh">
          <i class="bi bi-arrow-clockwise"></i>
        </button>

        <button class="btn btn-sm btn-outline-primary" data-action="create" title="Create">
          <i class="bi bi-plus-lg"></i>
        </button>

        <div class="dropdown">
          <button class="btn btn-sm btn-outline-secondary" data-bs-toggle="dropdown" type="button" data-bs-auto-close="outside" title="Columns">
            <i class="bi bi-layout-three-columns"></i>
          </button>
          <div class="dropdown-menu p-2" style="min-width:260px; max-height:320px; overflow:auto;">
            <div id="colChooserHost"></div>
          </div>
        </div>

        <button class="btn btn-sm btn-outline-secondary" data-action="querytoggle" title="Query">
          <i class="bi bi-sliders2"></i>
        </button>


      </div>

      ${queryBarHtml}
    `;

        // refresh
        this.controlsEl.querySelector('[data-action="refresh"]')
            ?.addEventListener("click", () => controller.refreshData());

        // create
        this.controlsEl.querySelector('[data-action="create"]')
            ?.addEventListener("click", () => controller.openCreateForm());


        // query toggle
        const qbInlineEl = this.controlsEl.querySelector("[data-qb-inline]");
        this.controlsEl.querySelector('[data-action="querytoggle"]')
            ?.addEventListener("click", () => {
                if (!qbInlineEl) return;
                const isVisible = qbInlineEl.style.display !== "none";
                if (isVisible) { qbInlineEl.style.display = "none"; return; }

                const q = controller.query || {};
                qbInlineEl.querySelector('[data-qb-inline-filter]').value = q.filter || "1=1";
                qbInlineEl.querySelector('[data-qb-inline-groupby]').value = q.groupBy || "";
                qbInlineEl.querySelector('[data-qb-inline-select]').value = q.select || "";
                qbInlineEl.querySelector('[data-qb-inline-includes]').value =
                    (q.includes || []).map(i => i.propertyName || i.name || i.prop || "").filter(Boolean).join(", ");
                qbInlineEl.querySelector('[data-qb-inline-orderby]').value = controller.sortColumn || "";
                qbInlineEl.querySelector('[data-qb-inline-desc]').checked = !!controller.sortDesc;
                qbInlineEl.style.display = "block";
            });

        // query apply
        qbInlineEl?.querySelector('[data-qb-inline-apply]')
            ?.addEventListener("click", () => {
                const filterVal = qbInlineEl.querySelector('[data-qb-inline-filter]').value.trim() || "1=1";
                const gbVal = qbInlineEl.querySelector('[data-qb-inline-groupby]').value.trim();
                const selVal = qbInlineEl.querySelector('[data-qb-inline-select]').value.trim();
                const incRaw = qbInlineEl.querySelector('[data-qb-inline-includes]').value.trim();
                const obVal = qbInlineEl.querySelector('[data-qb-inline-orderby]').value.trim();
                const descVal = qbInlineEl.querySelector('[data-qb-inline-desc]').checked;
                const includeList = incRaw ? incRaw.split(",").map(s => s.trim()).filter(Boolean) : [];

                controller.applyQueryFromBuilder({
                    filter: filterVal,
                    groupBy: gbVal,
                    select: selVal,
                    includes: includeList.map(name => ({
                        propertyName: name,
                        pager: { pageNumber: 1, pageSize: 10 },
                        thenIncludes: []
                    })),
                    sortColumn: obVal,
                    sortDesc: !!descVal
                });

                qbInlineEl.style.display = "none";
            });

        // query cancel
        qbInlineEl?.querySelector('[data-qb-inline-cancel]')
            ?.addEventListener("click", () => qbInlineEl.style.display = "none");

        // columns select2 (checkbox multiselect)
        const colHost = this.controlsEl.querySelector('#colChooserHost');
        if (colHost) {
            const localData = allCols.map(c => ({ id: c, name: c }));
            const selectedItems = selectedCols.map(id => ({ id, name: id }));

            // destroy previous (if any)
            if (this._colSelect && typeof this._colSelect.destroy === "function") {
                this._colSelect.destroy();
            }

            this._colSelect = new LinqSelect2({
                container: colHost,
                renderMode: 'checkbox',
                multiselect: true,
                localData,
                selectedItems,
                placeholder: 'Columns',
                onChange: (items) => {
                    const next = (items || []).map(x => x.id);
                    // update both view + controller so single source of truth stays in sync
                    this.visibleColumns = next;
                    if (this.controller) this.controller.visibleColumns = next;

                    // re-render only (no refetch)
                    this.renderTable(this.controller.rows, this.controller.columns, this.controller.properties);
                    this.renderPager(this.controller.pager, this.controller.totalCount);
                }
            });
        }
    }

    // -------------------------------------------------------------------
    // Table
    // -------------------------------------------------------------------
    renderTable(rows, columns, props) {
        if (!this.tableEl) return;

        const metaBy = {};
        (props || []).forEach(p => {
            const nm = p.name || p.Name;
            if (nm) metaBy[nm] = p;
        });

        // base columns: exclude complex/complexList here; id first
        let baseCols = (columns || []).filter(colName => {
            if (colName === "__actions") return false;
            const meta = metaBy[colName];
            if (!meta) return true;
            return this.registry.shouldShowColumn(meta);
        });

        // apply visibleColumns if provided
        if (Array.isArray(this.visibleColumns) && this.visibleColumns.length > 0) {
            const vis = new Set(this.visibleColumns);
            baseCols = baseCols.filter(c => vis.has(c));
        }

        const ordered = [];
        if (baseCols.includes("id")) ordered.push("id");
        baseCols.forEach(c => { if (c !== "id") ordered.push(c); });

        const addSelect = !!(this.controller?.cfg?.features?.showSelectionColumn);
        const radioGroupName = `dt-row-select-${this._uid}`; // YENİ

        const colsWithActions = addSelect
            ? ["__select", ...ordered, "__actions"]
            : [...ordered, "__actions"];
        // header
        const headHtml = `
      <thead class="table-light">
        <tr>${colsWithActions.map(c => this._renderHeaderCell(c)).join("")}</tr>
      </thead>
    `;

        // body
        const bodyRowsHtml = (rows || []).map((row, idx) => {
            const rowId = this._getRowId(row);
            const isEditing = this._isRowEditing(row);


            const ridNorm = this._normalizeId(rowId); // YENİ
            const addSelect = !!(this.controller?.cfg?.features?.showSelectionColumn);
            const selected = ridNorm && this.controller.selectedIds.has(ridNorm); // YENİ
            const selType = (this.controller?.cfg?.features?.selectionMode === "multi") ? "checkbox" : "radio";
            const selName = selType === "radio" ? `name="${radioGroupName}"` : ""; // YENİ


            const selCell = addSelect
                ? `<td class="text-center">
       <input type="${selType}" ${selName}
              data-row-select="${this._escape(ridNorm)}"
              ${selected ? "checked" : ""}>
     </td>`
                : "";

            const cells = ordered.map(col => {
                const m = metaBy[col] || {};
                const v = row ? row[col] : "";
                return isEditing
                    ? `<td>${this.renderEditorCell(v, m, col, row)}</td>`
                    : `<td>${this.renderInlineCell(v, m, col, row)}</td>`;
            }).join("");

            const actions = `<td class="text-end">${this._renderActionsCell(row, isEditing)}</td>`;
            const selectedCls = ridNorm && this.controller.selectedIds.has(ridNorm) ? "table-active" : "";

            let html = `
        <tr class="${selectedCls}" data-row="${idx}" data-row-id="${this._escape(rowId)}">
          ${selCell}${cells}${actions}
        </tr>
      `;

            // expanded relation panels (field-level)
            const expanded = this.expandedRelations.get(String(rowId));
            if (expanded && expanded.size > 0) {
                const complexProps = (this.properties || this.controller.properties || [])
                    .filter(p => p.kind === "complex" || p.kind === "complexList");
                complexProps.forEach(p => {
                    const fname = p.name || p.Name;
                    if (!expanded.has(fname)) return;
                    html += `
            <tr class="bg-light" data-row-child-for="${this._escape(rowId)}" data-rel-field="${this._escape(fname)}">
              <td colspan="${colsWithActions.length}" class="p-3 border-top">
                ${this._renderSingleRelationPanel(row, p)}
              </td>
            </tr>
          `;
                });
            }

            return html;
        }).join("");

        this.tableEl.innerHTML = headHtml + `<tbody>${bodyRowsHtml}</tbody>`;

        // interactions
        this.tableEl.querySelectorAll("th[data-sort]").forEach(th => {
            th.addEventListener("click", () => {
                const col = th.getAttribute("data-sort");
                this.controller.sortBy(col);
            });
        });

        this.tableEl.querySelectorAll("tbody tr[data-row-id]").forEach(tr => {
            tr.addEventListener("click", (ev) => {
                if (this._isClickInteractive(ev)) return;
                const rid = tr.getAttribute("data-row-id");
                if (!rid) return;
                this.controller.toggleRowSelection(rid);
            });
        });

        this.tableEl.querySelectorAll("button[data-row-edit]").forEach(btn => {
            btn.addEventListener("click", () => {
                const rid = btn.getAttribute("data-row-edit");
                this.controller.startInlineEdit(rid);
            });
        });

        this.tableEl.querySelectorAll("button[data-row-cancel]").forEach(btn => {
            btn.addEventListener("click", () => this.controller.cancelInlineEdit());
        });

        this.tableEl.querySelectorAll("button[data-row-save]").forEach(btn => {
            btn.addEventListener("click", () => {
                const rid = btn.getAttribute("data-row-save");
                const dto = this._collectInlineRowDto(rid);
                this.controller.saveInlineEdit(rid, dto);
            });
        });

        this.tableEl.querySelectorAll("button[data-row-delete]").forEach(btn => {
            btn.addEventListener("click", async () => {
                const rid = btn.getAttribute("data-row-delete");
                if (!confirm("Delete this row?")) return;
                await this.controller.formManagerAdapter.deleteRow(rid);
                await this.controller.refreshData();
            });
        });

        this.tableEl.querySelectorAll("button[data-row-relations]").forEach(btn => {
            btn.addEventListener("click", () => {
                const rid = btn.getAttribute("data-row-relations");
                const fname = btn.getAttribute("data-rel-field");
                this._toggleRelationFieldExpand(rid, fname);
                this.renderTable(this.controller.rows, this.controller.columns, this.controller.properties);
            });
        });


        + // selection inputs (only if visible)
            this.tableEl.querySelectorAll('input[data-row-select]').forEach(inp => {
                inp.addEventListener('click', (ev) => {
                    ev.stopPropagation();
                    const rid = inp.getAttribute('data-row-select');
                    const ridNorm = this._normalizeId(rid);        // YENİ
                    if (!ridNorm) return;
                    this.controller.toggleRowSelection(ridNorm);

                    if (this.controller?.cfg?.features?.selectionMode !== "multi") {
                        this.tableEl.querySelectorAll('input[data-row-select]').forEach(el => {
                            el.checked = (el === inp);
                        });
                    }
                    this.renderTable(this.controller.rows, this.controller.columns, this.controller.properties);
                });
            });

        this.tableEl.querySelectorAll("tbody tr[data-row-id]").forEach(tr => {
            tr.addEventListener("click", (ev) => {
                if (this._isClickInteractive(ev)) return;
                const rid = tr.getAttribute("data-row-id");
                const ridNorm = this._normalizeId(rid);      // YENİ
                if (!ridNorm) return;
                this.controller.toggleRowSelection(ridNorm);
            });
        });

        // custom actions
        this.tableEl.querySelectorAll('button[data-row-custom]').forEach(btn => {
            btn.addEventListener('click', async () => {
                const rid = btn.getAttribute('data-row-custom');
                const key = btn.getAttribute('data-custom-key');
                const row = (this.controller.rows || []).find(r => String(r.id ?? r.Id) === String(rid));
                const actions = (this.controller?.cfg?.customActions) || [];
                const act = actions.find(a => (a.key || a.label) === key);
                if (!act) return;
                try {
                    await act.onClick(row, this.controller.cfg.apiClient);
                    await this.controller.refreshData();
                } catch (err) {
                    this.controller._handleError(err);
                }
            });
        });

        this.registry.afterTableRender(this.tableEl, this.controller);
    }

    // header cell
    _renderHeaderCell(colName) {

        if (colName === "__select") {
            return `<th style="width:34px;"></th>`;
        }

        if (colName === "__actions") {
            return `<th class="text-end"><i class="bi bi-wrench-adjustable-circle"></i></th>`;
        }
        let icon = '';
        if (this.controller.sortColumn === colName) {
            icon = this.controller.sortDesc ? ' <i class="bi bi-sort-down-alt"></i>' : ' <i class="bi bi-sort-down"></i>';
        } else {
            icon = ' <i class="bi bi-arrow-down-up text-muted"></i>';
        }
        return `<th role="button" data-sort="${this._escape(colName)}">${this._escape(colName)}${icon}</th>`;
    }

    // cell render helpers
    renderInlineCell(value, propMeta, fieldName, fullRow) {
        return this.registry.renderInline(value, propMeta, { fieldName, fullRow });
    }

    renderEditorCell(value, propMeta, fieldName, fullRow) {
        const multiple =
            propMeta.kind === "complexList" ||
            propMeta.kind === "enumList" || false;
        return this.registry.renderEditor(value, propMeta, { fieldName, multiple, fullRow });
    }

    // actions cell
    _renderActionsCell(row, isEditing) {
        const rowId = this._getRowId(row);
        if (!rowId) return "";

        if (isEditing) {
            return `
        <div class="btn-group btn-group-sm">
          <button class="btn btn-success" data-row-save="${this._escape(rowId)}"><i class="bi bi-save"></i></button>
          <button class="btn btn-outline-secondary" data-row-cancel="${this._escape(rowId)}"><i class="bi bi-x-lg"></i></button>
        </div>
      `;
        }


        const complexProps = (this.properties || this.controller.properties || [])
            .filter(p => p.kind === "complex" || p.kind === "complexList");

        const relMenuHtml = complexProps.length ? `
      <div class="dropdown">
        <button class="btn btn-outline-secondary dropdown-toggle btn-sm" data-bs-toggle="dropdown" title="Relations">
          <i class="bi bi-diagram-3"></i>
        </button>
        <div class="dropdown-menu dropdown-menu-end p-1 small">
          ${complexProps.map(p => {
            const fname = p.name || p.Name;
            const isOpen = this._isRelationExpanded(rowId, fname);
            return `
                <button class="dropdown-item d-flex justify-content-between align-items-center"
                        data-row-relations="${this._escape(rowId)}"
                        data-rel-field="${this._escape(fname)}">
                  <span>${this._escape(fname)}</span>
                  <span>${isOpen ? "▾" : "▸"}</span>
                </button>
              `;
        }).join("")
            }
        </div>
      </div>
    ` : "";

        const customActions = (this.controller?.cfg?.customActions) || [];
        const customHtml = customActions.map(a => {
            const label = this._escape(a.label || a.key || "Action");
            const key = this._escape(a.key || label);
            return `<button class="btn btn-sm btn-outline-secondary" title="${label}" data-row-custom="${this._escape(rowId)}" data-custom-key="${key}"><i class="bi bi-lightning"></i></button>`;
        }).join(" ");

        return `
      <div class="btn-group btn-group-sm">
        <button class="btn btn-outline-primary" data-row-edit="${this._escape(rowId)}" title="Edit"><i class="bi bi-pencil"></i></button>
        <button class="btn btn-outline-danger"  data-row-delete="${this._escape(rowId)}" title="Delete"><i class="bi bi-trash"></i></button>
        ${relMenuHtml}
        ${customHtml}
      </div>
    `;
    }

    // relations helpers
    _toggleRelationFieldExpand(rowId, fieldName) {
        if (!rowId || !fieldName) return;
        const key = String(rowId);
        if (!this.expandedRelations.has(key)) {
            this.expandedRelations.set(key, new Set([fieldName]));
            return;
        }
        const set = this.expandedRelations.get(key);
        if (set.has(fieldName)) set.delete(fieldName); else set.add(fieldName);
        if (set.size === 0) this.expandedRelations.delete(key);
    }

    _isRelationExpanded(rowId, fieldName) {
        const set = this.expandedRelations.get(String(rowId));
        return !!(set && set.has(fieldName));
    }

    _renderSingleRelationPanel(row, propMeta) {
        const fieldName = propMeta.name || propMeta.Name;
        const rawVal = row[fieldName];

        if (propMeta.kind === "complexList") {
            const arr = Array.isArray(rawVal) ? rawVal : [];
            if (!arr.length) return `<div class="text-muted small fst-italic">${this._escape(fieldName)}: [0]</div>`;
            const keys = Object.keys(arr[0] || {});
            const head = keys.map(k => `<th class="small">${this._escape(k)}</th>`).join("");
            const body = arr.map(it => `<tr>${keys.map(k => `<td class="small">${this._escape(it[k])}</td>`).join("")}</tr>`).join("");
            return `
        <div class="mb-2">
          <div class="fw-bold small mb-2">${this._escape(fieldName)} (${arr.length})</div>
          <div class="border rounded p-2 bg-white">
            <table class="table table-sm table-bordered mb-0 bg-white">
              <thead class="table-light"><tr>${head}</tr></thead>
              <tbody>${body}</tbody>
            </table>
          </div>
        </div>
      `;
        }

        if (propMeta.kind === "complex") {
            if (!rawVal || typeof rawVal !== "object") {
                return `<div class="text-muted small fst-italic">${this._escape(fieldName)}: null</div>`;
            }
            const rowsHtml = Object.keys(rawVal).map(k =>
                `<tr><th class="small text-muted">${this._escape(k)}</th><td class="small">${this._escape(rawVal[k])}</td></tr>`
            ).join("");
            return `
        <div class="mb-2">
          <div class="fw-bold small mb-2">${this._escape(fieldName)}</div>
          <div class="border rounded p-2 bg-white">
            <table class="table table-sm table-bordered mb-0"><tbody>${rowsHtml}</tbody></table>
          </div>
        </div>
      `;
        }

        return `<div class="text-muted small">No data.</div>`;
    }

    // -------------------------------------------------------------------
    // Pager
    // -------------------------------------------------------------------
    renderPager(pager, totalCount) {
        if (!this.pagerEl) return;

        const pageNumber = pager.pageNumber;
        const pageSize = pager.pageSize;
        const total = totalCount || 0;
        const totalPages = Math.max(1, Math.ceil(total / pageSize));

        this.pagerEl.innerHTML = `
      <div class="d-flex flex-wrap align-items-center gap-2">
        <div><i class="bi bi-list-ol"></i> ${pageNumber} / ${totalPages} (${total})</div>
        <div class="btn-group btn-group-sm" role="group">
          <button class="btn btn-outline-secondary" data-page="${pageNumber - 1}" ${pageNumber <= 1 ? "disabled" : ""}>&laquo;</button>
          <button class="btn btn-outline-secondary" data-page="${pageNumber + 1}" ${pageNumber >= totalPages ? "disabled" : ""}>&raquo;</button>
        </div>
        <div class="d-flex align-items-center gap-1">
          <i class="bi bi-grid-3x3-gap"></i>
          <select class="form-select form-select-sm" data-pager-size style="width:auto;">
            ${[10, 20, 50, 100].map(sz => `<option value="${sz}" ${sz === pageSize ? "selected" : ""}>${sz}</option>`).join("")}
          </select>
        </div>
      </div>
    `;

        this.pagerEl.querySelectorAll("button[data-page]").forEach(btn => {
            btn.addEventListener("click", () => {
                const p = parseInt(btn.getAttribute("data-page"), 10);
                if (!Number.isNaN(p)) this.controller.goToPage(p);
            });
        });

        const sizeSel = this.pagerEl.querySelector("select[data-pager-size]");
        sizeSel?.addEventListener("change", () => {
            const newSize = sizeSel.value;
            this.controller.setPageSize(newSize);
        });
    }

    // -------------------------------------------------------------------
    // Misc
    // -------------------------------------------------------------------
    setLoading(isLoading) {
        if (isLoading) this.rootEl.classList.add("opacity-50");
        else this.rootEl.classList.remove("opacity-50");
    }

    showError(err) {
        console.error("DataTableView Error:", err);
        if (this.tableEl) {
            this.tableEl.innerHTML = `
        <tbody>
          <tr>
            <td colspan="99" class="text-danger text-center">
              ${this._escape(err?.message || "Error occurred")}
            </td>
          </tr>
        </tbody>
      `;
        }
    }

    _collectInlineRowDto(rid) {
        const tr = this.tableEl.querySelector(`tbody tr[data-row-id="${CSS.escape(rid)}"]`);
        if (!tr) return {};
        const dto = {};
        const props = this.controller.properties || [];

        props.forEach(meta => {
            const name = meta.name || meta.Name;
            if (!name) return;
            if (meta.kind === "complex" || meta.kind === "complexList") return;

            const el = tr.querySelector(`[name="${CSS.escape(name)}"]`);
            if (!el) return;

            const ft = el.getAttribute("data-field-type");
            if (ft === "boolean") { dto[name] = (el.checked === true); return; }
            if (ft === "enum") {
                if (el.multiple) {
                    dto[name] = Array.from(el.selectedOptions).map(o => {
                        const n = Number(o.value);
                        return Number.isNaN(n) ? o.value : n;
                    });
                } else {
                    const n = Number(el.value);
                    dto[name] = Number.isNaN(n) ? el.value : n;
                }
                return;
            }
            if (ft === "relation") {
                dto[name] = el.multiple
                    ? Array.from(el.selectedOptions).map(o => o.value)
                    : (el.value || null);
                return;
            }
            if (ft === "location") {
                const raw = el.value.trim();
                if (!raw) dto[name] = null;
                else {
                    const parts = raw.split(",");
                    if (parts.length === 2) {
                        const lat = parseFloat(parts[0]); const lng = parseFloat(parts[1]);
                        dto[name] = { lat, lng };
                    } else dto[name] = raw;
                }
                return;
            }
            if (ft === "fk-hidden") { dto[name] = el.value; return; }
            if (ft === "upload") { dto[name] = el.files ? Array.from(el.files) : []; return; }

            if (el.multiple) dto[name] = Array.from(el.selectedOptions).map(o => o.value);
            else dto[name] = el.value;
        });

        if (dto.id == null && dto.Id == null) {
            const n = Number(rid);
            dto.id = Number.isNaN(n) ? rid : n;
        }
        return dto;
    }

    // helpers
    _getRowId(row) { return row && (row.Id ?? row.id); }
    _normalizeId(v) { return v == null ? "" : String(v); }
    _isRowEditing(row) { return this._normalizeId(this.controller.editingRowId) === this._normalizeId(this._getRowId(row)); }

    _isClickInteractive(ev) {
        const t = ev?.target;
        if (!t) return false;
        const onBtn = t.closest('button[data-row-edit],button[data-row-delete],button[data-row-relations],button[data-row-save],button[data-row-cancel],button[data-row-custom]');
        if (onBtn) return true;
        const onEditor = t.closest("input,select,textarea,[contenteditable='true']");
        return !!onEditor;
    }

    _escape(v) {
        return String(v ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }
}
