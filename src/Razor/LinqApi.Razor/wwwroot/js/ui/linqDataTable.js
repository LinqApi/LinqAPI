import { StateManager } from "../core/state.js";
import { QueryBuilder } from "./queryBuilder.js";
import { defaults, fetchProperties, fetchPagedData, FormManager } from "./linqUtil.js";
import { Query, LogicalFilter, Pager } from "../core/models.js";
import { LinqSelect2 } from "./linqSelect2.js";
/**
 * Ana LinqDataTable bileşeni.
 * Tabloyu oluşturur, verileri sunucudan çeker ve kullanıcı etkileşimlerini (sayfalama, sıralama, düzenleme vb.) yönetir.
 * İsteğe bağlı olarak entegre bir QueryBuilder filtre arayüzü ile birlikte çalışır.
 */
export class LinqDataTable {
    /**
     * @param {Object} options - Bileşen ayarları.
     * @param {HTMLElement|string} options.container - Tablo bileşeninin oluşturulacağı konteyner (eleman veya seçici).
     * @param {string} options.controller - İlgili API controller adı (veri çekmek için kullanılacak).
     * @param {string} [options.apiPrefix="/api"] - API istekleri için kullanılacak ön ek.
     * @param {StateManager} [options.stateManager] - Harici bir StateManager örneği. Verilmezse içerde yeni oluşturulur.
     * @param {HTMLElement|string} [options.queryBuilderContainer] - QueryBuilder için harici bir konteyner. Verilmezse, LinqDataTable kendi içinde bir konteyner oluşturur.
     * @param {boolean} [options.autoCreateQueryBuilder=true] - true ise bir QueryBuilder arayüzü oluşturulur; false ise oluşturulmaz.
     */
    constructor({
        container,
        controller,
        apiPrefix = "/api",
        stateManager = null,
        queryBuilderContainer = null,
        autoCreateQueryBuilder = true,
        customActions = [],
        select2Columns = false // New config: enable column selection
    }) {
        // Find and assign the main container element
        if (typeof container === "string") {
            this.containerElm = document.querySelector(container);
        } else {
            this.containerElm = container;
        }
        if (!this.containerElm) {
            throw new Error("LinqDataTable: Invalid container element.");
        }
        this.controller = controller;
        this.apiPrefix = apiPrefix;
        // Use provided StateManager or create a new one
        this.stateManager = stateManager || new StateManager();
        this.customActions = customActions;
        // Component states
        this.data = [];          // Data for current page
        this.totalCount = 0;     // Total record count (for pagination)
        this.pageSize = 10;      // Records per page
        this.currentSort = null; // Sorting information ({ field, dir })
        this.isLoading = false;  // Flag to indicate if data is being loaded
        this.entityProperties = null; // Property metadata for form rendering
        this.editingIndex = null; // Currently editing row index (if any)

        // New configuration for column selection in the grid
        this.select2Columns = select2Columns;
        this.selectedColumns = []; // Will hold selected column names

        // Render the top controls (e.g., "New" button)
        this._renderTopControls();
        // Create table element and add to container
        this.tableElm = document.createElement("table");
        this.tableElm.className = "table table-striped table-hover";
        this.containerElm.appendChild(this.tableElm);

        // Create pagination element and add to container
        this.pagerElm = document.createElement("div");
        this.pagerElm.className = "d-flex justify-content-between align-items-center mt-3";
        this.containerElm.appendChild(this.pagerElm);

        // Setup QueryBuilder if enabled
        if (autoCreateQueryBuilder) {
            this.queryBuilder = new QueryBuilder({
                container: queryBuilderContainer,
                stateManager: this.stateManager,
                controller: this.controller,
                apiPrefix: this.apiPrefix
            });
            // Subscribe to query updates from QueryBuilder
            this.queryBuilder.onQueryUpdated((newQuery) => {
                this.currentQuery = newQuery;
                this.currentPage = 1; // Reset to first page when query changes
                this._fetchData();  // Trigger data reload
            });
            this.stateManager.subscribe("query", (newQuery) => {
                this.currentQuery = newQuery;
                this.currentPage = 1;
                this._fetchData();
            });
        }
        // Set the current query from state or QueryBuilder
        this.currentQuery = this.stateManager.getState("query") || this.queryBuilder.query;
        if (!(this.currentQuery instanceof Query)) {
            this.currentQuery = new Query(this.controller, {
                filter: this.currentQuery.filter || new LogicalFilter("AND", [{ toString: () => "1=1" }]),
                pager: this.currentQuery.pager || new Pager(),
                orderBy: this.currentQuery.orderBy || "id",
                desc: typeof this.currentQuery.desc === "boolean" ? this.currentQuery.desc : true,
                groupBy: this.currentQuery.groupBy || "",
                select: this.currentQuery.select || "",
                includes: this.currentQuery.includes || [],
            });
        }
        // Fetch initial data
        this._fetchData();
    }
    /**
     * Tablo üst kısmındaki kontrol düğmelerini oluşturur (örn: "Yeni Ekle").
     * @private
     */
    // In LinqDataTable.js (_renderTopControls method)
    _renderTopControls() {
        const controlsDiv = document.createElement("div");
        controlsDiv.className = "mb-3"; // Margin for spacing

        // "New" button
        const addBtn = document.createElement("button");
        addBtn.type = "button";
        addBtn.textContent = "Yeni Ekle";
        addBtn.className = "btn btn-primary";
        addBtn.addEventListener("click", () => this._openCreateForm());
        controlsDiv.appendChild(addBtn);

        // --- Column Selector Container (Placeholder) ---
        if (this.select2Columns) {
            this.columnSelectContainer = document.createElement("div");
            this.columnSelectContainer.className = "mb-2"; // Additional spacing
            controlsDiv.appendChild(this.columnSelectContainer);
        }

        this.containerElm.appendChild(controlsDiv);
    }



