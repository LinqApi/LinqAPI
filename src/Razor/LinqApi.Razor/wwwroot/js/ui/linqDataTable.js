import { Query, Include, LogicalFilter, Pager } from "../core/models.js";
import { StateManager } from "../core/state.js";

export class LinqDataTable {
    /**
     * Creates a dynamic data table with support for query binding, inline editing,
     * and create/update forms.
     * @param {object} config
     * @param {string} config.controller - E.g. "Country"
     * @param {HTMLElement} config.container - DOM element where the table will be rendered
     * @param {string} [config.apiPrefix="/api"] - API endpoint prefix
     * @param {boolean} [config.enableCreate=true] - Show "Create" button/form
     * @param {boolean} [config.enableUpdate=true] - Enable inline update (edit mode)
     * @param {boolean} [config.debug=false]
     * @param {object} [config.stateManager] - Optional shared state manager.
     */
    constructor(config) {
        const defaults = {
            controller: "",
            container: null,
            apiPrefix: "/api",
            enableCreate: true,
            enableUpdate: true,
            debug: false,
            stateManager: null
        };
        this.cfg = { ...defaults, ...config };

        if (!this.cfg.container) {
            throw new Error("LinqDataTable requires config.container to be set.");
        }
        if (!this.cfg.controller) {
            throw new Error("LinqDataTable requires config.controller to be set.");
        }

        // Data model defaults
        this.data = [];
        this.properties = []; // Column metadata
        this.pageNumber = 1;
        this.pageSize = 10;
        this.totalCount = 0;
        this.filter = "1=1";
        this.groupBy = "";
        this.select = "";
        this.orderBy = "id";
        this.desc = true;
        this.includes = [];

        // DOM references and API endpoint
        this.container = this.cfg.container;
        this.apiPrefix = this.cfg.apiPrefix.endsWith("/")
            ? this.cfg.apiPrefix
            : this.cfg.apiPrefix + "/";
        this.controller = this.cfg.controller;

        // Placeholders for UI elements:
        this.table = null;
        this.thead = null;
        this.tbody = null;
        this.paginationContainer = null;
        this.addFormElement = null;
        this.updateFormElement = null;
        this.selectedRow = null;
        this.selectedData = null;

        // Use provided stateManager or a dummy one.
        this.state = this.cfg.stateManager || {
            getState: () => ({}),
            setState: () => { },
            subscribe: () => { }
        };

        this.log("Constructed LinqDataTable with config", this.cfg);
    }

    log(...args) {
        if (this.cfg.debug) console.debug("[LinqDataTable]", ...args);
    }

    async init() {
        if (!this.properties.length) {
            await this.fetchProperties();
        }
        // StateManager’a abone olarak query değişikliklerini dinle:
        this.state.subscribe(async (newState) => {
            if (newState.query) {
                await this.fetchDataFromQuery(newState.query);
                this.render();
            }
        });
        await this.fetchData();
        this.render();
    }
    async fetchProperties() {
        const url = `${this.apiPrefix}${this.controller}/properties`;
        try {
            const res = await fetch(url);
            const result = await res.json();
            const arr = Array.isArray(result) ? result : (result.$values || result);
            // Basit tipleri filtrele:
            this.properties = (arr || []).filter(p => {
                const t = p.type || "";
                return !t.includes("Collection") && !t.startsWith("System.Object");
            });
            // Kompleks tipleri filtrele:
            this.complexProperties = (arr || []).filter(p =>
                p.type.includes("Collection") || p.type.includes("IEnumerable") || p.type.includes("ICollection")
            );
            this.log("Fetched simple properties:", this.properties);
            this.log("Fetched complex properties:", this.complexProperties);
        } catch (err) {
            console.error("Failed to fetch properties:", err);
        }
    }
    
