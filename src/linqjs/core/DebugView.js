// core/DebugView.js
//
// Görev:
// - API istekleri, controller state değişimleri, view etkileşimleri gibi olayları
//   canlı takip etmek için küçük bir konsol.
// - Yeni loglar üste yazılır.
// - Clear butonu var.
// - Max kayıt tutulur.
//
// Kullanım:
//   const dbg = new DebugView({ targetEl: ..., maxEntries: 200 });
//   dbg.log("info", "fetch started", { url, payload });
//   dbg.log("state", "page changed", { pageNumber: 2 });
//   dbg.clear();

export class DebugView {
    constructor({ targetEl, maxEntries = 200 }) {
        this.rootEl = targetEl;
        this.maxEntries = maxEntries;
        this.entries = [];
        this._renderShell();
    }

    _renderShell() {
        this.rootEl.innerHTML = `
            <div class="d-flex justify-content-between align-items-center mb-2">
                <div class="text-white-50 small">Debug Console</div>
                <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-light" data-dbg="clear">Clear</button>
                </div>
            </div>
            <div class="debug-scroll border border-secondary rounded p-2 bg-black text-success small"
                 style="max-height:220px; overflow-y:auto; font-family:monospace; white-space:pre-wrap;"
                 data-dbg="list">
            </div>
        `;

        this.listEl = this.rootEl.querySelector('[data-dbg="list"]');
        const clearBtn = this.rootEl.querySelector('[data-dbg="clear"]');
        clearBtn.addEventListener("click", () => {
            this.clear();
        });
    }

    clear() {
        this.entries = [];
        this._renderList();
    }

    log(level, message, data) {
        const timestamp = new Date().toLocaleTimeString();
        const entry = {
            ts: timestamp,
            level,
            message,
            data: data ?? null,
        };

        // yeni log en üste
        this.entries.unshift(entry);

        // limit
        if (this.entries.length > this.maxEntries) {
            this.entries.length = this.maxEntries;
        }

        this._renderList();
    }

    _renderList() {
        const colorByLevel = (lvl) => {
            switch (lvl) {
                case "error": return "text-danger";
                case "warn":  return "text-warning";
                case "state": return "text-info";
                default:      return "text-success";
            }
        };

        const html = this.entries.map(e => {
            let block = `[${e.ts}] [${e.level.toUpperCase()}] ${e.message}`;
            if (e.data) {
                block += "\n" + JSON.stringify(e.data, null, 2);
            }
            return `<div class="${colorByLevel(e.level)} mb-2"
                        style="border-bottom:1px solid rgba(255,255,255,0.1);padding-bottom:4px;">
                        ${this._escape(block)}
                    </div>`;
        }).join("");

        this.listEl.innerHTML = html;
    }

    _escape(v) {
        return String(v ?? "")
            .replace(/&/g,"&amp;")
            .replace(/</g,"&lt;")
            .replace(/>/g,"&gt;")
            .replace(/"/g,"&quot;")
            .replace(/'/g,"&#039;");
    }
}
