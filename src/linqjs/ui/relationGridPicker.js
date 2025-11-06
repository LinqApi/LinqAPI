
import { LinqDataTable } from "./linqDataTable.js";

export class RelationGridPicker {
    constructor({ baseType, controllerName, mode, apiClient, onConfirm, preselected = [] }) {
        this.baseType = baseType;           // örn: "SpotifyArtist"
        this.controllerName = controllerName || baseType;
        this.mode = mode;                   // "single" | "multi"
        this.apiClient = apiClient;
        this.onConfirm = onConfirm;
        this.preselected = preselected;     // [{id,name}]
        this._sel = new Map(preselected.map(x => [String(x.id), x]));
    }

    open() {
        this._render();
        this._mountGrid();
    }

    close() {
        this._root?.remove();
        this._root = null;
        this._table = null;
    }

    _render() {
        const root = document.createElement("div");
        root.className = "rgp-modal";
        root.innerHTML = `
      <div class="rgp-backdrop"></div>
      <div class="rgp-dialog">
        <div class="rgp-head">
          <div class="rgp-title">${this.controllerName} seç</div>
          <button class="rgp-close" aria-label="close">×</button>
        </div>
        <div class="rgp-body">
          <div class="rgp-toolbar">
            <input type="text" class="rgp-search" placeholder="Ara..."/>
            <span class="rgp-count">Seçili: ${this._sel.size}</span>
          </div>
          <div class="rgp-host"></div>
        </div>
        <div class="rgp-foot">
          <button class="rgp-clear">Temizle</button>
          <div class="rgp-spacer"></div>
          <button class="rgp-cancel">Vazgeç</button>
          <button class="rgp-ok">Ekle</button>
        </div>
      </div>
      <style>
        .rgp-modal{position:fixed;inset:0;z-index:9999;display:flex;align-items:center;justify-content:center}
        .rgp-backdrop{position:absolute;inset:0;background:rgba(0,0,0,.35)}
        .rgp-dialog{position:relative;background:#fff;border-radius:12px;box-shadow:0 10px 30px rgba(0,0,0,.2);width:min(1100px,95vw);height:min(80vh,900px);display:flex;flex-direction:column;overflow:hidden}
        .rgp-head,.rgp-foot{display:flex;align-items:center;gap:8px;padding:10px 14px;border:1px solid #eee}
        .rgp-head{border-top-left-radius:12px;border-top-right-radius:12px;border-bottom:0}
        .rgp-foot{border-bottom-left-radius:12px;border-bottom-right-radius:12px;border-top:0}
        .rgp-title{font-weight:600}
        .rgp-body{padding:10px 14px;display:flex;flex-direction:column;gap:8px;min-height:0}
        .rgp-toolbar{display:flex;align-items:center;gap:10px}
        .rgp-search{flex:1;border:1px solid #ddd;border-radius:8px;padding:8px 10px}
        .rgp-count{font-size:12px;color:#666}
        .rgp-host{flex:1;min-height:0;border:1px solid #eee;border-radius:10px;overflow:hidden}
        .rgp-spacer{flex:1}
        .rgp-foot button{border:1px solid #ddd;background:#fafafa;border-radius:8px;padding:8px 12px;cursor:pointer}
        .rgp-foot .rgp-ok{background:#0d6efd;border-color:#0d6efd;color:#fff}
        .rgp-close{border:0;background:transparent;font-size:22px;cursor:pointer;padding:6px 8px}
      </style>
    `;
        document.body.appendChild(root);
        this._root = root;

        const q = s => root.querySelector(s);
        q(".rgp-close").onclick = () => this.close();
        q(".rgp-cancel").onclick = () => this.close();
        q(".rgp-clear").onclick = () => { this._sel.clear(); this._syncCount(); this._table?.clearRowSelection?.(); };
        q(".rgp-ok").onclick = () => { this.onConfirm?.(Array.from(this._sel.values())); this.close(); };
        q(".rgp-search").oninput = (e) => this._table?.setQuickSearch?.(e.target.value || "");
    }

    _syncCount() { const el = this._root.querySelector(".rgp-count"); if (el) el.textContent = `Seçili: ${this._sel.size}`; }

    _mountGrid() {
        const host = this._root.querySelector(".rgp-host");

        // Picker kendi controller’ıyla ayrı bir tablo kurar:
        this._table = new LinqDataTable({
            entity: this.baseType,
            controllerName: this.controllerName,
            apiClient: this.apiClient,
            readOnly: true,
            selection: {
                mode: this.mode, // "single" | "multi"
                isSelected: (row) => this._sel.has(String(row.id ?? row.Id)),
                onToggle: (row, checked) => {
                    const id = String(row.id ?? row.Id);
                    const name = row.name ?? row.displayName ?? row.title ?? row.userName ?? id;
                    if (this.mode === "single") this._sel.clear();
                    checked ? this._sel.set(id, { id, name }) : this._sel.delete(id);
                    this._syncCount();
                },
                preselectIds: this.preselected.map(x => String(x.id))
            },
            defaultQuery: { pageSize: 20 }
        });

        this._table.mount(host);
        this._table.refresh();
        this._table.on?.("pageLoaded", () => this._table.syncRowSelection?.());
    }
}
