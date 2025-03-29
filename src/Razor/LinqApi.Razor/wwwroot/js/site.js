///*******************************************************
// * DynamicDataTable.js
// * A single-file, open-source-friendly, config-based,
// * IntelliSense-enabled, LINQ-inspired data table.
// *******************************************************/

//// Optional: external or in-file. Let's inline a basic mapping for demonstration
//const DefaultLinqIntellisenseMap = {
//    filter: {
//        String: [
//            { name: "Contains", tooltip: `Usage: Name.Contains("val")` },
//            { name: "StartsWith", tooltip: `Usage: Name.StartsWith("val")` },
//            { name: "EndsWith", tooltip: `Usage: Name.EndsWith("val")` },
//            { name: "Equals", tooltip: `Usage: Name == "val"` },
//            { name: "Length", tooltip: `Usage: Name.Length > 0` }
//        ],
//        Boolean: [
//            { name: "== true", tooltip: "Is true" },
//            { name: "== false", tooltip: "Is false" }
//        ],
//        Int64: [
//            { name: ">", tooltip: "Greater than" },
//            { name: "<", tooltip: "Less than" },
//            { name: "==", tooltip: "Equals" },
//            { name: ">=", tooltip: "Greater or equal" },
//            { name: "<=", tooltip: "Less or equal" }
//        ],
//        DateTime: [
//            { name: ">", tooltip: `Usage: CreatedDate > "2023-01-01"` },
//            { name: "<", tooltip: `Usage: CreatedDate < "2024-01-01"` },
//            { name: "==", tooltip: `Usage: CreatedDate == "2023-05-10"` },
//            { name: ".Year", tooltip: "Year part" },
//            { name: ".Month", tooltip: "Month part" },
//            { name: ".Day", tooltip: "Day part" }
//        ]
//    }
//};

///*******************************************************
// * The main class
// *******************************************************/
//class DynamicDataTable {
//    /**
//     * Creates a dynamic data table with LINQ-based filtering, grouping, and optional IntelliSense.
//     * @param {object} config
//     * @param {string} config.controller - e.g. "Country"
//     * @param {HTMLElement} config.container - The DOM element to render into
//     * @param {string} [config.apiPrefix="/api"] - API endpoint prefix
//     * @param {boolean} [config.enableCreate=true]
//     * @param {boolean} [config.enableUpdate=true]
//     * @param {boolean} [config.enableHamburgerLinq=true]
//     * @param {boolean} [config.enableIntellisense=true]
//     * @param {boolean} [config.vmMode=false] - If true, uses TCreateVm / TUpdateVm schemas
//     * @param {boolean} [config.debug=false]
//     * @param {object} [config.intellisenseMap=DefaultLinqIntellisenseMap] - If you want to override the mapping
//     */
//    constructor(config) {
//        // Merge config with defaults
//        const defaults = {
//            controller: "",
//            container: null,
//            apiPrefix: "/api",
//            enableCreate: true,
//            enableUpdate: true,
//            enableHamburgerLinq: true,
//            enableIntellisense: true,
//            vmMode: false,
//            debug: false,
//            intellisenseMap: DefaultLinqIntellisenseMap
//        };
//        this.cfg = { ...defaults, ...config };

//        if (!this.cfg.container) {
//            throw new Error("DynamicDataTable requires config.container to be set.");
//        }
//        if (!this.cfg.controller) {
//            throw new Error("DynamicDataTable requires config.controller to be set.");
//        }

//        // Basic data model
//        this.data = [];
//        this.properties = [];
//        this.pageNumber = 1;
//        this.pageSize = 10;
//        this.totalCount = 0;
//        this.filter = "1=1";
//        this.groupBy = "";
//        this.select = "";
//        this.orderBy = "id";
//        this.desc = true;

//        // Internal references
//        this.container = this.cfg.container;
//        this.apiPrefix = this.cfg.apiPrefix.endsWith("/")
//            ? this.cfg.apiPrefix
//            : this.cfg.apiPrefix + "/";
//        this.controller = this.cfg.controller;
//        this.intellisenseMap = this.cfg.intellisenseMap;

//        // Extra placeholders
//        this.table = null;
//        this.tbody = null;
//        this.thead = null;
//        this.intellisenseEnabled = true;
//        // Start
//        this.log("Constructed DynamicDataTable with config", this.cfg);
//    }
    
    

   

//    /**
//     * Initialization: fetch metadata, fetch data, render UI
//     */
//    async init() {
//        if (this.cfg.vmMode) {
//            await this.fetchVmSchema();
//        } else {
//            await this.fetchProperties();
//        }
//        await this.fetchData();
//    this.render();