    _createColumnSelector() {
        // Filter out displayproperty items.
        const normalProps = this.entityProperties.filter(p =>
            p.type.toLowerCase() !== "displayproperty" &&
            p.kind.toLowerCase() !== "complex" &&
            p.kind.toLowerCase() !== "complexlist"
        );
        const localData = normalProps.map(p => ({ id: p.name, name: p.name }));

        this.columnSelector = new LinqSelect2({
            container: this.columnSelectContainer,
            controller: "", // Not needed in client mode
            multiselect: true,
            renderMode: "checkbox",
            fetchMode: "client",
            localData: localData,  // Use only normal properties here.
            displayProperty: "name",
            valueField: "id",
            selectPlaceHolder: "Select columns...",
            onChange: (selectedItems) => {
                this.selectedColumns = selectedItems.map(s => s.id);
                this._renderTable(); // Re-render the table with updated columns.
            }
        });

        // Set default selection as before...
        if (localData.length > 0) {
            const idColumn = localData.find(item => item.id.toLowerCase() === "id");
            const otherColumns = localData.filter(item => item.id.toLowerCase() !== "id");
            const remainingCount = 8 - (idColumn ? 1 : 0);
            const defaultOtherColumns = otherColumns.slice(0, remainingCount);

            this.selectedColumns = [];
            if (idColumn) {
                this.selectedColumns.push(idColumn.id);
            }
            defaultOtherColumns.forEach(item => this.selectedColumns.push(item.id));

            this.columnSelector.selectedItems = localData.filter(item =>
                this.selectedColumns.includes(item.id)
            );
            this.columnSelector.updateSelectedUI();
        }
    }





