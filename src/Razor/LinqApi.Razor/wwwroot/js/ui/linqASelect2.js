import { debounce, fetchPagedData, defaults } from "./linqutil.js";
import { Query, LogicalFilter, Pager } from "../core/models.js";

/**
 * LinqSelect2 Component
 * ----------------------
 * A reusable, agile select component that supports both server- and client-side data fetching,
 * single/multiple selection, and three render modes: "tag", "grid", and now "checkbox".
 *
 * In single-select mode, the selected item is always shown in the input field.
 * In multi-select mode:
 *   - "tag" mode displays selections as small removable tags.
 *   - "grid" mode displays selections in a table below the input with removal buttons.
 *   - "checkbox" mode displays options as a small table with checkboxes next to each item.
 *
 * The component also supports a "disabled" flag. When disabled, new selections are not allowed.
 */
export class LinqSelect2 {
    constructor(cfg) {
        // Merge default config with user-provided options
        this.cfg = {
            container: null,
            controller: "",
            apiPrefix: defaults.apiPrefix,
            searchProperty: "name",
            displayProperty: "name", // Can be a string or an array of properties
            valueField: "id",
            filterSuffix: "",
            renderMode: "tag",  // "tag" or "grid"
            fetchMode: "server", // "server" or "client"
            selectPlaceHolder: "Select...",
            localData: [],
            multiselect: false,
            pageSize: defaults.defaultPageSize,
            debounceDuration: defaults.debounceDuration,
            disabled: false,   // When true, new selections are disabled
            ...cfg,
        };
        this.selectedItems = [];
        this.init();
    }

    /**
     * Initialize the component by setting up the markup and event handlers.
     */
    init() {
        this.container = this.cfg.container;
        // Ensure the container has position relative
        this.container.style.position = "relative";
        window.addEventListener("resize", () => {
            // For instance, you might reapply styles or recalc the top position if needed.
            // Here we assume top remains "100%" relative to the input.
            this.dropdown.style.top = "100%";
        });

        // Set up the component's markup inside a wrapper
        this.container.innerHTML = `
        <div class="select2-wrapper" style="position: relative;">
            <input class="form-control mb-1" placeholder="${this.cfg.placeholder || 'Select...'}" />
            <div class="dropdown-menu w-100"></div>
        </div>
        <div class="selected-items mt-2"></div>
    `;
        const wrapper = this.container.querySelector(".select2-wrapper");
        [this.input, this.dropdown] = wrapper.children;
        // The selectedContainer remains outside the wrapper if desired.
        this.selectedContainer = this.container.querySelector(".selected-items");

        // Set dropdown absolute positioning
        Object.assign(this.dropdown.style, {
            position: "absolute",
            top: "100%",
            left: "0",
            zIndex: "1000", // ensure it appears on top of other elements
        });

        // If disabled, disable input and do not bind selection events.
        if (this.cfg.disabled) {
            this.input.disabled = true;
            this.dropdown.classList.remove("show");
        } else {
            // Bind search input events with debouncing.
            this.input.oninput = debounce(() => this.fetchData(this.input.value), this.cfg.debounceDuration);
            this.input.onfocus = () => this.fetchData("");
            document.addEventListener("click", (e) => {
                if (!this.container.contains(e.target)) {
                    this.dropdown.classList.remove("show");
                }
            });
        }
    }


    /**
     * Fetch data either from the server or from localData based on the fetchMode.
     * @param {string} searchQuery - The search string entered by the user.
     */
    async fetchData(searchQuery) {
        if (this.cfg.fetchMode === "server") {
            const url = `${this.cfg.apiPrefix}/${this.cfg.controller}/${defaults.filterPagedRoute}`;
            const filterSegments = [];

            if (searchQuery) {
                filterSegments.push(`${this.cfg.searchProperty}.Contains("${searchQuery}")`);
            }
            if (this.cfg.filterSuffix) {
                filterSegments.push(this.cfg.filterSuffix);
            }

            const filter = new LogicalFilter("AND", filterSegments);
            const query = new Query(this.cfg.controller, {
                filter,
                pager: new Pager(1, this.cfg.pageSize),
                orderBy: this.cfg.valueField,
                desc: false,
            });

            // Expecting the server response to have an "items" property.
            this.data = await fetchPagedData(url, query);
        } else {
            // Client-side filtering on localData.
            const q = searchQuery.toLowerCase();
            this.data = {
                items: this.cfg.localData.filter(i =>
                    i[this.cfg.searchProperty].toLowerCase().includes(q)
                )
            };
        }
        this.renderDropdown();
    }