//    // Başlangıçta bunu class içinde tanımla
//    this.intellisenseEnabled = true;

//    // Toggle button click eventi
//    document.getElementById('toggleIntellisense').onclick = (e) => {
//        this.intellisenseEnabled = !this.intellisenseEnabled; // <-- burası düzeltildi
//        e.target.textContent = `Intellisense: ${this.intellisenseEnabled ? 'ON' : 'OFF'}`;
//    };

//    }
  

//    /**
//     * If vmMode is true, fetch TCreateVm / TUpdateVm schema endpoints
//     */
//    async fetchVmSchema() {
//        // e.g.: GET /api/Country/create-vm-schema
//        // e.g.: GET /api/Country/update-vm-schema
//        try {
//            const createUrl = `${this.apiPrefix}${this.controller}/create-vm-schema`;
//            const updateUrl = `${this.apiPrefix}${this.controller}/update-vm-schema`;
//            this.log("Fetching VM schema from", createUrl, updateUrl);

//            const [cRes, uRes] = await Promise.all([fetch(createUrl), fetch(updateUrl)]);
//            const cSchema = await cRes.json();
//            const uSchema = await uRes.json();

//            // let's pick update schema for 'properties'
//            const arr = Array.isArray(uSchema) ? uSchema : (uSchema.$values || uSchema);
//            this.properties = (arr || []).filter(p => {
//                const t = p.type || p.Type || "";
//                return !t.startsWith("System.Collections") && !t.startsWith("System.Object");
//            });

//            this.log("VM-based properties =>", this.properties);
//        } catch (err) {
//            console.error("Failed to fetch VM schemas =>", err);
//        }
//    }

//    /**
//     * If vmMode is false, fetch entity-based properties from /properties
//     */
//    async fetchProperties() {
//        const url = `${this.apiPrefix}${this.controller}/properties`;
//        this.log("Fetching entity properties from", url);
//        try {
//            const res = await fetch(url);
//            const result = await res.json();
//            const arr = Array.isArray(result) ? result : (result.$values || result);
//            this.properties = (arr || []).filter(p => {
//                const t = p.type || p.Type || "";
//                return !t.startsWith("System.Collections") && !t.startsWith("System.Object");
//            });
//            this.log("Entity-based properties =>", this.properties);
//        } catch (err) {
//            console.error("Failed to fetchProperties =>", err);
//        }
//    }

//    /**
//     * Fetch data from filterpaged
//     */
//    async fetchData() {
//        const url = `${this.apiPrefix}${this.controller}/filterpaged`;
//        const payload = {
//            filter: this.filter,
//            groupBy: this.groupBy,
//            select: this.select,
//            orderBy: this.orderBy,
//            desc: this.desc,
//            pager: { pageNumber: this.pageNumber, pageSize: this.pageSize },
//            includes: this.includes
//        };
//        try {
//            if (typeof this.onBeforeRequest === "function")
//                this.onBeforeRequest(payload);

//            const res = await fetch(url, {
//                method: 'POST',
//                headers: { 'Content-Type': 'application/json' },
//                body: JSON.stringify(payload)
//            });

//            const result = await res.json();
//            let items = result.items || result.Items || result.results;
//            if (items && items.$values) {
//                items = items.$values;
//            }
//            this.data = items || [];
//            this.totalCount = result.totalRecords || result.TotalRecords || 0;

//            if (typeof this.onAfterRequest === "function")
//                this.onAfterRequest(result);

//            this.log("fetchData() sonrası data:", this.data);
//        } catch (err) {
//            console.error("Error fetching data:", err);
//        }



//    }

//    /**
//     * Render the full table UI, including hamburger panel, create button, etc.
//     */
//    async render() {
//        this.log("Rendering the table...");

//        this.container.innerHTML = "";

//        if (this.cfg.enableCreate) this.renderCreateButton();
//        if (this.cfg.enableHamburgerLinq) this.renderHamburgerPanel();

//        const tableWrapper = document.createElement("div");
//        tableWrapper.className = "table-responsive";

//        this.table = document.createElement("table");
//        this.table.className = "table table-striped table-bordered";

//        this.thead = document.createElement("thead");
//        this.tbody = document.createElement("tbody");

//        this.theadElement = this.thead;
//        this.tbodyElement = this.tbody;

//        this.table.appendChild(this.thead);
//        this.table.appendChild(this.tbody);

//        tableWrapper.appendChild(this.table);
//        this.container.appendChild(tableWrapper);

//        this.paginationContainer = document.createElement("div");
//        this.paginationContainer.className = "d-flex justify-content-between align-items-center my-3";
//        this.container.appendChild(this.paginationContainer);

//        this.renderTableHeader();
//        this.renderTableBody();
//        this.renderPagination();