    /**
     * Geçerli filtre/sayfa/sıralama bilgilerine göre sunucudan verileri çekerek tabloyu günceller.
     * @private
     */
    _fetchData() {
        if (this.isLoading) return;
        this.isLoading = true;

        // Ensure currentQuery is a Query instance
        if (!(this.currentQuery instanceof Query)) {
            this.currentQuery = new Query(this.controller, this.currentQuery);
        }

        // Fetch properties from the server
        fetchProperties(this.controller, this.apiPrefix)
            .then(props => {
                this.entityProperties = props;
                // If select2Columns is enabled, create or update the column selector
                if (this.select2Columns) {
                    if (!this.columnSelector) {
                        this._createColumnSelector();
                    } else {
                        const localData = this.entityProperties.map(p => ({ id: p.name, name: p.name }));
                        this.columnSelector.cfg.localData = localData;
                        if (!this.selectedColumns || this.selectedColumns.length === 0) {
                            this.selectedColumns = localData.slice(0, 8).map(item => item.id);
                            this.columnSelector.selectedItems = localData.filter(item => this.selectedColumns.includes(item.id));
                        }
                        this.columnSelector.updateSelectedUI();
                    }
                }
            })
            .catch(error => {
                console.error("fetchProperties error:", error);
            });

        // Continue fetching the table data...
        fetchPagedData(
            `${this.apiPrefix.replace(/\/+$/, "")}/${this.controller}/${defaults.filterPagedRoute}`,
            this.currentQuery
        )
            .then(data => {
                this.data = data.items;
                this.totalCount = data.totalCount;

                // If the query includes a select or groupBy clause,
                // derive the columns from the returned data.
                if ((this.currentQuery.select && this.currentQuery.select.trim() !== "") ||
                    (this.currentQuery.groupBy && this.currentQuery.groupBy.trim() !== "")) {
                    this.queryBuilder._updateUI();
                }

                this._renderTable();
                this._renderPager();
            })
            .catch(error => {
                console.error("Data fetch error:", error);
                this.tableElm.innerHTML = "<tr><td colspan='100%'>Data not found.</td></tr>";
            })
            .finally(() => {
                this.isLoading = false;
            });
    }



    _updateQuery(newUpdates) {
        // Merge new updates into the current query
        const updated = { ...this.currentQuery, ...newUpdates };
        // Ensure the new query is an instance of Query
        this.currentQuery = new Query(this.controller, updated);
        // Publish the updated query
        this.stateManager.setState({ query: this.currentQuery });
    }


    /**
     * Tabloyu güncel veriyle yeniden oluşturur (başlıklar ve satırlar dahil).
     * @private
     */
    // In LinqDataTable.js (_renderTable method)
    _renderTable() {
        this.tableElm.innerHTML = "";
        if (!this.data || this.data.length === 0) {
            const noDataRow = this.tableElm.insertRow();
            const cell = noDataRow.insertCell();
            cell.colSpan = 100;
            cell.textContent = "No records found";
            return;
        }

        const visibleProperties = this.getVisibleProperties();
        if (visibleProperties.length === 0) {
            this.tableElm.innerHTML = "<tr><td colspan='100%'>No columns selected.</td></tr>";
            return;
        }

        // Render header.
        const header = this.tableElm.createTHead();
        const headerRow = header.insertRow();
        visibleProperties.forEach(prop => {
            const th = document.createElement("th");
            th.textContent = prop.name;
            if (prop.kind === "simple") {
                th.style.cursor = "pointer";
                th.addEventListener("click", () => this._onHeaderClick(prop.name));
                if (this.currentQuery.orderBy === prop.name && this.currentQuery.orderBy) {
                    th.textContent += this.currentQuery.desc ? " ▲" : " ▼";
                }
            } else {
                th.style.cursor = "default";
                th.style.pointerEvents = "none";
            }
            headerRow.appendChild(th);
        });
        // Extra header for actions.
        const actionTh = document.createElement("th");
        actionTh.textContent = "Actions";
        headerRow.appendChild(actionTh);

        // Render body rows.
        const body = this.tableElm.createTBody();
        this.data.forEach((item, index) => {
            const row = body.insertRow();
            row.setAttribute("data-index", index);
            for (const prop of visibleProperties) {
                const cell = row.insertCell();
                const value = item[prop.name];
                if (prop.kind === "simple") {
                    cell.textContent = (value == null) ? "" : value;
                } else {
                    if (value) {
                        const plusBtn = document.createElement("button");
                        plusBtn.textContent = "+";
                        plusBtn.className = "btn btn-sm btn-secondary";
                        plusBtn.addEventListener("click", () => {
                            alert(`Details for ${prop.name}: ${JSON.stringify(value)}`);
                        });
                        cell.appendChild(plusBtn);
                    } else {
                        cell.textContent = "";
                    }
                }
            }
            // Create an action cell before calling renderActionCell.
            const actionCell = row.insertCell();
            this.renderActionCell(actionCell, item, index);
        });
    }




