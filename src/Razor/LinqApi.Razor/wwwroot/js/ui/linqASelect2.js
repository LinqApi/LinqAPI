// ================================================
// Base Component: LinqASelect2Base
// ================================================
import { buildQueryFilter } from "./linqUtil.js";
import { fetchPagedData, debounce, buildFilter } from "./linqUtil.js";
/**
 * LinqASelect2Base
 *
 * Base class for Select2-like components.
 * 
 * - The constructor merges configuration and stores initial data.
 * - The init() method (which must be called after the DOM is ready)
 *   validates required properties and creates all DOM elements (input,
 *   dropdown, count, and selected container).
 * - The buildFilter() method uses a shared query utility to avoid magic strings.
 */
export class LinqASelect2Base {
    constructor(config) {
        this.cfg = {
            container: null,
            url: "",
            searchProperty: "",
            displayProperty: "name",
            valueField: "id",
            filterSuffix: "",
            pageSize: 10,
            debounceDuration: 300,
            multiselect: false,
            dataPath: ["items", "$values", "results"],
            ...config
        };
        this.data = [];
        this.selectedItems = [];
    }

    init() {
        const { container } = this.cfg;
        if (!container) throw new Error("Container is required!");

        this.wrapper = document.createElement("div");
        this.wrapper.className = "linq-select2-wrapper position-relative";
        container.appendChild(this.wrapper);

        this.inputEl = document.createElement("input");
        this.inputEl.className = "form-control";
        this.inputEl.placeholder = "Search...";
        this.wrapper.appendChild(this.inputEl);

        this.dropdownEl = document.createElement("div");
        this.dropdownEl.className = "linq-select2-dropdown position-absolute w-100 border bg-white";
        this.dropdownEl.style.display = "none";
        this.wrapper.appendChild(this.dropdownEl);

        this.attachEvents();
    }

    attachEvents() {
        this.inputEl.value = this.selectedItems.map(i =>
            Array.isArray(this.cfg.displayProperty)
                ? this.cfg.displayProperty.map(p => i[p]).join(this.cfg.separator || " - ")
                : i[this.cfg.displayProperty]
        ).join(", ");

        this.inputEl.addEventListener("focus", () => {
            if (!this.data.length) this.fetchData("");
        });

        document.addEventListener("click", (e) => {
            if (!this.wrapper.contains(e.target)) this.hideDropdown();
        });
    }

    async fetchData(query) {
        const filter = buildFilter({
            searchProperty: this.cfg.searchProperty,
            query,
            filterSuffix: this.cfg.filterSuffix
        });
        this.data = await fetchPagedData(this.cfg.url, {
            filter,
            pager: { pageNumber: 1, pageSize: this.cfg.pageSize }
        }, this.cfg.dataPath);
        this.renderDropdown();
    }

    renderDropdown() {
        this.dropdownEl.innerHTML = "";
        this.data.forEach(item => {
            const option = document.createElement("div");
            option.className = "linq-select2-option p-1";
            option.textContent = Array.isArray(this.cfg.displayProperty)
                ? this.cfg.displayProperty.map(p => item[p]).join(" - ")
                : item[this.cfg.displayProperty];

            option.onclick = () => this.selectItem(item);
            this.dropdownEl.appendChild(option);
        });
        this.dropdownEl.style.display = "block";
    }

    selectItem(item) {
        const exists = this.selectedItems.some(i => i[this.cfg.valueField] === item[this.cfg.valueField]);
        if (!exists || !this.cfg.multiselect) {
            this.selectedItems = this.cfg.multiselect ? [...this.selectedItems, item] : [item];
        }
        this.inputEl.value = this.selectedItems.map(i => i[this.cfg.displayProperty]).join(", ");
        this.hideDropdown();
    }

    hideDropdown() {
        this.dropdownEl.style.display = "none";
    }

    getValue() {
        return this.cfg.multiselect ? this.selectedItems : this.selectedItems[0] || null;
    }
}

/**
 * LinqASelect2Server
 *
 * Server-side implementation: fetches data from the backend.
 */