//        // Bu satırı buradan kaldır:
//        // if (this.data.length > 0) {
//        //     const firstRowElement = this.tbodyElement.querySelector('tr');
//        //     if (firstRowElement) {
//        //         this.onRowSelected(firstRowElement, this.data[0]);
//        //     }
//        // }

//        // Yerine bunu koy (kısa süreli bekleme ile DOM renderlanması sağlanır):
//        setTimeout(() => {
//            const firstRowElement = this.tbodyElement.querySelector('tr');
//            if (firstRowElement) {
//                this.onRowSelected(firstRowElement, this.data[0]);
//            }
//        }, 0);
//    }



//    // Render pagination controls using Bootstrap pagination component.
//    renderPagination() {
//        this.paginationContainer.innerHTML = '';
//        const totalPages = Math.ceil(this.totalCount / this.pageSize);

//        const nav = document.createElement('nav');
//        const ul = document.createElement('ul');
//        ul.className = 'pagination';

//        // Önceki Sayfa
//        const prevLi = document.createElement('li');
//        prevLi.className = `page-item ${this.pageNumber === 1 ? 'disabled' : ''}`;
//        const prevLink = document.createElement('a');
//        prevLink.className = 'page-link';
//        prevLink.href = '#';
//        prevLink.innerHTML = '&laquo;';
//        prevLink.onclick = (e) => {
//            e.preventDefault();
//            if (this.pageNumber > 1) {
//                this.pageNumber--;
//                this.fetchData().then(() => this.render());
//            }
//        };
//        prevLi.appendChild(prevLink);
//        ul.appendChild(prevLi);

//        // Sayfa numaraları
//        for (let i = 1; i <= totalPages; i++) {
//            const li = document.createElement('li');
//            li.className = `page-item ${i === this.pageNumber ? 'active' : ''}`;
//            const link = document.createElement('a');
//            link.className = 'page-link';
//            link.href = '#';
//            link.textContent = i;
//            link.onclick = (e) => {
//                e.preventDefault();
//                this.pageNumber = i;
//                this.fetchData().then(() => this.render());
//            };
//            li.appendChild(link);
//            ul.appendChild(li);
//        }

//        // Sonraki Sayfa
//        const nextLi = document.createElement('li');
//        nextLi.className = `page-item ${this.pageNumber === totalPages ? 'disabled' : ''}`;
//        const nextLink = document.createElement('a');
//        nextLink.className = 'page-link';
//        nextLink.href = '#';
//        nextLink.innerHTML = '&raquo;';
//        nextLink.onclick = (e) => {
//            e.preventDefault();
//            if (this.pageNumber < totalPages) {
//                this.pageNumber++;
//                this.fetchData().then(() => this.render());
//            }
//        };
//        nextLi.appendChild(nextLink);
//        ul.appendChild(nextLi);

//        nav.appendChild(ul);
//        this.paginationContainer.appendChild(nav);
//    }

//    /**
//     * Renders the 'Add New' button
//     */
//    renderCreateButton() {
//        const btn = document.createElement("button");
//        btn.className = "btn btn-success mb-3";
//        btn.innerHTML = `<i class="bi bi-plus-circle"></i>`;
//        btn.addEventListener("click", () => this.toggleCreateForm());
//        this.container.appendChild(btn);
//    }

//    toggleCreateForm() {
//        if (!this.addFormElement) {
//            this.createAddForm();
//        }
//        const form = this.addFormElement;
//        const btnIcon = this.container.querySelector('button.btn-success i');

//        if (form.classList.contains('show')) {
//            form.classList.remove('show');
//            form.classList.add('collapse');
//            btnIcon.className = "bi bi-plus-circle";
//        } else {
//            form.classList.remove('collapse');
//            form.classList.add('show');
//            btnIcon.className = "bi bi-dash-circle";
//        }
//    }

//    createAddForm() {
//        const form = document.createElement('div');
//        form.className = 'card card-body collapse mb-3';

//        const inputs = {};
//        const allowedTypes = ['string', 'int64', 'boolean', 'datetime', 'int32'];

//        this.properties.forEach(prop => {
//            const typeLower = prop.type.toLowerCase();
//            if (prop.name.toLowerCase() === 'id' || !allowedTypes.some(t => typeLower.includes(t))) return;

//            const label = document.createElement('label');
//            label.textContent = prop.name;

//            const input = document.createElement('input');
//            input.type = 'text';
//            input.className = 'form-control form-control-sm mb-2';

//            form.appendChild(label);
//            form.appendChild(input);

//            inputs[prop.name] = input;
//        });

//        const btnGroup = document.createElement('div');
//        btnGroup.className = 'd-flex justify-content-end gap-2';
       

