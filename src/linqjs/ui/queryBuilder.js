import { Query, Include, LogicalFilter, Pager } from "../core/models.js";
import { StateManager } from "../core/state.js";
import { getSuggestions } from "../core/intellisense.js";

export class QueryBuilder {
    constructor(config) {
        const defaults = {
            container: null,
            controller: "",
            intellisense: true,
            showInclude: true,
            showGroupBy: true,
            showSelect: true,
            stateManager: new StateManager({}),
            debug: false
        };

        this.cfg = { ...defaults, ...config };
        this.state = this.cfg.stateManager;
        this.container = this.cfg.container;
        this.intellisenseOn = this.cfg.intellisense;
        this.query = new Query(this.cfg.controller);
        this.properties = [];
        this.complexProperties = [];

        this.init();
    }

    log(...args) {
        if (this.cfg.debug) console.debug("[QueryBuilder]", ...args);
    }

    async init() {
        if (this.intellisenseOn) {
            await this.fetchProperties();
        }
        this.render();
    }

    async fetchProperties() {
        const url = `/api/${this.cfg.controller}/properties`;
        try {
            const res = await fetch(url);
            if (!res.ok) throw new Error("Properties fetch failed");
            const result = await res.json();
            this.properties = result.filter(p => !p.type.includes("Collection") && !p.type.startsWith("System.Object"));
            this.complexProperties = result.filter(p => p.type.includes("Collection") || p.type.includes("IEnumerable") || p.type.includes("ICollection"));
            this.log("Properties loaded:", this.properties, this.complexProperties);
        } catch (err) {
            console.warn(err);
            this.intellisenseOn = false;
        }
    }

    render() {
        this.container.innerHTML = `
            <div class="card p-3 shadow-sm mb-3">
                ${this.intellisenseOn ? `<button id="toggleIntellisenseBtn" class="btn btn-sm btn-secondary mb-3">Intellisense: ON</button>` : ''}
                
                <input id="filter-input" class="form-control mb-2" placeholder="Filter..." value="${this.query.filter.toString()}">

                ${this.cfg.showInclude ? `
                <select id="include-select" class="form-select mb-2">
                    <option value="">Include...</option>
                    ${this.complexProperties.map(p => `<option>${p.name}</option>`).join('')}
                </select>` : ''}
                
                ${this.cfg.showGroupBy ? `<input id="groupby-input" class="form-control mb-2" placeholder="GroupBy...">` : ''}
                ${this.cfg.showSelect ? `<input id="select-input" class="form-control mb-2" placeholder="Select...">` : ''}

                <input id="orderby-input" class="form-control mb-2" placeholder="OrderBy..." value="${this.query.orderBy}">
                <div class="form-check mb-3">
                    <input id="desc-input" type="checkbox" class="form-check-input" ${this.query.desc ? "checked" : ""}>
                    <label class="form-check-label">Desc?</label>
                </div>

                <button id="generate-query-btn" class="btn btn-primary">Generate Query</button>
            </div>
        `;

        this.setupEventHandlers();
    }

    setupEventHandlers() {
        const filterInput = this.container.querySelector("#filter-input");
        const includeSelect = this.container.querySelector("#include-select");
        const groupByInput = this.container.querySelector("#groupby-input");
        const selectInput = this.container.querySelector("#select-input");
        const orderByInput = this.container.querySelector("#orderby-input");
        const descInput = this.container.querySelector("#desc-input");
        const generateQueryBtn = this.container.querySelector("#generate-query-btn");
        const toggleIntellisenseBtn = this.container.querySelector("#toggleIntellisenseBtn");

        if (toggleIntellisenseBtn) {
            toggleIntellisenseBtn.onclick = () => {
                this.intellisenseOn = !this.intellisenseOn;
                toggleIntellisenseBtn.textContent = `Intellisense: ${this.intellisenseOn ? 'ON' : 'OFF'}`;
                if (this.intellisenseOn && !this.properties.length) this.fetchProperties();
            };
        }

        generateQueryBtn.onclick = () => {
            this.query.filter = new LogicalFilter("AND", [filterInput.value]);
            this.query.orderBy = orderByInput.value;
            this.query.desc = descInput.checked;
            this.query.groupBy = groupByInput?.value || "";
            this.query.select = selectInput?.value || "";

            if (includeSelect && includeSelect.value) {
                this.query.includes.push(new Include(includeSelect.value));
            }

            this.state.setState({ query: this.query });
            this.log("Generated query:", this.query);
        };

        if (this.intellisenseOn) {
            filterInput.addEventListener('keyup', (e) => {
                if (e.ctrlKey && e.code === 'Space') {
                    this.showIntellisense(filterInput);
                }
            });
        }
    }

    showIntellisense(inputEl) {
        const dropdown = document.createElement("div");
        dropdown.className = "list-group position-absolute w-100 shadow";
        dropdown.style.zIndex = 9999;

        this.properties.forEach(prop => {
            const item = document.createElement("button");
            item.type = "button";
            item.className = "list-group-item list-group-item-action";
            item.textContent = prop.name;
            item.onclick = () => {
                inputEl.value += prop.name;
                dropdown.remove();
            };
            dropdown.appendChild(item);
        });

        inputEl.parentElement.style.position = "relative";
        inputEl.parentElement.appendChild(dropdown);

        document.addEventListener('click', () => dropdown.remove(), { once: true });
    }
}