export class LinqASelect2Server extends LinqASelect2Base {
    async fetchData(query) {
        const filter = this.buildFilter(query);
        const payload = {
            filter: filter,
            pager: { pageNumber: 1, pageSize: this.cfg.pageSize }
        };
        try {
            const res = await fetch(this.cfg.url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });
            const result = await res.json();
            this.data = result.items || result.$values || [];
            const totalCount = result.totalRecords || this.data.length;
            if (this.cfg.showCount) {
                this.countEl.textContent = `${this.cfg.countLabel}${totalCount}`;
            }
            this.renderDropdown();
        } catch (err) {
            console.error("LinqASelect2Server fetchData error:", err);
        }
    }
}

export class LinqASelect2ServerGrid extends LinqASelect2Base {
    selectItem(item) {
        super.selectItem(item);
        this.renderSelectedItems();
    }

    renderSelectedItems() {
        if (this.selectedContainer) this.selectedContainer.remove();
        this.selectedContainer = document.createElement("div");
        this.selectedContainer.className = "linq-selected-items mt-2";

        this.selectedItems.forEach(item => {
            const div = document.createElement("div");
            div.textContent = Array.isArray(this.cfg.displayProperty)
                ? this.cfg.displayProperty.map(p => item[p]).join(this.cfg.separator || " - ")
                : item[this.cfg.displayProperty];
            this.selectedContainer.appendChild(div);
        });

        this.wrapper.appendChild(this.selectedContainer);
    }
}

/**
 * LinqASelect2Client
 *
 * Client-side implementation: filters using local data.
 */
export class LinqASelect2Client extends LinqASelect2Base {
    async fetchData(query) {
        if (!this.cfg.localData) {
            console.warn("LinqASelect2Client: localData is not provided.");
            return;
        }
        const lowerQuery = query.toLowerCase();
        this.data = this.cfg.localData.filter(item => {
            const value = String(item[this.cfg.searchProperty] || "").toLowerCase();
            return value.indexOf(lowerQuery) !== -1;
        });
        if (this.cfg.showCount) {
            this.countEl.textContent = `Total: ${this.data.length}`;
        }
        this.renderDropdown();
    }
}

/**
 * LinqASelect2Grid
 *
 * Grid rendering variant.
 */
export class LinqASelect2Grid extends LinqASelect2Base {
    updateSelectedUI() {
        // Sadece seçim gösterimini güncelle, wrapper içindeki input/dropdown'ı asla silme
        if (!this.wrapper) return;

        // Daha önce eklenmiş seçim listesi varsa sil
        const existing = this.wrapper.querySelector(".linq-selected-items");
        if (existing) existing.remove();

        const selectionDiv = document.createElement("div");
        selectionDiv.className = "linq-selected-items mt-2";

        this.selectedItems.forEach(item => {
            const div = document.createElement("div");
            div.className = "linq-selected-item";
            div.style.padding = "2px";
            if (Array.isArray(this.cfg.displayProperty)) {
                div.textContent = this.cfg.displayProperty.map(p => item[p]).join(" - ");
            } else {
                div.textContent = item[this.cfg.displayProperty];
            }
            selectionDiv.appendChild(div);
        });

        this.wrapper.appendChild(selectionDiv);
    }


}

/**
 * LinqASelect2Tag
 *
 * Tag rendering variant.
 */
export class LinqASelect2Tag extends LinqASelect2Base {
    updateSelectedUI() {
        this.selectedContainer.innerHTML = "";
        this.selectedItems.forEach(item => {
            const tag = document.createElement("span");
            tag.className = "linq-select2-tag badge bg-secondary me-1";
            tag.style.cursor = "default";
            if (Array.isArray(this.cfg.displayProperty)) {
                tag.textContent = this.cfg.displayProperty.map(prop => item[prop]).join(" - ");
            } else {
                tag.textContent = item[this.cfg.displayProperty];
            }
            this.selectedContainer.appendChild(tag);
        });
    }
}