//        const saveBtn = document.createElement('button');
//        saveBtn.className = 'btn btn-success me-2';
        
//        saveBtn.innerHTML = `<i class="bi bi-check-circle"></i>`;
//        saveBtn.addEventListener('click', async () => {
//            const newItem = {};
//            for (const propName in inputs) {
//                newItem[propName] = inputs[propName].value;
//            }
//            const createdItem = await this.addItem(newItem);

//            form.classList.remove('show');
//            form.classList.add('collapse');

//            if (createdItem) {
//                this.displayCreatedItem(createdItem);
//            }
//        });

//        const cancelBtn = document.createElement('button');
//        cancelBtn.className = 'btn btn-danger';
//        cancelBtn.innerHTML = `<i class="bi bi-x-circle"></i>`;
//        cancelBtn.addEventListener('click', () => {
//            form.classList.remove('show');
//            form.classList.add('collapse');
//        });

//        btnGroup.appendChild(saveBtn);
//        btnGroup.appendChild(cancelBtn);
//        form.appendChild(btnGroup);

//        this.container.insertBefore(form, this.container.firstChild.nextSibling);
//        this.addFormElement = form;
//    }
//attachIntellisense(inputElement) {
//    inputElement.addEventListener('input', (e) => {
//        if (!intellisenseEnabled) return;  // Toggle kontrolü

//        const suggestions = this.getFilterSuggestions(e.target.value);
//        this.showSuggestions(inputElement, suggestions);
//    });
//}


//    /**
//     * Renders hamburger panel for filter, groupBy, select, orderBy, desc
//     */
//    renderHamburgerPanel() {
//        const accordion = document.createElement('div');
//        accordion.className = 'accordion mb-3';

//        accordion.innerHTML = `
//   <div class="accordion mb-3">
//  <div class="accordion-item">
//    <h2 class="accordion-header">
//      <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#linq-panel">
//        LINQ Panel
//      </button>
//    </h2>
//    <div id="linq-panel" class="accordion-collapse collapse">
//      <div class="accordion-body">
//        <div class="row">
//          <div class="col-md-6">
//            <label>Filter:</label>
//            <input id="filter-input" class="form-control form-control-sm mb-2" value="1=1">
//          </div>
//          <div class="col-md-6">
//            <label>GroupBy:</label>
//            <input id="groupby-input" class="form-control form-control-sm mb-2" value="">
//          </div>
//          <div class="col-md-6">
//            <label>Select:</label>
//            <input id="select-input" class="form-control form-control-sm mb-2" value="">
//          </div>
//          <div class="col-md-6">
//            <label>OrderBy:</label>
//            <input id="orderby-input" class="form-control form-control-sm mb-2" value="id">
//          </div>
//          <div class="col-md-6">
//            <div class="form-check form-switch">
//              <input id="desc-input" type="checkbox" class="form-check-input" checked>
//              <label class="form-check-label">Desc?</label>
//            </div>
//          </div>
//          <!-- Include Bölümü -->
//          <div class="col-md-12">
//            <label>Include:</label>
//            <div id="include-container" class="mb-2">
//              <!-- Dinamik include satırları burada eklenecek -->
//            </div>
//            <button id="add-include-btn" class="btn btn-secondary btn-sm">Add Include</button>
//          </div>
//          <div class="col-md-6 text-end align-self-end">
//            <button id="apply-query-btn" class="btn btn-primary btn-sm">Apply Query</button>
//          </div>
//        </div>
//      </div>
//    </div>
//  </div>
//</div>
//`;

//        this.container.appendChild(accordion);

//        // Element referanslarını al
//        const filterInput = accordion.querySelector('#filter-input');
//        const groupByInput = accordion.querySelector('#groupby-input');
//        const selectInput = accordion.querySelector('#select-input');
//        const orderByInput = accordion.querySelector('#orderby-input');
//        const descInput = accordion.querySelector('#desc-input');
//        const applyQueryBtn = accordion.querySelector('#apply-query-btn');

//        // IntelliSense ekle
//        this.attachIntellisense(filterInput, 'filter');
//        this.attachIntellisense(groupByInput, 'groupby');
//        this.attachIntellisense(selectInput, 'select');

//        // Ctrl+Space desteği için event listener ekle
//        [filterInput, groupByInput, selectInput].forEach((inputElement, index) => {
//            const type = ['filter', 'groupby', 'select'][index];
//            inputElement.addEventListener('keydown', (e) => {
//                if (e.ctrlKey && e.code === 'Space') {
//                    e.preventDefault();
//                    this.showIntellisenseSuggestions(inputElement, type);
//                }
//            });
//        });

