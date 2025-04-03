import { defaults, fetchProperties } from "./linqutil.js";
import { Query, LogicalFilter, Include, Pager } from "../core/models.js";

export class QueryBuilder {

    /**
     * @param {Object} options
     * @param {HTMLElement|string} options.container - UI container.
     * @param {StateManager} options.stateManager - Shared state manager.
     * @param {string} options.controller - Relevant controller name.
     * @param {string} [options.apiPrefix="/api"] - API prefix.
     * @param {Array} [options.properties=null] - List of properties if provided.
     */
    constructor({ container, stateManager, controller, apiPrefix = "/api", properties = null }) {
        this.stateManager = stateManager;
        this.controller = controller;
        this.apiPrefix = apiPrefix;

        // Select the container element
        this.containerElm = (typeof container === "string")
            ? document.querySelector(container)
            : container;
        if (!this.containerElm) throw new Error("QueryBuilder: Invalid container.");

        // Retrieve query from state manager or create a new Query with defaults
        this.query = this.stateManager.getState("query") || new Query(controller, {
            filter: new LogicalFilter("AND", [{ toString: () => "1=1" }]),
            pager: new Pager(),
            orderBy: "id",
            desc: true,
            groupBy: "",
            select: "",
            includes: []
        });

        this.properties = properties;
        if (!this.properties) {
            fetchProperties(this.controller, this.apiPrefix)
                .then(props => {
                    this.properties = props;
                    // Optionally extend the UI based on properties.
                })
                .catch(error => console.error("Failed to fetch properties:", error));
        }
        this._render();

        // Subscribe to state changes and update the UI accordingly.
        this.stateManager.subscribe("query", (newQuery) => {
            this.query = newQuery;
            this._updateUI();
        });
    }

    _render() {
        // Clear the container
        this.containerElm.innerHTML = "";

        // Row 1: Filter and Select fields in two columns
        const row1 = document.createElement("div");
        row1.className = "row mb-3";
        // Filter column
        const filterCol = document.createElement("div");
        filterCol.className = "col-md-6";
        const filterLabel = document.createElement("label");
        filterLabel.textContent = "Filter:";
        filterLabel.className = "form-label";
        this.filterInput = document.createElement("input");
        this.filterInput.type = "text";
        this.filterInput.className = "form-control";
        // Use default "1=1" if undefined
        this.filterInput.value = (this.query && this.query.filter && this.query.filter.toString()) || "1=1";
        filterCol.appendChild(filterLabel);
        filterCol.appendChild(this.filterInput);
        row1.appendChild(filterCol);
        // Select column
        const selectCol = document.createElement("div");
        selectCol.className = "col-md-6";
        const selectLabel = document.createElement("label");
        selectLabel.textContent = "Select:";
        selectLabel.className = "form-label";
        this.selectInput = document.createElement("input");
        this.selectInput.type = "text";
        this.selectInput.className = "form-control";
        this.selectInput.value = (this.query && this.query.select) || "";
        selectCol.appendChild(selectLabel);
        selectCol.appendChild(this.selectInput);
        row1.appendChild(selectCol);
        this.containerElm.appendChild(row1);

        // Row 2: GroupBy and OrderBy fields
        const row2 = document.createElement("div");
        row2.className = "row mb-3";
        // GroupBy column
        const groupByCol = document.createElement("div");
        groupByCol.className = "col-md-6";
        const groupByLabel = document.createElement("label");
        groupByLabel.textContent = "GroupBy:";
        groupByLabel.className = "form-label";
        this.groupByInput = document.createElement("input");
        this.groupByInput.type = "text";
        this.groupByInput.className = "form-control";
        this.groupByInput.value = (this.query && this.query.groupBy) || "";
        groupByCol.appendChild(groupByLabel);
        groupByCol.appendChild(this.groupByInput);
        row2.appendChild(groupByCol);
        // OrderBy column
        const orderByCol = document.createElement("div");
        orderByCol.className = "col-md-6";
        const orderByLabel = document.createElement("label");
        orderByLabel.textContent = "OrderBy:";
        orderByLabel.className = "form-label";
        this.orderByInput = document.createElement("input");
        this.orderByInput.type = "text";
        this.orderByInput.className = "form-control";
        this.orderByInput.value = (this.query && this.query.orderBy) || "id";
        orderByCol.appendChild(orderByLabel);
        orderByCol.appendChild(this.orderByInput);
        row2.appendChild(orderByCol);
        this.containerElm.appendChild(row2);

        // Row 3: Desc checkbox and Apply button
        const row3 = document.createElement("div");
        row3.className = "row mb-3 align-items-center";
        // Desc column
        const descCol = document.createElement("div");
        descCol.className = "col-md-6";
        const descDiv = document.createElement("div");
        descDiv.className = "form-check";
        this.descInput = document.createElement("input");
        this.descInput.type = "checkbox";
        this.descInput.className = "form-check-input";
        this.descInput.checked = (this.query && typeof this.query.desc === "boolean") ? this.query.desc : true;
        const descLabel = document.createElement("label");
        descLabel.textContent = "Desc:";
        descLabel.className = "form-check-label";
        descDiv.appendChild(this.descInput);
        descDiv.appendChild(descLabel);
        descCol.appendChild(descDiv);
        row3.appendChild(descCol);
        // Apply button column
        const btnCol = document.createElement("div");
        btnCol.className = "col-md-6 text-end";
        this.applyBtn = document.createElement("button");
        this.applyBtn.textContent = "Uygula";
        this.applyBtn.className = "btn btn-primary";
        this.applyBtn.addEventListener("click", () => this._applyQuery());
        btnCol.appendChild(this.applyBtn);
        row3.appendChild(btnCol);
        this.containerElm.appendChild(row3);
    }

    _applyQuery() {
        // Update the Query instance with input values using default values as needed.
        const newQuery = {
            filter: new LogicalFilter("AND", [
                { toString: () => (this.filterInput.value ? this.filterInput.value : "1=1") }
            ]),
            select: this.selectInput.value || "",
            groupBy: this.groupByInput.value || "",
            orderBy: this.orderByInput.value || "id",
            desc: !!this.descInput.checked
        };
        // Use stateManager to publish the updated query
        this.stateManager.setState({ query: new Query(this.controller, newQuery) });
    }

    _updateUI() {
        // Reflect the current query state in the UI controls.
        this.filterInput.value = (this.query && this.query.filter && this.query.filter.toString()) || "1=1";
        this.selectInput.value = (this.query && this.query.select) || "";
        this.groupByInput.value = (this.query && this.query.groupBy) || "";
        this.orderByInput.value = (this.query && this.query.orderBy) || "id";
        this.descInput.checked = (this.query && typeof this.query.desc === "boolean") ? this.query.desc : true;
    }

    onQueryUpdated(newQuery) {
        // Optionally update the query manually if needed.
        this.query = newQuery;
        this.query.select = this.selectInput.value;
        this.query.groupBy = this.groupByInput.value;
        this.query.orderBy = this.orderByInput.value;
        this.query.desc = this.descInput.checked;
        this.query.pager = newQuery.Pager;
    }
}