    openNestedGrid(propertyName, rowData) {
        // İlgili property nesnesini complexProperties listesinden bulalım
        const propObj = this.complexProperties.find(p => p.name.toLowerCase() === propertyName.toLowerCase());
        let nestedController = propertyName;


        let typeStr = propObj.type;
        if (typeStr.includes("`1")) {
            // Eğer generic tip adını backend düzeltemiyorsa; burada sabit bir dönüşüm uygulayabilirsiniz.
            // Bu örnekte, varsayılan olarak "PosCompany" kullanılıyor, ama dinamik yapmak isterseniz ekstra mantık ekleyin.
            typeStr = typeStr.replace(/`1/, "<PosCompany>");
        }
        const collectionTypeMatch = typeStr.match(/ICollection<(.+)>/);
        if (collectionTypeMatch && collectionTypeMatch[1]) {
            nestedController = collectionTypeMatch[1];
        }

        // Yeni Query oluşturuluyor:
        const nestedQuery = new Query(nestedController);
        if (rowData && rowData.id) {
            nestedQuery.filter = new LogicalFilter("AND", [`${this.cfg.controller}Id = ${rowData.id}`]);
        }
        nestedQuery.pager = new Pager(1, 10);

        // Yeni StateManager oluşturuluyor ve dictionary'ye ekleniyor:
        const nestedState = new StateManager({ query: nestedQuery });
        if (!this.nestedStates) {
            this.nestedStates = {};
        }
        this.nestedStates[propertyName] = nestedState;

        console.log("Opening nested grid for", propertyName, "with nested controller:", nestedController, "and query:", nestedQuery);

        // Yeni container oluşturun (örneğin, modal veya nested div)
        const nestedContainer = document.createElement("div");
        nestedContainer.className = "nested-grid-container";
        document.body.appendChild(nestedContainer); // Veya uygun bir DOM noktasına ekleyin

        // Yeni nested grid (LinqDataTable) instance'ı oluşturun:
        const nestedDataTable = new LinqDataTable({
            container: nestedContainer,
            controller: nestedController,
            stateManager: nestedState,
            debug: this.cfg.debug
        });
        nestedDataTable.init();
    }


    // Yardımcı singularize fonksiyonu (basit örnek)
    singularize(name) {
        // Örneğin, "PosServices" → "PosService"
        if (name.endsWith("s")) {
            return name.slice(0, -1);
        }
        return name;
    }


    async fetchData() {
        const url = `${this.apiPrefix}${this.controller}/filterpaged`;
        const payload = {
            filter: this.filter,
            groupBy: this.groupBy,
            select: this.select,
            orderBy: this.orderBy,
            desc: this.desc,
            pager: { pageNumber: this.pageNumber, pageSize: this.pageSize },
            includes: this.includes
        };
        try {
            const res = await fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });
            const result = await res.json();
            let items = result.items || result.Items || result.results;
            if (items && items.$values) {
                items = items.$values;
            }
            this.data = items || [];
            this.totalCount = result.totalRecords || result.TotalRecords || 0;
            this.log("Data fetched:", this.data);
        } catch (err) {
            console.error("Error fetching data:", err);
        }
    }

    /**
     * Update internal query parameters from an external query object,
     * then fetch data.
     * @param {object} query - A query object (instance of Query)
     */
    async fetchDataFromQuery(query) {
        this.filter = query.filter.toString();
        this.orderBy = query.orderBy;
        this.desc = query.desc;
        this.groupBy = query.groupBy;
        this.select = query.select;
        this.includes = query.includes;
        this.pageNumber = 1;
        await this.fetchData();
    }

    render() {
        this.log("Rendering data table...");
        this.container.innerHTML = "";

        if (this.cfg.enableCreate) this.renderCreateButton();

        const tableWrapper = document.createElement("div");
        tableWrapper.className = "table-responsive";
        this.table = document.createElement("table");
        this.table.className = "table table-striped table-bordered";
        this.thead = document.createElement("thead");
        this.tbody = document.createElement("tbody");
        this.table.appendChild(this.thead);
        this.table.appendChild(this.tbody);
        tableWrapper.appendChild(this.table);
        this.container.appendChild(tableWrapper);

        this.paginationContainer = document.createElement("div");
        this.paginationContainer.className = "d-flex justify-content-between align-items-center my-3";
        this.container.appendChild(this.paginationContainer);

        this.renderTableHeader();
        this.renderTableBody();
        this.renderPagination();

        setTimeout(() => {
            const firstRow = this.tbody.querySelector("tr");
            if (firstRow) this.onRowSelected(firstRow, this.data[0]);
        }, 0);
    }

    renderTableHeader() {
        let simpleKeys = [];
        let complexKeys = [];
        let keys = [];

        if (this.data.length > 0) {
            // Basit alanlar, properties üzerinden:
            simpleKeys = this.properties.map(p => p.name.toLowerCase());
            // "id" her zaman başta olsun:
            const idIndex = simpleKeys.findIndex(k => k === 'id');
            if (idIndex > 0) {
                const idKey = simpleKeys.splice(idIndex, 1)[0];
                simpleKeys.unshift(idKey);
            }
            // Kompleks alanlar, complexProperties üzerinden:
            complexKeys = this.complexProperties.map(p => p.name.toLowerCase());
            // Sıralama: önce basit alanlar, sonra kompleks alanlar.
            keys = [...simpleKeys, ...complexKeys];
        } else {
            // Eğer veri yoksa, fallback olarak properties’den kullanabilirsiniz.
            keys = this.properties.map(p => p.name.toLowerCase())
                .concat(this.complexProperties.map(p => p.name.toLowerCase()));
        }

        this.thead.innerHTML = "";
        const headerRow = document.createElement("tr");

        keys.forEach(key => {
            const th = document.createElement("th");
            let displayName = key;
            // Eğer basit alan ise (sıralama yapılabilir):
            if (simpleKeys.includes(key)) {
                if (this.orderBy.toLowerCase() === key.toLowerCase()) {
                    const arrowIcon = this.desc ? "▼" : "▲";
                    displayName += ` ${arrowIcon}`;
                }
                th.style.cursor = "pointer";
                th.onclick = () => {
                    if (this.orderBy.toLowerCase() === key.toLowerCase()) {
                        this.desc = !this.desc;
                    } else {
                        this.orderBy = key;
                        this.desc = false;
                    }
                    this.pageNumber = 1;
                    this.fetchData().then(() => this.render());
                };
            } else {
                const viewBtn = document.createElement("button");
                viewBtn.className = "btn btn-sm btn-info";
                viewBtn.textContent = "View";
                viewBtn.onclick = (e) => {
                    e.stopPropagation();
                    console.log("View button clicked for:", key);
                    // İlgili row verisini (varsa) da iletebilirsiniz. Örneğin, openNestedGrid(key, rowData)
                    this.openNestedGrid(key, null);
                    // İsteğe bağlı: state güncellemesi
                    this.state.setState({ query: this.getQuery() });
                };
                th.appendChild(viewBtn);
            }
            th.textContent = displayName;
            headerRow.appendChild(th);
        });

        // "Actions" kolonu her zaman en sonda
        const actionTh = document.createElement("th");
        actionTh.textContent = "Actions";
        headerRow.appendChild(actionTh);
        this.thead.appendChild(headerRow);

        // Kaydedilen keys dizisini, body render’ında da kullanmak üzere saklayalım:
        this._renderKeys = keys;
    }

    renderTableBody() {
        this.tbody.innerHTML = "";
        // Eğer _renderKeys tanımlı ise, onu kullanın; yoksa header'dan dinamik hesaplama yapın.
        const keys = this._renderKeys || (
            this.properties.map(p => p.name.toLowerCase())
                .concat(this.complexProperties.map(p => p.name.toLowerCase()))
        );

        this.data.forEach(item => {
            const normalizedItem = {};
            // Tüm item anahtarlarını küçük harfe çevirerek normalize edelim.
            Object.keys(item).forEach(key => {
                normalizedItem[key.toLowerCase()] = item[key];
            });
            const row = document.createElement("tr");
            row.onclick = () => this.onRowSelected(row, item);
            keys.forEach(key => {
                const td = document.createElement("td");
                // Eğer kompleks alan ise, örneğin, sadece "View" butonu ekleyebilirsiniz.
                if (this.complexProperties.map(p => p.name.toLowerCase()).includes(key)) {
                    // Basit örnek: "View" yazısı, tıklanırsa nested grid açar.
                    td.innerHTML = `<button class="btn btn-sm btn-info" onclick="event.stopPropagation();">View</button>`;
                    td.onclick = (e) => {
                        e.stopPropagation();
                        // Burada, nested grid açmak için openNestedGrid çağrısı yapabilirsiniz.
                        this.openNestedGrid(key, item);
                    };
                } else {
                    td.textContent = normalizedItem[key] !== undefined ? normalizedItem[key] : "";
                }
                row.appendChild(td);
            });
            // Action column.
            const actionTd = document.createElement("td");
            if (this.cfg.enableUpdate) {
                const editBtn = document.createElement("button");
                editBtn.className = "btn btn-sm btn-warning";
                editBtn.innerHTML = "✎";
                editBtn.onclick = (e) => {
                    e.stopPropagation();
                    this.openUpdateForm(item);
                };
                actionTd.appendChild(editBtn);
            }
            row.appendChild(actionTd);
            this.tbody.appendChild(row);
        });
    }

    renderPagination() {
        this.paginationContainer.innerHTML = "";
        const totalPages = Math.ceil(this.totalCount / this.pageSize);

        // Paging Dropdown ekleyin:
        const pageSizeSelect = document.createElement("select");
        pageSizeSelect.className = "form-select w-auto d-inline-block me-2";
        const options = [10, 25, 50, 100, 250, 500];
        options.forEach(opt => {
            const optionEl = document.createElement("option");
            optionEl.value = opt;
            optionEl.textContent = opt;
            if (this.pageSize === opt) optionEl.selected = true;
            pageSizeSelect.appendChild(optionEl);
        });
        pageSizeSelect.onchange = () => {
            this.pageSize = parseInt(pageSizeSelect.value);
            if (!this.query) {
                this.query = new Query(this.controller);
            }
            if (!this.query.pager) {
                this.query.pager = new Pager(this.pageNumber, this.pageSize);
            } else {
                this.query.pager.pageNumber = this.pageNumber;
                this.query.pager.pageSize = this.pageSize;
            }
            this.state.setState({ query: this.getQuery() });
        };
        this.paginationContainer.appendChild(pageSizeSelect);

        // Klasik pagination (önceki kod):
        const nav = document.createElement("nav");
        const ul = document.createElement("ul");
        ul.className = "pagination";

        // Previous button
        const prevLi = document.createElement("li");
        prevLi.className = `page-item ${this.pageNumber === 1 ? "disabled" : ""}`;
        const prevLink = document.createElement("a");
        prevLink.className = "page-link";
        prevLink.href = "#";
        prevLink.innerHTML = "&laquo;";
        prevLink.onclick = (e) => {
            e.preventDefault();
            if (this.pageNumber > 1) {
                this.pageNumber--;
                this.query.pager = new Pager(this.pageNumber, this.pageSize);
                this.state.setState({ query: this.getQuery() });
            }
        };
        prevLi.appendChild(prevLink);
        ul.appendChild(prevLi);

        // Page numbers
        for (let i = 1; i <= totalPages; i++) {
            const li = document.createElement("li");
            li.className = `page-item ${i === this.pageNumber ? "active" : ""}`;
            const link = document.createElement("a");
            link.className = "page-link";
            link.href = "#";
            link.textContent = i;
            link.onclick = (e) => {
                e.preventDefault();
                this.pageNumber = i;
                this.query.pager = new Pager(this.pageNumber, this.pageSize);
                this.state.setState({ query: this.getQuery() });
            };
            li.appendChild(link);
            ul.appendChild(li);
        }

        // Next button
        const nextLi = document.createElement("li");
        nextLi.className = `page-item ${this.pageNumber === totalPages ? "disabled" : ""}`;
        const nextLink = document.createElement("a");
        nextLink.className = "page-link";
        nextLink.href = "#";
        nextLink.innerHTML = "&raquo;";
        nextLink.onclick = (e) => {
            e.preventDefault();
            if (this.pageNumber < totalPages) {
                this.pageNumber++;
                this.query.pager = new Pager(this.pageNumber, this.pageSize);
                this.state.setState({ query: this.getQuery() });
            }
        };
        nextLi.appendChild(nextLink);
        ul.appendChild(nextLi);

        nav.appendChild(ul);
        this.paginationContainer.appendChild(nav);
    }


    getQuery() {
        return {
            filter: this.filter,
            groupBy: this.groupBy,
            select: this.select,
            orderBy: this.orderBy,
            desc: this.desc,
            pager: { pageNumber: this.pageNumber, pageSize: this.pageSize },
            includes: this.includes
        };
    }

    /**
    * Returns a new object with all keys converted to lower case.
    * @param {object} item - The original item object.
    * @returns {object} - The normalized item with lower-case keys.
    */
    normalizeItem(item) {
        const normalized = {};
        for (const key in item) {
            if (item.hasOwnProperty(key)) {
                normalized[key.toLowerCase()] = item[key];
            }
        }
        return normalized;
    }
    // --- EDIT / UPDATE METHODS ---
    editCell(td, rowObj, prop) {
        if (td.querySelector("input")) return;
        const oldVal = td.textContent;
        td.innerHTML = "";
        const input = document.createElement("input");
        input.type = "text";
        input.value = oldVal;
        input.className = "form-control";
        td.appendChild(input);
        input.focus();

        let finished = false;
        const finish = (save) => {
            if (save && input.value !== oldVal) {
                if (confirm("Changes detected. Do you want to update?")) {
                    const key = prop.name.charAt(0).toLowerCase() + prop.name.slice(1);
                    rowObj[key] = input.value;
                    this.updateItem(rowObj);
                    td.textContent = input.value;
                } else {
                    td.textContent = oldVal;
                }
            } else {
                td.textContent = oldVal;
            }
        };

        input.onblur = () => {
            if (!finished) {
                finished = true;
                finish(true);
            }
        };

        input.onkeydown = (e) => {
            if (e.key === "Enter") {
                if (!finished) {
                    finished = true;
                    finish(true);
                }
                e.preventDefault();
            } else if (e.key === "Escape") {
                if (!finished) {
                    finished = true;
                    finish(false);
                }
                e.preventDefault();
            }
        };
    }

    async openUpdateForm(item) {
        if (this.updateFormElement) this.updateFormElement.remove();
        if (!item) {
            alert("Please select a row first.");
            return null;
        }
        // Ensure properties are loaded
        if (!this.properties.length) {
            await this.fetchProperties();
        }
        const form = document.createElement("div");
        form.className = "card card-body mb-3";
        const inputs = {};
        const allowedTypes = ["string", "int64", "boolean", "datetime", "int32"];
        const rowDiv = document.createElement("div");
        rowDiv.className = "row";

        this.properties.forEach(prop => {
            const typeLower = (prop.type || "").toLowerCase();
            if (prop.type.includes("ICollection") || prop.name === "Id" || !allowedTypes.some(t => typeLower.includes(t))) return;
            const col = document.createElement("div");
            col.className = "col-md-6 mb-2";
            const label = document.createElement("label");
            label.className = "form-label";
            label.textContent = prop.name;
            const input = document.createElement("input");
            input.className = "form-control";
            const key = prop.name.charAt(0).toLowerCase() + prop.name.slice(1);
            input.value = item[key] || "";
            inputs[prop.name] = input;
            col.appendChild(label);
            col.appendChild(input);
            rowDiv.appendChild(col);
        });
        form.appendChild(rowDiv);

        const btnGroup = document.createElement("div");
        btnGroup.className = "d-flex justify-content-end gap-2";
        const saveBtn = document.createElement("button");
        saveBtn.className = "btn btn-success";
        saveBtn.textContent = "Update";
        saveBtn.onclick = async () => {
            const updateItem = { id: item.id };
            Object.keys(inputs).forEach(k => updateItem[k] = inputs[k].value);
            if (JSON.stringify(updateItem) !== JSON.stringify(item)) {
                if (confirm("Confirm update?")) {
                    await this.updateItem(updateItem);
                    form.remove();
                    await this.fetchData();
                    this.render();
                }
            } else {
                alert("No changes detected.");
                form.remove();
            }
        };
        const cancelBtn = document.createElement("button");
        cancelBtn.className = "btn btn-danger";
        cancelBtn.textContent = "Cancel";
        cancelBtn.onclick = () => form.remove();
        btnGroup.appendChild(saveBtn);
        btnGroup.appendChild(cancelBtn);
        form.appendChild(btnGroup);
        this.container.prepend(form);
        this.updateFormElement = form;
        return form;
    }

    createForm(type, data = {}) {
        const form = document.createElement("div");
        form.className = "card card-body collapse mb-3";
        const inputs = {};
        this.properties.forEach(prop => {
            if (prop.type.includes("ICollection") || prop.name === "Id") return;
            const label = document.createElement("label");
            label.textContent = prop.name;
            const input = document.createElement("input");
            input.type = "text";
            input.className = "form-control mb-2";
            input.value = data[prop.name] || "";
            form.appendChild(label);
            form.appendChild(input);
            inputs[prop.name] = input;
        });

        const btnGroup = document.createElement("div");
        btnGroup.className = "d-flex justify-content-end gap-2";

        const saveBtn = document.createElement("button");
        saveBtn.className = "btn btn-success me-2";
        saveBtn.innerHTML = `<i class="bi bi-check-circle"></i>`;
        saveBtn.onclick = async () => {
            const item = {};
            for (const prop in inputs) {
                item[prop] = inputs[prop].value;
            }
            if (type === "Create") {
                await this.addItem(item);
            } else if (type === "Update") {
                item.id = data.id;
                await this.updateItem(item);
            }
            form.classList.remove("show");
        };

        const cancelBtn = document.createElement("button");
        cancelBtn.className = "btn btn-danger";
        cancelBtn.innerHTML = `<i class="bi bi-x-circle"></i>`;
        cancelBtn.onclick = () => {
            form.classList.remove("show");
            form.classList.add("collapse");
        };

        btnGroup.appendChild(saveBtn);
        btnGroup.appendChild(cancelBtn);
        form.appendChild(btnGroup);

        if (this.container.firstChild && this.container.firstChild.nextSibling) {
            this.container.insertBefore(form, this.container.firstChild.nextSibling);
        } else {
            this.container.appendChild(form);
        }

        return form;
    }

    async updateItem(updatedItem) {
        const url = `${this.apiPrefix}${this.controller}/${updatedItem.id}`;
        try {
            const res = await fetch(url, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(updatedItem)
            });
            if (!res.ok) {
                const err = await res.text();
                this.showError(`Update failed: ${err}`);
            } else {
                await this.fetchData();
                this.render();
            }
        } catch (error) {
            console.error("Error in updateItem:", error);
        }
    }

    async addItem(newItem) {
        const url = `${this.apiPrefix}${this.controller}`;
        try {
            const res = await fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(newItem)
            });
            if (!res.ok) {
                const err = await res.text();
                alert(`Create failed: ${err}`);
            } else {
                const created = await res.json();
                alert(`Created item ID: ${created.id}`);
                await this.fetchData();
                this.render();
                return created;
            }
        } catch (error) {
            console.error("Error in addItem:", error);
        }
    }

    showError(message) {
        const alertEl = document.createElement("div");
        alertEl.className = "alert alert-danger alert-dismissible fade show";
        alertEl.innerHTML = `${message}<button type="button" class="btn-close" data-bs-dismiss="alert"></button>`;
        this.container.prepend(alertEl);
    }

    displayCreatedItem(createdItem) {
        const div = document.createElement("div");
        div.className = "alert alert-info alert-sm mt-2";
        div.innerHTML = `<b>Newly created:</b> ${JSON.stringify(createdItem)}`;
        if (this.addFormElement) {
            this.addFormElement.after(div);
        } else {
            this.container.appendChild(div);
        }
        setTimeout(() => div.remove(), 5000);
    }

    getFilterSuggestions(currentInput) {
        const tokens = currentInput.trim().split(/\s+/);
        const lastToken = tokens[tokens.length - 1];
        if (!currentInput || currentInput.trim() === "") {
            return this.properties.map(p => p.name);
        }
        if (tokens.length > 0 && this.isValidFilterSegment(tokens.slice(-3).join(" "))) {
            if (currentInput.endsWith(" ")) {
                return ["AND", "OR"];
            }
        }
        if (lastToken.includes(".")) {
            const [propName] = lastToken.split(".");
            const propType = this.getPropertyType(propName);
            if (propType.includes("string")) {
                return ['Contains("")', 'StartsWith("")', 'EndsWith("")'];
            } else if (propType.includes("int") || propType.includes("datetime")) {
                return ["Year", "Month", "Day"];
            }
        } else if (this.properties.some(p => p.name.toLowerCase() === lastToken.toLowerCase())) {
            const propType = this.getPropertyType(lastToken);
            if (propType.includes("string")) {
                return ["==", "!=", ".Contains(\"\")", ".StartsWith(\"\")"];
            } else if (propType.includes("int") || propType.includes("datetime")) {
                return ["==", "!=", ">", "<", ">=", "<="];
            } else if (propType.includes("bool")) {
                return ["== true", "== false"];
            }
        }
        return this.searchProps(lastToken, this.properties).map(p => p.name);
    }

    showSuggestions(inputEl, suggestions) {
        let dropdown = inputEl.nextElementSibling;
        if (dropdown && dropdown.classList.contains("dropdown-menu")) {
            dropdown.remove();
        }
        dropdown = document.createElement("ul");
        dropdown.className = "dropdown-menu show";
        dropdown.style.width = inputEl.offsetWidth + "px";
        suggestions.forEach(s => {
            const li = document.createElement("li");
            li.innerHTML = `<a class="dropdown-item">${s}</a>`;
            li.onclick = () => {
                const inputParts = inputEl.value.split(/\s+/);
                inputParts.pop();
                inputParts.push(s);
                inputEl.value = inputParts.join(" ") + " ";
                dropdown.remove();
                inputEl.focus();
            };
            dropdown.appendChild(li);
        });
        inputEl.parentElement.style.position = "relative";
        inputEl.parentElement.appendChild(dropdown);
        document.addEventListener("click", () => dropdown.remove(), { once: true });
    }

    getSuggestions(type, currentValue) {
        if (type !== "filter") return [];
        const tokens = currentValue.trim().split(/\s+/);
        const lastToken = tokens[tokens.length - 1];
        if (currentValue.endsWith(".")) {
            const prop = this.properties.find(p => p.name.toLowerCase() === lastToken.replace(".", "").toLowerCase());
            if (prop) {
                const typeLower = (prop.type || "").toLowerCase();
                if (typeLower.includes("string")) return ['Contains("")', 'StartsWith("")', 'EndsWith("")'];
                if (typeLower.includes("int") || typeLower.includes("datetime")) return ["Year", "Month", "Day"];
                if (typeLower.includes("bool")) return ["Equals(true)", "Equals(false)"];
            }
        } else if (/(\w+)\s*(==|>|<|>=|<=)\s*["']?\w*["']?$/.test(currentValue)) {
            return ["AND", "OR"];
        }
        return this.properties.map(p => p.name);
    }


    renderCreateButton() {
        const btn = document.createElement("button");
        btn.className = "btn btn-success mb-3";
        btn.innerHTML = '<i class="bi bi-plus-circle"></i>';
        btn.addEventListener("click", () => this.toggleCreateForm());
        this.container.appendChild(btn);
    }
    toggleCreateForm() {
        if (!this.addFormElement) {
            this.createAddForm();
        }
        const form = this.addFormElement;
        const btn = this.container.querySelector('button.btn-success'); // The Create button
        if (form.classList.contains('show')) {
            form.classList.remove('show');
            form.classList.add('collapse');
            btn.innerHTML = '<i class="bi bi-plus-circle"></i> Create';
        } else {
            form.classList.remove('collapse');
            form.classList.add('show');
            btn.innerHTML = '<i class="bi bi-dash-circle"></i> Cancel';
        }
    }
    createAddForm() {
        const form = document.createElement('div');
        form.className = 'card card-body collapse mb-3';

        const inputs = {};
        const allowedTypes = ['string', 'int64', 'boolean', 'datetime', 'int32'];

        this.properties.forEach(prop => {
            const typeLower = prop.type.toLowerCase();
            if (prop.name.toLowerCase() === 'id' || !allowedTypes.some(t => typeLower.includes(t))) return;

            const label = document.createElement('label');
            label.textContent = prop.name;

            const input = document.createElement('input');
            input.type = 'text';
            input.className = 'form-control form-control-sm mb-2';

            form.appendChild(label);
            form.appendChild(input);

            inputs[prop.name] = input;
        });

        const btnGroup = document.createElement('div');
        btnGroup.className = 'd-flex justify-content-end gap-2';


        const saveBtn = document.createElement('button');
        saveBtn.className = 'btn btn-success me-2';

        saveBtn.innerHTML = '<i class="bi bi-check-circle"></i>';
        saveBtn.addEventListener('click', async () => {
            const newItem = {};
            for (const propName in inputs) {
                newItem[propName] = inputs[propName].value;
            }
            const createdItem = await this.addItem(newItem);

            form.classList.remove('show');
            form.classList.add('collapse');

            if (createdItem) {
                this.displayCreatedItem(createdItem);
            }
        });

        const cancelBtn = document.createElement('button');
        cancelBtn.className = 'btn btn-danger';
        cancelBtn.innerHTML = '<i class="bi bi-x-circle"></i>';
        cancelBtn.addEventListener('click', () => {
            form.classList.remove('show');
            form.classList.add('collapse');
        });

        btnGroup.appendChild(saveBtn);
        btnGroup.appendChild(cancelBtn);
        form.appendChild(btnGroup);

        this.container.insertBefore(form, this.container.firstChild.nextSibling);
        this.addFormElement = form;
    }

    // Seçilen row yönetimi
    onRowSelected(rowElement, rowData) {
        if (this.selectedRow) {
            this.selectedRow.classList.remove('table-active');
        }
        rowElement.classList.add('table-active');
        this.selectedRow = rowElement;
        this.selectedData = rowData;
    }

    searchProps(input, properties) {
        return properties.filter(p => p.name.toLowerCase().includes(input.toLowerCase()));
    }

    getPropertyType(propName) {
        const prop = this.properties.find(p => p.name === propName);
        return prop ? (prop.type || "").toLowerCase() : "";
    }

    isValidFilterSegment(segment) {
        const regex = /(\w+)\s*(==|>|<|>=|<=|!=)\s*(\w+|".*"|'.*')/;
        const funcRegex = /(\w+)\.(Contains|StartsWith|EndsWith)\(["'][^"']+["']\)/;
        return regex.test(segment.trim()) || funcRegex.test(segment.trim());
    }
}