    /**
     * Sayfalama arayüzünü (ileri/geri) günceller.
     * @private
     */
    _renderPager() {
        this.pagerElm.innerHTML = "";

        // Calculate total pages from totalCount and current pageSize
        const pageSize = this.currentQuery.pager.pageSize;
        const totalPages = this.totalCount ? Math.ceil(this.totalCount / pageSize) : 1;
        const currentPage = this.currentQuery.pager.pageNumber;

        // Create container for pagination buttons
        const pagerContainer = document.createElement("div");
        pagerContainer.className = "d-flex align-items-center";

        // Previous button using arrow icon
        const prevBtn = document.createElement("button");
        prevBtn.innerHTML = "&lt;"; // '<' arrow icon
        prevBtn.className = "btn btn-secondary me-2";
        prevBtn.disabled = currentPage <= 1;
        prevBtn.addEventListener("click", () => {
            if (currentPage > 1) {
                this._updateQuery({ pager: new Pager(currentPage - 1, pageSize) });
                this._fetchData();
            }
        });
        pagerContainer.appendChild(prevBtn);

        // Function to create page number buttons
        const createPageButton = (page) => {
            const btn = document.createElement("button");
            btn.textContent = page;
            btn.className = "btn btn-light mx-1";
            if (page === currentPage) {
                btn.classList.add("active");
            }
            btn.addEventListener("click", () => {
                if (page !== currentPage) {
                    this._updateQuery({ pager: new Pager(page, pageSize) });
                    this._fetchData();
                }
            });
            return btn;
        };

        // Always show the first page button
        pagerContainer.appendChild(createPageButton(1));

        // Determine start and end for page number buttons
        let startPage = Math.max(2, currentPage);
        let endPage = Math.min(totalPages - 1, currentPage + 2);

        // Add ellipsis if startPage > 2
        if (startPage > 2) {
            const ellipsis = document.createElement("span");
            ellipsis.textContent = "...";
            ellipsis.className = "mx-1";
            pagerContainer.appendChild(ellipsis);
        }

        // Add page number buttons from startPage to endPage
        for (let i = startPage; i <= endPage; i++) {
            pagerContainer.appendChild(createPageButton(i));
        }

        // Add ellipsis if endPage < totalPages - 1
        if (endPage < totalPages - 1) {
            const ellipsis = document.createElement("span");
            ellipsis.textContent = "...";
            ellipsis.className = "mx-1";
            pagerContainer.appendChild(ellipsis);
        }

        // Always show the last page button if totalPages > 1
        if (totalPages > 1) {
            pagerContainer.appendChild(createPageButton(totalPages));
        }

        // Next button using arrow icon
        const nextBtn = document.createElement("button");
        nextBtn.innerHTML = "&gt;"; // '>' arrow icon
        nextBtn.className = "btn btn-secondary ms-2";
        nextBtn.disabled = currentPage >= totalPages;
        nextBtn.addEventListener("click", () => {
            if (currentPage < totalPages) {
                this._updateQuery({ pager: new Pager(currentPage + 1, pageSize) });
                this._fetchData();
            }
        });
        pagerContainer.appendChild(nextBtn);

        // Page-size dropdown
        const pageSizeSelect = document.createElement("select");
        pageSizeSelect.className = "form-select ms-3";
        const pageSizeOptions = [10, 25, 50, 100, 250, 500];
        pageSizeOptions.forEach(size => {
            const option = document.createElement("option");
            option.value = size;
            option.textContent = size;
            if (size === pageSize) {
                option.selected = true;
            }
            pageSizeSelect.appendChild(option);
        });
        pageSizeSelect.addEventListener("change", () => {
            const newSize = parseInt(pageSizeSelect.value, 10);
            // Reset to first page when page size changes
            this._updateQuery({ pager: new Pager(1, newSize) });
            this._fetchData();
        });
        pagerContainer.appendChild(pageSizeSelect);

        // Append the pager container to the pager element
        this.pagerElm.appendChild(pagerContainer);
    }