//        // Butona tıklanınca değerleri güncelle ve veriyi yenile
//        applyQueryBtn.addEventListener('click', async () => {
//            this.filter = filterInput.value;
//            this.groupBy = groupByInput.value;
//            this.select = selectInput.value;
//            this.orderBy = orderByInput.value;
//            this.desc = descInput.checked;

//            this.pageNumber = 1;
//            await this.fetchData();
//            this.render();
//        });
//    }



 
//    showIntellisenseSuggestions(inputEl, context) {
//        let dropdown = inputEl.nextElementSibling;
//        if (dropdown && dropdown.classList.contains('dropdown-menu')) {
//            dropdown.remove();
//        }

//        dropdown = document.createElement('div');
//        dropdown.className = 'dropdown-menu show';
//        dropdown.style.width = '100%';

//        const suggestions = this.intellisenseMap[context]?.String || [];

//        suggestions.forEach(s => {
//            const option = document.createElement('button');
//            option.className = 'dropdown-item';
//            option.textContent = s.name;

//            option.addEventListener('click', () => {
//                inputEl.value += `.${s.name}`;
//                dropdown.remove();
//            });

//            if (s.tooltip) {
//                option.setAttribute('data-bs-toggle', 'tooltip');
//                option.setAttribute('data-bs-placement', 'right');
//                option.title = s.tooltip;
//                new bootstrap.Tooltip(option);
//            }

//            dropdown.appendChild(option);
//        });

//        inputEl.parentElement.style.position = 'relative';
//        inputEl.parentElement.appendChild(dropdown);

//        document.addEventListener('click', (e) => {
//            if (!inputEl.contains(e.target)) {
//                dropdown.remove();
//            }
//        }, { once: true });
//    }

//    renderTableHeader() {
//        this.theadElement.innerHTML = '';
//        const headerRow = document.createElement('tr');

//        const columns = this.properties.map(p => p.name);

//        columns.forEach(columnName => {
//            const th = document.createElement('th');
//            let displayName = columnName;

//            if (this.orderBy.toLowerCase() === columnName.toLowerCase()) {
//                const arrowIcon = this.desc
//                    ? '<i class="bi bi-arrow-down"></i>'
//                    : '<i class="bi bi-arrow-up"></i>';
//                displayName += ` ${arrowIcon}`;
//            }

//            th.innerHTML = displayName;
//            th.style.cursor = 'pointer';

//            th.addEventListener('click', () => {
//                if (this.orderBy === columnName) {
//                    this.desc = !this.desc;
//                } else {
//                    this.orderBy = columnName;
//                    this.desc = false;
//                }
//                this.pageNumber = 1;
//                this.fetchData().then(() => this.render());
//            });

//            headerRow.appendChild(th);
//        });

//        this.theadElement.appendChild(headerRow);
//    }

//    renderTableBody() {
//        this.tbodyElement.innerHTML = '';

//        this.data.forEach(item => {
//            const row = document.createElement('tr');

//            row.onclick = () => this.onRowSelected(row, item);

//            this.properties.forEach(prop => {
//                const key = prop.name.charAt(0).toLowerCase() + prop.name.slice(1);
//                const td = document.createElement('td');
//                td.textContent = item[key];

//                const allowedTypes = ['string', 'int64', 'boolean', 'datetime', 'int32'];
//                const typeLower = prop.type.toLowerCase();

//                // Editable kontrolü (id ve collection değilse editable)
//                if (prop.name !== 'Id' && !prop.type.includes('ICollection') && allowedTypes.some(t => typeLower.includes(t))) {
//                    td.onclick = (e) => {
//                        e.stopPropagation();
//                        this.editCell(td, item, prop);
//                    };
//                    td.style.cursor = 'pointer';
//                }

//                row.appendChild(td);
//            });

//            // Update butonu kolonunu buraya ekle
//            const editTd = document.createElement('td');
//            const editBtn = document.createElement('button');
//            editBtn.className = 'btn btn-sm btn-warning';
//            editBtn.innerHTML = '<i class="bi bi-pencil"></i>';
//            editBtn.onclick = (e) => {
//                e.stopPropagation();
//                this.openUpdateForm(item);
//            };
//            editTd.appendChild(editBtn);
//            row.appendChild(editTd);

//            this.tbodyElement.appendChild(row);
//        });
//    }


//    // Seçilen row yönetimi
//    onRowSelected(rowElement, rowData) {
//        if (this.selectedRow) {
//            this.selectedRow.classList.remove('table-active');
//        }
//        rowElement.classList.add('table-active');
//        this.selectedRow = rowElement;
//        this.selectedData = rowData;
//    }