    /**
     * Render the dropdown list with the fetched data.
     * Uses getDisplayText() to build the display string for each item.
     */
    renderDropdown() {
        // Check for the new "checkbox" render mode
        if (this.cfg.renderMode === "checkbox") {
            // Render the dropdown as a small table with checkboxes
            let html = '<table class="table table-sm mb-0"><tbody>';
            this.data.items.forEach((item, index) => {
                const displayText = this.getDisplayText(item);
                // Check if the item is already selected
                const isChecked = this.selectedItems.some(i => i[this.cfg.valueField] === item[this.cfg.valueField]);
                html += `
                <tr>
                    <td style="width: 1%;">
                        <input type="checkbox" data-index="${index}" ${isChecked ? "checked" : ""} />
                    </td>
                    <td>${displayText}</td>
                </tr>
            `;
            });
            html += '</tbody></table>';
            this.dropdown.innerHTML = html;

            // Attach event listeners to each checkbox to update the selection on change
            const checkboxes = this.dropdown.querySelectorAll("input[type='checkbox']");
            checkboxes.forEach(checkbox => {
                checkbox.addEventListener("change", (e) => {
                    const index = e.target.getAttribute("data-index");
                    const item = this.data.items[index];
                    if (e.target.checked) {
                        // Add the item if it's not already selected
                        if (!this.selectedItems.some(i => i[this.cfg.valueField] === item[this.cfg.valueField])) {
                            this.selectedItems.push(item);
                        }
                    } else {
                        // Remove the item from the selected items
                        this.selectedItems = this.selectedItems.filter(i => i[this.cfg.valueField] !== item[this.cfg.valueField]);
                    }
                    // Trigger onChange callback if provided in the config
                    if (typeof this.cfg.onChange === "function") {
                        this.cfg.onChange(this.getValue());
                    }
                });
            });
            this.dropdown.classList.add("show");
        } else {
            // Fallback to the existing render implementation for "tag" or "grid" modes
            this.dropdown.innerHTML = this.data.items.map(item => {
                const displayText = this.getDisplayText(item);
                return `<a class="dropdown-item">${displayText}</a>`;
            }).join("");
            Array.from(this.dropdown.children).forEach((el, i) => {
                el.onclick = () => this.selectItem(this.data.items[i]);
            });
            this.dropdown.classList.add("show");
        }
    }


    /**
     * Handle item selection.
     * For single-select mode, the selected item replaces any existing selection.
     * For multi-select mode, the item is added if not already selected.
     * If the component is disabled, no action is taken.
     * @param {object} item - The item being selected.
     */
    selectItem(item) {
        if (this.cfg.disabled) return; // Do nothing if component is disabled

        if (this.cfg.multiselect) {
            // In multi-select, add the item if it's not already selected.
            if (!this.selectedItems.some(i => i[this.cfg.valueField] === item[this.cfg.valueField])) {
                this.selectedItems.push(item);
            }
        } else {
            // In single-select, replace the current selection.
            this.selectedItems = [item];
        }
        this.updateSelectedUI();
        // For single-select mode, hide the dropdown.
        if (!this.cfg.multiselect) {
            this.dropdown.classList.remove("show");
        }
    }

    /**
     * Update the selected items display.
     * - In single-select mode, the selected item is shown inside the input field.
     * - In multi-select mode:
     *     * In "tag" mode, selections are shown as removable tags.
     *     * In "grid" mode, selections are shown in a table with removal buttons.
     */
    /**
  * Update the selected items display.
  * This method now also triggers an onChange callback if provided in the config.
  */
    updateSelectedUI() {
        const displayText = (item) => this.getDisplayText(item);

        if (!this.cfg.multiselect) {
            this.input.value = this.selectedItems[0] ? displayText(this.selectedItems[0]) : "";
            this.selectedContainer.innerHTML = "";
        } else {
            this.input.value = "";
            if (this.cfg.renderMode === "tag") {
                this.selectedContainer.innerHTML = this.selectedItems.map(item => `
                <span class="badge bg-secondary me-1">
                    ${displayText(item)}
                    <span data-id="${item[this.cfg.valueField]}" class="ms-1" style="cursor: pointer;">&times;</span>
                </span>
            `).join("");
                this.selectedContainer.querySelectorAll("[data-id]").forEach(span => {
                    span.onclick = () => {
                        this.removeSelected(span.dataset.id);
                        if (typeof this.cfg.onChange === "function") {
                            this.cfg.onChange(this.getValue());
                        }
                    };
                });
            } else if (this.cfg.renderMode === "grid") {
                this.selectedContainer.innerHTML = `
                <table class="table table-bordered small">
                    <tbody>
                        ${this.selectedItems.map(item => `
                            <tr>
                                <td>${displayText(item)}</td>
                                <td style="width:1%; white-space: nowrap;">
                                    <button class="btn btn-sm btn-danger" data-id="${item[this.cfg.valueField]}">&times;</button>
                                </td>
                            </tr>
                        `).join("")}
                    </tbody>
                </table>
            `;
                this.selectedContainer.querySelectorAll("button[data-id]").forEach(btn => {
                    btn.onclick = () => {
                        this.removeSelected(btn.getAttribute("data-id"));
                        if (typeof this.cfg.onChange === "function") {
                            this.cfg.onChange(this.getValue());
                        }
                    };
                });
            } // 👈 Bu } eksikti
        }

        // Trigger onChange even after full update
        if (typeof this.cfg.onChange === "function") {
            this.cfg.onChange(this.getValue());
        }
    }


/**
 * Remove a selected item by its valueField.
 * @param {string|number} id - The identifier of the item to remove.
 */
removeSelected(id) {
    this.selectedItems = this.selectedItems.filter(i =>
        i[this.cfg.valueField].toString() !== id.toString()
    );
    this.updateSelectedUI();
}

/**
 * Get the current selection.
 * @returns {Array|object|null} - Returns an array for multi-select or a single object (or null) for single-select.
 */
getValue() {
    return this.cfg.multiselect ? this.selectedItems : this.selectedItems[0] || null;
}

/**
 * Helper method to build the display text for an item.
 * If displayProperty is an array, concatenates the values separated by " - ".
 * Otherwise, returns the single property value.
 * @param {object} item - The data item.
 * @returns {string} The display text.
 */
    getDisplayText(item) {
        if (Array.isArray(this.cfg.displayProperty)) {
            return this.cfg.displayProperty
                .map(prop => item[prop])
                .filter(val => val != null)
                .join(" - ");
        }
        return item[this.cfg.displayProperty] ?? "";
    }
}
