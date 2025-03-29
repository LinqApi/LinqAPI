import { Query, Include, LogicalFilter, Pager } from "../core/models.js";
import { StateManager } from "../core/state.js";

let instanceCounter = 0;

export class QueryBuilder {
    constructor(config) {
        const defaults = {
            container: null,
            controller: "",
            intellisense: false,
            showInclude: true,
            showGroupBy: true,
            showSelect: true,
            applyButtonText: "Generate Query",
            stateManager: new StateManager({}),
            debug: false,
            onApplyQuery: null // optional callback when query is applied
        };

        this.cfg = { ...defaults, ...config };

        if (!this.cfg.container) {
            throw new Error("QueryBuilder requires a container element.");
        }
        if (!this.cfg.controller) {
            throw new Error("QueryBuilder requires a controller name.");
        }

        // Generate a unique instance ID for this QueryBuilder widget.
        this.instanceId = ++instanceCounter;

        // Use provided stateManager or create a new one.
        this.state = this.cfg.stateManager;
        this.container = this.cfg.container;
        this.intellisenseOn = this.cfg.intellisense;
        // Create the Query instance using the provided controller.
        this.query = new Query(this.cfg.controller);
        this.properties = [];
        this.complexProperties = [];

        // Optionally, fetch properties immediately (if needed)
        // this.fetchProperties().then(() => this.render());
        this.init();
    }

    log(...args) {
        if (this.cfg.debug) console.debug("[QueryBuilder]", ...args);
    }

    async init() {
        await this.fetchProperties();
        this.render();
    }

    async fetchProperties() {
        const url = `/api/${this.cfg.controller}/properties`;
        try {
            const res = await fetch(url);
            if (!res.ok) throw new Error("Properties fetch failed");
            const result = await res.json();
            // Convert result into an array, if needed.
            const arr = Array.isArray(result) ? result : (result.$values || result);
            // Simple properties: exclude collections and System.Object types
            this.properties = (arr || []).filter(
                (p) => !p.type.includes("Collection") && !p.type.startsWith("System.Object")
            );
            // Complex properties: those that are collections/enumerables
            this.complexProperties = (arr || []).filter(
                (p) =>
                    p.type.includes("Collection") ||
                    p.type.includes("IEnumerable") ||
                    p.type.includes("ICollection")
            );
            this.log("Properties loaded:", this.properties, this.complexProperties);
        } catch (err) {
            console.warn(err);
            this.intellisenseOn = false;
        }
    }

    render() {
        // Create unique IDs using the instanceId
        const uid = this.instanceId;
        this.container.innerHTML = `
      <div class="card p-3 shadow-sm mb-3">
        <input id="filter-input-${uid}" class="form-control mb-2" placeholder="Filter..." value="${this.query.filter.toString()}">
        ${this.cfg.showInclude ? `
          <select id="include-select-${uid}" class="form-select mb-2">
            <option value="">Include...</option>
            ${this.complexProperties.map(p => `<option>${p.name}</option>`).join('')}
          </select>` : ""}
        ${this.cfg.showGroupBy ? `<input id="groupby-input-${uid}" class="form-control mb-2" placeholder="GroupBy...">` : ""}
        ${this.cfg.showSelect ? `<input id="select-input-${uid}" class="form-control mb-2" placeholder="Select...">` : ""}
        <input id="orderby-input-${uid}" class="form-control mb-2" placeholder="OrderBy..." value="${this.query.orderBy}">
        <div class="form-check mb-3">
          <input id="desc-input-${uid}" type="checkbox" class="form-check-input" ${this.query.desc ? "checked" : ""}>
          <label class="form-check-label">Desc?</label>
        </div>
        <button id="generate-query-btn-${uid}" class="btn btn-primary">${this.cfg.applyButtonText}</button>
      </div>
    `;
        this.setupEventHandlers(uid);
    }

    setupEventHandlers(uid) {
        const filterInput = this.container.querySelector(`#filter-input-${uid}`);
        const includeSelect = this.container.querySelector(`#include-select-${uid}`);
        const groupByInput = this.container.querySelector(`#groupby-input-${uid}`);
        const selectInput = this.container.querySelector(`#select-input-${uid}`);
        const orderByInput = this.container.querySelector(`#orderby-input-${uid}`);
        const descInput = this.container.querySelector(`#desc-input-${uid}`);
        const generateQueryBtn = this.container.querySelector(`#generate-query-btn-${uid}`);

        // On filter input change: update query.filter and state.
        filterInput.addEventListener("input", () => {
            this.query.filter = new LogicalFilter("AND", [filterInput.value]);
            this.state.setState({ query: this.query });
        });

        generateQueryBtn.onclick = () => {
            // Update other query parameters
            this.query.orderBy = orderByInput.value;
            this.query.desc = descInput.checked;
            this.query.groupBy = groupByInput ? groupByInput.value : "";
            this.query.select = selectInput ? selectInput.value : "";
            if (includeSelect && includeSelect.value) {
                if (!this.query.includes) {
                    this.query.includes = [];
                }
                const includeObj = new Include(includeSelect.value, new Pager(1, 10), []);
                this.query.includes = [includeObj]; // Veya çoklu include için push()
            }

            // Update state manager with the new query.
            this.state.setState({ query: this.getQuery() });
            if (typeof this.cfg.onApplyQuery === "function") {
                this.cfg.onApplyQuery(this.query);
            }
            this.log("Generated query:", this.query);
        };
    }

    getQuery() {
        return this.query;
    }
}