/**
 * USAGE EXAMPLES:
 *
 * 1. Server-Side Grid Rendering:
 * 
 *    // In your HTML, include a container element:
 *    // <div id="server-grid-container"></div>
 *    const serverGridContainer = document.getElementById("server-grid-container");
 *    const select2ServerGrid = new LinqASelect2Server({
 *         container: serverGridContainer,
 *         url: "/api/Country/filterpaged",
 *         searchProperty: "Name",
 *         displayProperty: ["Name", "Code"],
 *         valueField: "Id",
 *         filterSuffix: "1=1",
 *         pageSize: 10,
 *         debounceDuration: 500,
 *         multiselect: true,
 *         showPropertyHeader: true,
 *         initialData: null,
 *         stateManager: { getState: () => ({}), setState: (s) => console.log(s), subscribe: () => {} },
 *         renderMode: "grid"
 *    });
 *    select2ServerGrid.init();
 *
 * 2. Client-Side Tag Rendering:
 *
 *    // In your HTML, include a container element:
 *    // <div id="client-tag-container"></div>
 *    const clientTagContainer = document.getElementById("client-tag-container");
 *    const localData = [
 *         { Id: 1, Name: "USA", Code: "US" },
 *         { Id: 2, Name: "Canada", Code: "CA" },
 *         { Id: 3, Name: "Mexico", Code: "MX" }
 *    ];
 *    const select2ClientTag = new LinqASelect2Client({
 *         container: clientTagContainer,
 *         searchProperty: "Name",
 *         displayProperty: "Name",
 *         valueField: "Id",
 *         filterSuffix: "",
 *         debounceDuration: 500,
 *         multiselect: true,
 *         initialData: null,
 *         localData: localData,
 *         stateManager: { getState: () => ({}), setState: (s) => console.log(s), subscribe: () => {} },
 *         renderMode: "tag",
 *         tagSeparatorRegex: /[,\s]+/   // Split on commas or whitespace.
 *    });
 *    select2ClientTag.init();
 *
 * 3. General Grid Rendering using a specialized grid variant:
 *
 *    // In your HTML, include a container element:
 *    // <div id="select2-grid-container"></div>
 *    const gridContainer = document.getElementById("select2-grid-container");
 *    const select2Grid = new LinqASelect2Grid({
 *         container: gridContainer,
 *         url: "/api/YourController/filterpaged",
 *         searchProperty: "Name",
 *         displayProperty: ["Name", "Surname"],
 *         valueField: "Id",
 *         filterSuffix: "Id>5",
 *         pageSize: 10,
 *         debounceDuration: 500,
 *         multiselect: true,
 *         showPropertyHeader: true,
 *         initialData: null,
 *         stateManager: { getState: () => ({}), setState: (s) => console.log("State updated:", s), subscribe: () => {} },
 *         renderMode: "grid"
 *    });
 *    select2Grid.init();
 *
 * 4. Tag Rendering with Bootstrap 5 styling (using the tag variant):
 *
 *    // In your HTML, include a container element:
 *    // <div id="select2-tag-container"></div>
 *    const tagContainer = document.getElementById("select2-tag-container");
 *    const select2Tag = new LinqASelect2Tag({
 *         container: tagContainer,
 *         url: "/api/YourController/filterpaged",
 *         searchProperty: "Name",
 *         displayProperty: "Name",
 *         valueField: "Id",
 *         filterSuffix: "Id>5",
 *         pageSize: 10,
 *         debounceDuration: 500,
 *         multiselect: true,
 *         showPropertyHeader: false,
 *         initialData: null,
 *         stateManager: { getState: () => ({}), setState: (s) => console.log("State updated:", s), subscribe: () => {} },
 *         renderMode: "tag",
 *         tagSeparatorRegex: /[,\s]+/
 *    });
 *    select2Tag.init();
 *
 * NOTE:
 * - These components use Bootstrap 5 CSS classes (e.g., "form-control", "table", "badge")
 *   for styling but do not depend on Bootstrap's JavaScript.
 * - The design follows SOLID principles and is modular and extensible.
 * - The static factory method approach is available if you prefer to initialize with a single call,
 *   but in this design we use instance-based initialization for better control over lifecycle.
 */