    /**
     * Sütun başlığına tıklandığında çağrılır; sıralamayı uygular ve veriyi yeniler.
     * @param {string} field - Sıralanacak alan adı.
     * @private
     */
    // Mevcut _onHeaderClick fonksiyonunu aşağıdaki gibi güncelleyin:
    _onHeaderClick(field) {
        if (this.currentQuery && this.currentQuery.orderBy === field) {
            this._updateQuery({ desc: !this.currentQuery.desc });
        } else {
            this._updateQuery({ orderBy: field, desc: false });
        }
        this.currentPage = 1;
        this._fetchData();
    }


    /**
     * Belirtilen satırı düzenleme moduna geçirir (inline edit).
     * @param {number} index - Düzenlemeye alınacak satırın indeks değeri (this.data içinde).
     * @private
     */
    _openInlineEdit(index) {
        // Use the centralized visible properties
        const visibleProperties = this.getVisibleProperties();
        const row = this.tableElm.querySelector(`tr[data-index='${index}']`);
        if (!row) return;
        const item = this.data[index];
        const cells = row.cells;
        // Assume the number of editable fields corresponds to visibleProperties (minus the last action cell)
        visibleProperties.forEach((prop, i) => {
            if (prop.name.toLowerCase() === "id") {
                return; // skip editing the id
            }
            const cell = cells[i];
            const oldValue = item[prop.name] == null ? "" : item[prop.name];
            cell.innerHTML = "";
            let inputType = "text";
            if (typeof item[prop.name] === "number") inputType = "number";
            else if (typeof item[prop.name] === "boolean") inputType = "checkbox";
            else if (typeof item[prop.name] === "string" && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(oldValue)) inputType = "date";
            const input = document.createElement("input");
            input.name = prop.name;
            input.type = inputType;
            input.className = "form-control";
            if (inputType === "checkbox") {
                input.checked = Boolean(oldValue);
            } else if (inputType === "date") {
                input.value = oldValue ? String(oldValue).substring(0, 10) : "";
            } else {
                input.value = oldValue;
            }
            cell.appendChild(input);
        });
        // Then update the action cell as before.
        const actionCell = cells[cells.length - 1];
        actionCell.innerHTML = "";
        this.renderInlineActionCell(actionCell, item, index);
        this.editingIndex = index;
    }