//    editCell(td, rowObj, prop) {
//        if (td.querySelector("input")) return;
//        const oldVal = td.textContent;
//        td.innerHTML = "";
//        const input = document.createElement("input");
//        input.type = "text";
//        input.value = oldVal;
//        input.className = "form-control";
//        td.appendChild(input);
//        input.focus();

//        const finish = (save) => {
//            if (save) {
//                const key = prop.name.charAt(0).toLowerCase() + prop.name.slice(1);
//                rowObj[key] = input.value;
//                this.saveItem(rowObj);
//                td.textContent = input.value;
//            } else {
//                td.textContent = oldVal;
//            }
//        };

//        input.onblur = () => finish(true);
//        input.onkeydown = (e) => {
//            if (e.key === "Enter") finish(true);
//            else if (e.key === "Escape") finish(false);
//        };
//    }


//    // Dışarıda "Update" butonu oluştur:
//    renderUpdateButton() {
//        const btn = document.createElement('button');
//        btn.className = 'btn btn-warning mb-3';
//        btn.innerHTML = `<i class="bi bi-pencil"></i> Update`;
//        btn.onclick = () => this.openUpdateForm();
//        this.container.appendChild(btn);
//    }

//    // case insensitive filter
//     searchProps = (input, properties) => properties.filter(
//        p => p.name.toLowerCase().includes(input.toLowerCase())
//    );

//    // Property tipini al
//     getPropertyType = (propName) => {
//        const prop = this.properties.find(p => p.name === propName);
//        return prop ? prop.type.toLowerCase() : null;
//    };

//    // Valid filtre regex (basitleştirilmiş)
//     isValidFilterSegment = (segment) => {
//        const regex = /(\w+)\s*(==|>|<|>=|<=|!=)\s*(\w+|".*"|'.*')/;
//        const funcRegex = /(\w+)\.(Contains|StartsWith|EndsWith)\(["'][^"']+["']\)/;
//        return regex.test(segment.trim()) || funcRegex.test(segment.trim());
//    };


//    // Update form açılması:
//    openUpdateForm(item) {
//        if (this.updateFormElement) this.updateFormElement.remove();

//        if (!item) {
//            alert("Önce bir satır seçmelisiniz.");
//            return;
//        }

//        const form = document.createElement('div');
//        form.className = 'card card-body mb-3';

//        const inputs = {};
//        const allowedTypes = ['string', 'int64', 'boolean', 'datetime', 'int32'];

//        const row = document.createElement('div');
//        row.className = 'row';

//        this.properties.forEach(prop => {
//            const typeLower = prop.type.toLowerCase();
//            if (prop.type.includes('ICollection') || prop.name === 'Id' || !allowedTypes.some(t => typeLower.includes(t)))
//                return;

//            const col = document.createElement('div');
//            col.className = 'col-md-6 mb-2';

//            const label = document.createElement('label');
//            label.className = 'form-label';
//            label.textContent = prop.name;
//            function pascalToCamel(str) {
//                return str.charAt(0).toLowerCase() + str.slice(1);
//            }
//            const input = document.createElement('input');
//            input.className = 'form-control';
//            input.value = item[pascalToCamel(prop.name)];
           
//            inputs[prop.name] = input;
//            col.appendChild(label);
//            col.appendChild(input);
//            row.appendChild(col);
//        });

//        form.appendChild(row);

//        const btnGroup = document.createElement('div');
//        btnGroup.className = 'd-flex justify-content-end gap-2';

//        const saveBtn = document.createElement('button');
//        saveBtn.className = 'btn btn-success';
//        saveBtn.textContent = 'Update';
//        saveBtn.onclick = async () => {
//            const updateItem = { id: item.id };
//            Object.keys(inputs).forEach(k => updateItem[k] = inputs[k].value);
//            await this.saveItem(updateItem);
//            form.remove();
//            await this.fetchData();
//            this.render();
//        };

//        const cancelBtn = document.createElement('button');
//        cancelBtn.className = 'btn btn-danger';
//        cancelBtn.textContent = 'Cancel';
//        cancelBtn.onclick = () => form.remove();

//        btnGroup.appendChild(saveBtn);
//        btnGroup.appendChild(cancelBtn);
//        form.appendChild(btnGroup);

//        this.container.prepend(form);
//        this.updateFormElement = form;
//    }
//    // Basit PascalCase → camelCase dönüştürücü


//    // Form oluşturucu ortak metot:
//    createForm(type, data = {}) {
//        const form = document.createElement('div');
//        form.className = 'card card-body collapse mb-3';

//        const inputs = {};

//        this.properties.forEach(prop => {
//            if (prop.type.includes('ICollection') || prop.name === 'Id') return; // Complex & Id alanını çıkar