    /**
     * Inline düzenleme modundaki satırı kaydeder (sunucuya güncelleme isteği gönderir).
     * @param {number} index - Düzenlenen satırın indeksi.
     * @private
     */
    _saveInlineEdit(index) {
        const row = this.tableElm.querySelector(`tr[data-index='${index}']`);
        if (!row) return;
        const item = this.data[index];
        const inputs = row.querySelectorAll("input");
        const updatedData = { ...item }; // Clone existing data

        inputs.forEach(input => {
            const field = input.name;
            const propMeta = this.entityProperties.find(p => p.name.toLowerCase() === field.toLowerCase());

            let value;
            if (input.type === "checkbox") {
                value = input.checked;
            } else if (input.type === "number") {
                value = input.value === "" ? null : Number(input.value);
            } else if (input.type === "date") {
                value = input.value; // YYYY-MM-DD format
            } else {
                value = input.value;
            }

            // 1. Boş olan complex/complexList alanları atla
            if (propMeta && (propMeta.kind === "complex" || propMeta.kind === "complexList") && (value === "" || value === null)) {
                return;
            }

            // 2. Diğer boş değerleri de filtrele (örneğin boş string vs)
            if (
                value === null ||
                value === undefined ||
                (typeof value === "string" && value.trim() === "")
            ) {
                return;
            }

            updatedData[field] = value;
        });


        const idField = Object.keys(item).find(k => k.toLowerCase() === "id") || "id";
        const idValue = item[idField];
        const url = `${this.apiPrefix.replace(/\/+$/, "")}/${this.controller}/${idValue}`;
        fetch(url, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(updatedData)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`Update error: ${response.status}`);
                }
                return response.json();
            })
            .then(updatedRecord => {
                // Update local data and re-render row.
                this.data[index] = updatedRecord;
                this.editingIndex = null;
                this._renderTable();
            })
            .catch(error => {
                console.error("Record update error:", error);
            });
    }


    /**
     * Inline düzenleme modunu iptal eder, satırı orijinal haline döndürür.
     * @param {number} index - İptal edilecek satır indeksi.
     * @private
     */
    _cancelInlineEdit(index) {
        if (this.editingIndex === index) {
            this.editingIndex = null;
        }
        this._renderTable();
    }

    /**
 * Returns an array of property metadata objects for grid display.
 */
    getVisibleProperties() {
        let visibleProperties = [];
        if ((this.currentQuery.select && this.currentQuery.select.trim() !== "") ||
            (this.currentQuery.groupBy && this.currentQuery.groupBy.trim() !== "")) {
            // Derive properties dynamically from the first data item.
            const dataKeys = Object.keys(this.data[0] || {});
            visibleProperties = dataKeys.map(key => ({ name: key, kind: "simple" }));
        } else {
            // Use fetched entityProperties and filter out non-grid fields.
            visibleProperties = (this.entityProperties || []).filter(p =>
                p.type.toLowerCase() !== "displayproperty" &&
                p.kind.toLowerCase() !== "complex" &&
                p.kind.toLowerCase() !== "complexlist"
            );
            if (this.select2Columns && this.selectedColumns && this.selectedColumns.length > 0) {
                visibleProperties = visibleProperties.filter(p =>
                    this.selectedColumns.includes(p.name)
                );
            }
        }
        // Ensure 'id' always comes first if it exists.
        visibleProperties.sort((a, b) => {
            if (a.name.toLowerCase() === "id") return -1;
            if (b.name.toLowerCase() === "id") return 1;
            return 0;
        });
        return visibleProperties;
    }

    /**
* Renders the action cell for a given row.
* This method creates the default buttons (e.g., "Edit", "Nested")
* and appends any custom actions provided.
*
* @param {HTMLTableCellElement} actionCell - The cell element where the buttons will be appended.
* @param {object} item - The data item of the row.
* @param {number} index - The index of the row.
*/
    renderActionCell(actionCell, item, index) {
        // Clear the cell
        actionCell.innerHTML = "";

        // Create the default "Edit" button
        const editBtn = document.createElement("button");
        editBtn.textContent = "Edit";
        editBtn.className = "btn btn-sm btn-primary me-1";
        editBtn.addEventListener("click", () => this._openInlineEdit(index));
        actionCell.appendChild(editBtn);


        // Add any custom actions
        this.customActions.forEach(action => {
            const btn = document.createElement("button");
            btn.textContent = action.label;
            btn.className = action.className || "btn btn-sm btn-warning ms-1";
            btn.addEventListener("click", () => {
                // Call the custom callback with the data item and its index.
                action.onClick(item, index);
            });
            actionCell.appendChild(btn);
        });
    }


    /**
 * Renders the inline action cell that shows Save and Cancel buttons
 * for the inline edit mode.
 *
 * @param {HTMLTableCellElement} actionCell - The cell element for inline buttons.
 * @param {object} item - The data item of the row.
 * @param {number} index - The index of the row.
 */
    renderInlineActionCell(actionCell, item, index) {
        // Clear the cell
        actionCell.innerHTML = "";

        // Create the Save (Kaydet) button
        const saveBtn = document.createElement("button");
        saveBtn.textContent = "Save";
        saveBtn.className = "btn btn-sm btn-success me-1";
        saveBtn.addEventListener("click", () => this._saveInlineEdit(index));
        actionCell.appendChild(saveBtn);

        // Create the Cancel (İptal) button
        const cancelBtn = document.createElement("button");
        cancelBtn.textContent = "Cancel";
        cancelBtn.className = "btn btn-sm btn-secondary";
        cancelBtn.addEventListener("click", () => this._cancelInlineEdit(index));
        actionCell.appendChild(cancelBtn);
    }


    // LinqDataTable içinde _openCreateForm fonksiyonunu aşağıdaki gibi güncelleyin:
    _openCreateForm() {
        if (!this.entityProperties) {
            fetchProperties(this.controller, this.apiPrefix)
                .then(props => {
                    this.entityProperties = props;
                    this._showCreateForm();
                })
                .catch(error => {
                    console.error("Prop List could not got error, Can not create the form", error);
                });
        } else {
            this._showCreateForm();
        }
    }


    _showCreateForm() {
        // Eğer form-container adında bir element yoksa oluştur.
        let formContainer = this.containerElm.querySelector('.form-container');
        if (!formContainer) {
            formContainer = document.createElement('div');
            formContainer.className = 'form-container mb-3'; // mb-3 ile alt boşluk ekleniyor
            // Form container'ı, tablo (this.tableElm) elementinin hemen öncesine ekliyoruz
            this.containerElm.insertBefore(formContainer, this.tableElm);
        } else {
            // Önceki içeriği temizle
            formContainer.innerHTML = '';
        }

        // Formu, oluşturduğumuz formContainer içine render ediyoruz.
        FormManager.createForm(formContainer, this.entityProperties, {}, {
            mode: "Create",
            onSave: (newRecord) => {
                // Boş değerleri temizle
                const cleanedRecord = {};
                for (const key in newRecord) {
                    const value = newRecord[key];
                    if (
                        value !== null &&
                        value !== undefined &&
                        (typeof value !== "string" || value.trim() !== "")
                    ) {
                        cleanedRecord[key] = value;
                    }
                }

                const url = `${this.apiPrefix.replace(/\/+$/, "")}/${this.controller}`;
                fetch(url, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(cleanedRecord) // Temizlenmiş obje gönderiliyor
                })
                    .then(response => {
                        if (!response.ok) throw new Error(`Kayıt oluşturulamadı: ${response.status}`);
                        return response.json();
                    })
                    .then(createdRecord => {
                        this._fetchData();
                    })
                    .catch(error => console.error("Yeni kayıt oluşturma hatası:", error));
            }
        });
    }


    _openUpdateForm(index) {
        const simpleProperties = this.entityProperties.filter(p =>
            p.kind.toLowerCase() !== "complex" &&
            p.kind.toLowerCase() !== "complexlist"
        );
        const item = this.data[index];
        // FormManager ile update formunu açıyoruz; initialData olarak mevcut item'ı gönderiyoruz.
        FormManager.createForm(this.containerElm, simpleProperties, item, {
            mode: "Update",
            onSave: (updatedItem) => {
                // Boş alanları temizle
                const cleanedItem = {};
                for (const key in updatedItem) {
                    const value = updatedItem[key];
                    if (
                        value !== null &&
                        value !== undefined &&
                        (typeof value !== "string" || value.trim() !== "")
                    ) {
                        cleanedItem[key] = value;
                    }
                }

                const idField = Object.keys(item).find(k => k.toLowerCase() === "id") || "id";
                const idValue = item[idField];
                const url = `${this.apiPrefix.replace(/\/+$/, "")}/${this.controller}/${idValue}`;

                fetch(url, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(cleanedItem)
                })
                    .then(response => {
                        if (!response.ok) throw new Error(`Güncelleme hatası: ${response.status}`);
                        return response.json();
                    })
                    .then(updatedRecord => {
                        this.data[index] = updatedRecord;
                        this._fetchData();
                    })
                    .catch(error => console.error("Kayıt güncelleme hatası:", error));
            }


        });
    }
}