//            const label = document.createElement('label');
//            label.textContent = prop.name;

//            const input = document.createElement('input');
//            input.type = 'text';
//            input.className = 'form-control mb-2';
//            input.value = data[prop.name] || '';

//            form.appendChild(label);
//            form.appendChild(input);
//            inputs[prop.name] = input;
//        });

//        const saveBtn = document.createElement('button');
//        saveBtn.className = 'btn btn-success me-2';
//        saveBtn.innerHTML = `<i class="bi bi-check-circle"></i>`;
//        saveBtn.onclick = async () => {
//            const item = {};
//            for (const prop in inputs) {
//                item[prop] = inputs[prop].value;
//            }

//            if (type === 'Create') {
//                await this.addItem(item);
//            } else if (type === 'Update') {
//                item.id = data.id; // ID ekleniyor
//                await this.saveItem(item);
//            }
//            form.classList.remove('show');
//        };

//        const cancelBtn = document.createElement('button');
//        cancelBtn.className = 'btn btn-danger';
//        cancelBtn.innerHTML = `<i class="bi bi-x-circle"></i>`;
//        cancelBtn.onclick = () => form.classList.remove('show');

//        form.appendChild(saveBtn);
//        form.appendChild(cancelBtn);
//        this.container.appendChild(form);

//        return form;
//    }

//    // Form doldurucu:
//    populateForm(form, data) {
//        [...form.querySelectorAll('input')].forEach(input => {
//            const key = input.previousElementSibling.textContent;
//            input.value = data[key] || '';
//        });
//    }

//    // saveItem (PUT):
//    async saveItem(updatedItem) {
//        const url = `${this.apiPrefix}${this.controller}/${updatedItem.id}`;

//        const res = await fetch(url, {
//            method: 'PUT',
//            headers: { 'Content-Type': 'application/json' },
//            body: JSON.stringify(updatedItem)
//        });

//        if (!res.ok) {
//            const err = await res.text();
//            this.showError(`Update failed: ${err}`);
//        } else {
//            await this.fetchData();
//            this.render();
//        }
//    }

//    // addItem (POST):
//    async addItem(newItem) {
//        const url = `${this.apiPrefix}${this.controller}`;

//        const res = await fetch(url, {
//            method: 'POST',
//            headers: { 'Content-Type': 'application/json' },
//            body: JSON.stringify(newItem)
//        });

//        if (!res.ok) {
//            const err = await res.text();
//            alert(`Create failed: ${err}`);
//        } else {
//            const created = await res.json();
//            alert(`Created item ID: ${created.id}`);
//            await this.fetchData();
//            this.render();
//        }
//    }
//    // basit hata gösterimi
//    showError(message) {
//        const alertEl = document.createElement('div');
//        alertEl.className = 'alert alert-danger alert-dismissible fade show';
//        alertEl.innerHTML = `${message}<button type="button" class="btn-close" data-bs-dismiss="alert"></button>`;
//        this.container.prepend(alertEl);
//    }
//    displayCreatedItem(createdItem) {
//        const div = document.createElement('div');
//        div.className = 'alert alert-info alert-sm mt-2';
//        div.innerHTML = `<b>Newly created:</b> ${JSON.stringify(createdItem)}`;
//        this.addFormElement.after(div);

//        setTimeout(() => div.remove(), 5000); // 5 saniye sonra silinsin
//    }
   
//    getFilterSuggestions(currentInput) {
//        const tokens = currentInput.trim().split(/\s+/);
//        const lastToken = tokens[tokens.length - 1];

//        // Eğer boş ise, direkt property'leri öner
//        if (!currentInput || currentInput.trim() === "") {
//            return this.properties.map(p => p.name);
//        }

//        // AND / OR önerisi kontrolü
//        if (tokens.length > 0 && isValidFilterSegment(tokens.slice(-3).join(' '))) {
//            if (currentInput.endsWith(' ')) {
//                return ['AND', 'OR'];
//            }
//        }

//        // Property ve operatör önerisi
//        if (lastToken.includes('.')) {
//            const [propName, methodPart] = lastToken.split('.');
//            const propType = getPropertyType(propName);
//            if (propType.includes('string')) {
//                return ["Contains(\"\")", "StartsWith(\"\")", "EndsWith(\"\")", "Length"];
//            } else if (propType.includes('int') || propType.includes('datetime')) {
//                return ['Year', 'Month', 'Day'];
//            }
//        } else if (this.properties.some(p => p.name.toLowerCase() === lastToken.toLowerCase())) {
//            const propType = getPropertyType(lastToken);
//            if (propType.includes('string')) {
//                return ["==", "!=", ".Contains(\"\")", ".StartsWith(\"\")"];
//            } else if (propType.includes('int') || propType.includes('datetime')) {
//                return ["==", "!=", ">", "<", ">=", "<="];
//            } else if (propType.includes('bool')) {
//                return ["== true", "== false"];
//            }
//        }

//        // Hala property önerisi yapabiliyor muyuz kontrol et:
//        return searchProps(lastToken, this.properties).map(p => p.name);
//    }
//    showSuggestions(inputEl, suggestions) {
//        let dropdown = inputEl.nextElementSibling;
//        if (dropdown && dropdown.classList.contains('dropdown-menu')) {
//            dropdown.remove();
//        }

//        dropdown = document.createElement('ul');
//        dropdown.className = 'dropdown-menu show';
//        dropdown.style.width = inputEl.offsetWidth + 'px';

//        suggestions.forEach(s => {
//            const li = document.createElement('li');
//            li.innerHTML = `<a class="dropdown-item">${s}</a>`;
//            li.onclick = () => {
//                const inputParts = inputEl.value.split(/\s+/);
//                inputParts.pop();
//                inputParts.push(s);
//                inputEl.value = inputParts.join(' ') + ' ';
//                dropdown.remove();
//                inputEl.focus();
//            };
//            dropdown.appendChild(li);
//        });

//        inputEl.parentElement.style.position = 'relative';
//        inputEl.parentElement.appendChild(dropdown);

//        document.addEventListener('click', () => dropdown.remove(), { once: true });
//    }

//    getSuggestions(type, currentValue) {
//        if (type !== 'filter') return [];

//        const tokens = currentValue.trim().split(/\s+/);
//        const lastToken = tokens[tokens.length - 1];

//        if (currentValue.endsWith('.')) {
//            const prop = this.properties.find(p => p.name.toLowerCase() === tokens[tokens.length - 1].replace('.', '').toLowerCase());
//            if (prop) {
//                const typeLower = prop.type.toLowerCase();
//                if (typeLower.includes('string')) return ['Contains("")', 'StartsWith("")', 'EndsWith("")'];
//                if (typeLower.includes('int') || typeLower.includes('datetime')) return ['Year', 'Month', 'Day'];
//                if (typeLower.includes('bool')) return ['Equals(true)', 'Equals(false)'];
//            }
//        } else if (/(\w+)\s*(==|>|<|>=|<=)\s*["']?\w*["']?$/.test(currentValue)) {
//            return ['AND', 'OR'];
//        }

//        return this.properties.map(p => p.name);
//    }

//    showSuggestionsDropdown(inputElement, suggestions) {
//        let dropdown = document.querySelector('.dynamic-intellisense-dropdown');
//        if (dropdown) dropdown.remove();

//        dropdown = document.createElement('ul');
//        dropdown.className = 'dropdown-menu show dynamic-intellisense-dropdown';

//        const rect = inputElement.getBoundingClientRect();
//        dropdown.style.position = 'absolute';
//        dropdown.style.top = `${rect.bottom + window.scrollY}px`;
//        dropdown.style.left = `${rect.left + window.scrollX}px`;
//        dropdown.style.width = `${rect.width}px`;
//        dropdown.style.zIndex = 1000;

//        suggestions.forEach(s => {
//            const li = document.createElement('li');
//            li.innerHTML = `<a class="dropdown-item" href="#">${s}</a>`;
//            li.onclick = () => {
//                inputElement.value += s;
//                dropdown.remove();
//            };
//            dropdown.appendChild(li);
//        });

//        document.body.appendChild(dropdown);

//        document.addEventListener('click', function handler(e) {
//            if (!dropdown.contains(e.target) && e.target !== inputElement) {
//                dropdown.remove();
//                document.removeEventListener('click', handler);
//            }
//        });
//    }


//}


//// LinqStateManager: Basit state container (isteğe bağlı, event tabanlı)
//class LinqStateManager {
//    constructor(initialState = {}) {
//        this.state = initialState;
//        this.listeners = [];
//    }
//    getState() {
//        return this.state;
//    }
//    setState(newState) {
//        this.state = { ...this.state, ...newState };
//        this.notify();
//    }
//    subscribe(listener) {
//        this.listeners.push(listener);
//    }
//    notify() {
//        this.listeners.forEach(listener => listener(this.state));
//    }
//}


//// Örnek kullanım:
//document.addEventListener("DOMContentLoaded", () => {
//    const container = document.getElementById("dataTableContainer");
//    const dataTable = new DynamicDataTable({
//        controller: "Country",
//        container: container,
//        debug: true
//        // Gerekirse stateManager de dışarıdan verilebilir.
//    });
//    dataTable.init();
//});
