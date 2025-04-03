import { StateManager } from "../core/state.js";
import { QueryBuilder } from "./queryBuilder.js";
import { defaults, fetchProperties, fetchPagedData, FormManager } from "./linqutil.js";
import { Query, LogicalFilter, Pager } from "../core/models.js";

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
    constructor({ container, controller, apiPrefix = "/api", stateManager = null, queryBuilderContainer = null, autoCreateQueryBuilder = true }) {
        // Ana konteyner elementini bul
        if (typeof container === "string") {
            this.containerElm = document.querySelector(container);
        } else {
            this.containerElm = container;
        }
        if (!this.containerElm) {
            throw new Error("LinqDataTable: Geçersiz container elementi.");
        }
        this.controller = controller;
        this.apiPrefix = apiPrefix;
        // StateManager: dışarıdan verilmişse kullan, yoksa yeni oluştur
        this.stateManager = stateManager || new StateManager();
        // Mevcut query durumunu al (başlangıçta varsa)
        //this.currentQuery = this.stateManager.getState("query") || null;

        // Bileşen durumları
        this.data = [];           // Tablo verileri (mevcut sayfanın kayıtları)
        this.totalCount = 0;      // Toplam kayıt sayısı (sayfalama için)
        this.pageSize = 10;       // Sayfa başına kayıt adedi
        this.currentPage = 1;     // Şu anki sayfa numarası
        this.currentSort = null;  // Mevcut sıralama bilgisi ({ field, dir })
        this.isLoading = false;   // Veri çekme işleminin devam edip etmediği
        this.entityProperties = null;   // Alan listesi (özellik meta verileri), formlarda kullanılabilir
        this.editingIndex = null; // Halen düzenleme modunda olan satır indeksi (varsa)


       
        // Tablo üstü kontrol panelini oluştur (ör: "Yeni Ekle" butonu)
        this._renderTopControls();
        // Tablo elementini oluştur
        this.tableElm = document.createElement("table");
        this.tableElm.className = "table table-striped table-hover"; // Bootstrap table classes
        this.containerElm.appendChild(this.tableElm);

        this.pagerElm = document.createElement("div");
        this.pagerElm.className = "d-flex justify-content-between align-items-center mt-3"; // Flex container for pager
        this.containerElm.appendChild(this.pagerElm);

        if (autoCreateQueryBuilder) {
            this.queryBuilder = new QueryBuilder({
                container: queryBuilderContainer,
                stateManager: this.stateManager,
                controller: this.controller,
                apiPrefix: this.apiPrefix
            });

            // Subscribe to query updates from the QueryBuilder
            this.queryBuilder.onQueryUpdated((newQuery) => {
                this.currentQuery = newQuery;
                this.currentPage = 1; // Reset to the first page when query changes
                this._fetchData();  // Trigger data reload
            });


            this.stateManager.subscribe("query", (newQuery) => {
                this.currentQuery = newQuery;
                this.currentPage = 1;
                this._fetchData();
            });

        }
        // Get the query from state manager or from the queryBuilder
        this.currentQuery = this.stateManager.getState("query") || this.queryBuilder.query;
        // Always wrap if it’s not a Query instance:
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
        // Başlangıçta veriyi çek
        this._fetchData();
    }

    /**
     * Tablo üst kısmındaki kontrol düğmelerini oluşturur (örn: "Yeni Ekle").
     * @private
     */
    _renderTopControls() {
        const controlsDiv = document.createElement("div");
        controlsDiv.className = "mb-3"; // margin-bottom for spacing
        // "Yeni Ekle" button with Bootstrap classes:
        const addBtn = document.createElement("button");
        addBtn.type = "button";
        addBtn.textContent = "Yeni Ekle";
        addBtn.className = "btn btn-primary"; // Bootstrap primary button
        addBtn.addEventListener("click", () => this._openCreateForm());
        controlsDiv.appendChild(addBtn);
        this.containerElm.appendChild(controlsDiv);
    }

    /**
     * Geçerli filtre/sayfa/sıralama bilgilerine göre sunucudan verileri çekerek tabloyu günceller.
     * @private
     */
    _fetchData() {
        if (this.isLoading) return;
        this.isLoading = true;

        // Ensure currentQuery is an instance of Query
        if (!(this.currentQuery instanceof Query)) {
            this.currentQuery = new Query(this.controller, this.currentQuery);
        }

        fetchProperties(this.controller, this.apiPrefix)
            .then(props => {
                this.entityProperties = props;
            })


        fetchPagedData(
            `${this.apiPrefix.replace(/\/+$/, "")}/${this.controller}/${defaults.filterPagedRoute}`,
            this.currentQuery
        )
            .then(data => {
                // data is now an object with 'items' and 'totalCount'
                this.data = data.items;
                // Use the returned totalCount directly
                this.totalCount = data.totalCount;
                // Update the pager part of the Query and publish
                //this._updateQuery({
                //    pager: new Pager(this.currentPage, this.pageSize)
                //});
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
    _renderTable() {
        this.tableElm.innerHTML = "";
        if (!this.data || this.data.length === 0) {
            const noDataRow = this.tableElm.insertRow();
            const cell = noDataRow.insertCell();
            cell.colSpan = 100;
            cell.textContent = "Kayıt bulunamadı";
            return;
        }
        const header = this.tableElm.createTHead();
        const headerRow = header.insertRow();
        let fields;
        if (this.data.length > 0) {
            if (this.entityProperties && Array.isArray(this.entityProperties)) {
                fields = this.entityProperties.filter(p => p && typeof p === "object")
                    .map(p => p.name || p.type || p);
            } else {
                fields = Object.keys(this.data[0]);
            }
        } else {
            fields = [];
        }

        for (const field of fields) {
            const th = document.createElement("th");
            th.textContent = field;
            th.style.cursor = "pointer";
            th.addEventListener("click", () => this._onHeaderClick(field));
            if (this.currentQuery.orderBy === field && this.currentQuery.orderBy) {
                th.textContent += this.currentQuery.desc === true ? " ▲" : " ▼";
            }
            headerRow.appendChild(th);
        }
       
        const actionTh = document.createElement("th");
        actionTh.textContent = "İşlemler";
        headerRow.appendChild(actionTh);

        const body = this.tableElm.createTBody();
        this.data.forEach((item, index) => {
            const row = body.insertRow();
            row.setAttribute("data-index", index);
            for (const field of fields) {
                const cell = row.insertCell();
                let text = item[field];
                if (text === null || text === undefined) text = "";
                cell.textContent = text;
            }
            const actionCell = row.insertCell();
            // Edit button using Bootstrap
            const editBtn = document.createElement("button");
            editBtn.textContent = "Düzenle";
            editBtn.className = "btn btn-sm btn-primary me-1";
            // IMPORTANT: Change from _openUpdateForm to _openInlineEdit to allow re-opening after cancel
            editBtn.addEventListener("click", () => this._openInlineEdit(index));
            actionCell.appendChild(editBtn);
            // Nested grid button
            const nestBtn = document.createElement("button");
            nestBtn.textContent = "Alt";
            nestBtn.className = "btn btn-sm btn-info";
            nestBtn.addEventListener("click", () => this.openNestedGrid(index));
            actionCell.appendChild(nestBtn);
        });
    }

    /**
     * Sayfalama arayüzünü (ileri/geri) günceller.
     * @private
     */
    _renderPager() {
        this.pagerElm.innerHTML = "";
        const totalPages = this.totalCount ? Math.ceil(this.totalCount / this.pageSize) : this.currentPage;
        // Page info
        const pageInfo = document.createElement("span");
        pageInfo.textContent = `Sayfa ${this.currentPage} / ${totalPages}`;
        this.pagerElm.appendChild(pageInfo);

        // Previous button
        const prevBtn = document.createElement("button");
        prevBtn.textContent = "Önceki";
        prevBtn.className = "btn btn-secondary ms-2"; // secondary button with left margin
        prevBtn.disabled = this.currentPage <= 1;
        prevBtn.addEventListener("click", () => {
            if (this.currentPage > 1) {
                this.currentPage--;
                this._fetchData();
            }
        });
        this.pagerElm.appendChild(prevBtn);

        // Next button
        const nextBtn = document.createElement("button");
        nextBtn.textContent = "Sonraki";
        nextBtn.className = "btn btn-secondary ms-2"; // secondary button with left margin
        nextBtn.disabled = this.totalCount
            ? (this.currentPage >= Math.ceil(this.totalCount / this.pageSize))
            : (this.data.length < this.pageSize);
        nextBtn.addEventListener("click", () => {
            if (this.totalCount) {
                if (this.currentPage < Math.ceil(this.totalCount / this.pageSize)) {
                    this.currentPage++;
                    this._fetchData();
                }
            } else {
                if (this.data.length === this.pageSize) {
                    this.currentPage++;
                    this._fetchData();
                }
            }
        });
        this.pagerElm.appendChild(nextBtn);
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
        if (this.editingIndex !== null && this.editingIndex !== index) {
            this._cancelInlineEdit(this.editingIndex);
        }
        this.editingIndex = index;
        const row = this.tableElm.querySelector(`tr[data-index='${index}']`);
        if (!row) return;
        const item = this.data[index];
        const cells = row.cells;
        const fieldCount = cells.length - 1;
        const fields = Object.keys(item);
        for (let i = 0; i < fieldCount; i++) {
            const fieldName = fields[i];
            if (fieldName === "id") {
                continue;
            }
            const cell = cells[i];
            const oldValue = item[fieldName] == null ? "" : item[fieldName];
            cell.innerHTML = "";
            let inputType = "text";
            const origType = typeof item[fieldName];
            if (origType === "number") inputType = "number";
            if (origType === "boolean") inputType = "checkbox";
            if (origType === "string" && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(oldValue)) {
                inputType = "date";
            }
            const input = document.createElement("input");
            input.name = fieldName;
            input.type = inputType;
            input.className = "form-control"; // Bootstrap input class
            if (inputType === "checkbox") {
                input.checked = Boolean(oldValue);
            } else if (inputType === "date") {
                input.value = oldValue ? String(oldValue).substring(0, 10) : "";
            } else {
                input.value = oldValue;
            }
            cell.appendChild(input);
        }
        const actionCell = cells[cells.length - 1];
        actionCell.innerHTML = "";
        const saveBtn = document.createElement("button");
        saveBtn.textContent = "Kaydet";
        saveBtn.className = "btn btn-sm btn-success me-1";
        saveBtn.addEventListener("click", () => this._saveInlineEdit(index));
        const cancelBtn = document.createElement("button");
        cancelBtn.textContent = "İptal";
        cancelBtn.className = "btn btn-sm btn-secondary";
        cancelBtn.addEventListener("click", () => this._cancelInlineEdit(index));
        actionCell.appendChild(saveBtn);
        actionCell.appendChild(cancelBtn);
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
        // Form içindeki tüm inputları al
        const inputs = row.querySelectorAll("input");
        const updatedData = { ...item }; // mevcut veriyi kopyala
        inputs.forEach(input => {
            const field = input.name;
            let value;
            if (input.type === "checkbox") {
                value = input.checked;
            } else if (input.type === "number") {
                value = input.value === "" ? null : Number(input.value);
            } else if (input.type === "date") {
                value = input.value; // tarih stringini (YYYY-MM-DD) al
            } else {
                value = input.value;
            }
            updatedData[field] = value;
        });
        // Güncelleme API çağrısı (PUT)
        const idField = Object.keys(item).find(k => k === "id") || "id";
        const idValue = item[idField];
        const url = `${this.apiPrefix.replace(/\/+$/, "")}/${this.controller}/${idValue}`;
        fetch(url, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(updatedData)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`Güncelleme hatası: ${response.status}`);
                }
                return response.json();
            })
            .then(updatedRecord => {
                // Sunucu güncellenmiş kaydı döndürdüyse, local data'yı güncelle
                this.data[index] = updatedRecord;
                this.editingIndex = null;
                // Satırı yeni verilerle yeniden oluştur
                this._renderUpdatedRow(index);
            })
            .catch(error => {
                console.error("Kayıt güncelleme hatası:", error);
                // Hata durumunda kullanıcıya bilgi verilebilir (alert, vb.)
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
        this._renderUpdatedRow(index);
    }

    /**
     * Belirli bir satırı (veri indeksine göre) this.data içeriğine göre yeniden oluşturur.
     * @param {number} index - Güncellenecek satırın indeksi.
     * @private
     */
    _renderUpdatedRow(index) {
        const row = this.tableElm.querySelector(`tr[data-index='${index}']`);
        if (!row) return;
        const item = this.data[index];
        const cells = row.cells;
        const fieldCount = cells.length - 1;
        const fields = Object.keys(item);
        // Veri hücrelerini güncelle
        for (let i = 0; i < fieldCount; i++) {
            const fieldName = fields[i];
            const cell = cells[i];
            cell.innerHTML = "";
            cell.textContent = item[fieldName] == null ? "" : item[fieldName];
        }
        // İşlem hücresini tekrar düzenle/detay butonları ile donat
        const actionCell = cells[cells.length - 1];
        actionCell.innerHTML = "";
        const editBtn = document.createElement("button");
        editBtn.textContent = "Düzenle";
        editBtn.className = "linq-edit-btn";
        editBtn.addEventListener("click", () => this._openInlineEdit(index));

        actionCell.appendChild(editBtn);
        const nestBtn = document.createElement("button");
        nestBtn.textContent = "Alt";
        nestBtn.className = "linq-nest-btn";
        nestBtn.addEventListener("click", () => this.openNestedGrid(index));
        actionCell.appendChild(nestBtn);
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
                    console.error("Özellik listesi alınamadı, form oluşturulamıyor.", error);
                });
        } else {
            this._showCreateForm();
        }
    }

    _showCreateForm() {
        // FormManager üzerinden formu oluşturuyoruz.
        // Container olarak dataTable'ın ana container'ını kullanabilirsiniz.
        FormManager.createForm(this.containerElm, this.entityProperties, {}, {
            mode: "Create",
            onSave: (newRecord) => {
                const url = `${this.apiPrefix.replace(/\/+$/, "")}/${this.controller}`;
                fetch(url, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(newRecord)
                })
                    .then(response => {
                        if (!response.ok) throw new Error(`Kayıt oluşturulamadı: ${response.status}`);
                        return response.json();
                    })
                    .then(createdRecord => {
                        // Kayıt başarıyla oluşturuldu, veriyi yeniden çek.
                        this._fetchData();
                    })
                    .catch(error => console.error("Yeni kayıt oluşturma hatası:", error));
            }
        });
    }

    _openUpdateForm(index) {
        const item = this.data[index];
        // FormManager ile update formunu açıyoruz; initialData olarak mevcut item'ı gönderiyoruz.
        FormManager.createForm(this.containerElm, this.entityProperties, item, {
            mode: "Update",
            onSave: (updatedItem) => {
                const idField = Object.keys(item).find(k => "id") || "id";
                const idValue = item[idField];
                const url = `${this.apiPrefix.replace(/\/+$/, "")}/${this.controller}/${idValue}`;
                fetch(url, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(updatedItem)
                })
                    .then(response => {
                        if (!response.ok) throw new Error(`Güncelleme hatası: ${response.status}`);
                        return response.json();
                    })
                    .then(updatedRecord => {
                        // Güncellenen kaydı verilerimize yansıtıp, tabloyu yenileyelim.
                        this.data[index] = updatedRecord;
                        this._fetchData();
                    })
                    .catch(error => console.error("Kayıt güncelleme hatası:", error));
            }
        });
    }

    /**
     * Seçilen satırın altında yeni bir alt grid (nested grid) açar veya zaten açıksa kapatır.
     //* @param {number} index - Alt grid açılacak üst satır indeksi.
     //* @param {string} [nestedController] - Alt grid için kullanılacak farklı bir controller adı. Verilmezse üst grid ile aynı kullanılır.
     //* @param {string} [foreignKeyField] - Alt grid filtresi için üst kaydın anahtar alanı (örn: "ParentId"). Verilmezse üst kaydın ID'sine göre otomatik türetilir.
     */
    //openNestedGrid(index, nestedController = null, foreignKeyField = null) {
    //    const parentRow = this.tableElm.querySelector(`tr[data-index='${index}']`);
    //    if (!parentRow) return;
    //    // Eğer zaten bu satırın altında bir alt grid açıksa, onu kapat (satırı kaldır)
    //    const nextRow = parentRow.nextElementSibling;
    //    if (nextRow && nextRow.classList.contains("linq-nested-row")) {
    //        this.tableElm.deleteRow(nextRow.rowIndex);
    //        return;
    //    }
    //    // Alt grid açık değilse, yeni bir satır ekleyerek alt grid konteynerini oluştur
    //    const colCount = parentRow.cells.length;
    //    const nestedRow = this.tableElm.insertRow(parentRow.rowIndex + 1);
    //    nestedRow.className = "linq-nested-row";
    //    const nestedCell = nestedRow.insertCell();
    //    nestedCell.colSpan = colCount;
    //    // Alt grid içeriği için bir div oluştur
    //    const nestedContainer = document.createElement("div");
    //    nestedContainer.className = "linq-nested-container";
    //    nestedCell.appendChild(nestedContainer);
    //    // Alt grid için kullanılacak controller belirle
    //    const childController = nestedController || this.controller;
    //    // Alt grid için ayrı bir durum yöneticisi oluştur (üst grid'inkinden bağımsız olmalı)
    //    const childState = new StateManager();
    //    // Eğer foreignKeyField belirtilmemişse, controller adına göre tahmin et
    //    if (!foreignKeyField) {
    //        // Örneğin üst controller "Customers" ise foreignKey "CustomerId" olabilir
    //        const parentIdField = Object.keys(this.data[index]).find(k => k.toLowerCase() === "id");
    //        let controllerName = this.controller;
    //        if (controllerName.endsWith("s")) {
    //            controllerName = controllerName.slice(0, -1);
    //        }
    //        foreignKeyField = controllerName + "Id";
    //        if (parentIdField && this.data[index][parentIdField] != null) {
    //            // Alt grid state'ine foreign key koşulunu başlangıçta ekle
    //            childState.setState("query", { [foreignKeyField]: this.data[index][parentIdField] });
    //        } else {
    //            console.warn("Üst kaydın ID değeri bulunamadı veya tanımsız, alt grid filtresi uygulanamadı.");
    //        }
    //    } else {
    //        // foreignKey alan adı verilmişse, üst veriden ID değerini alarak alt grid query'sine koy
    //        const parentIdField = Object.keys(this.data[index]).find(k => k.toLowerCase() === "id");
    //        if (parentIdField) {
    //            childState.setState("query", { [foreignKeyField]: this.data[index][parentIdField] });
    //        }
    //    }
    //    // Yeni bir LinqDataTable örneği oluştur (alt grid)
    //    new LinqDataTable({
    //        container: nestedContainer,
    //        controller: childController,
    //        apiPrefix: this.apiPrefix,
    //        stateManager: childState,
    //        autoCreateQueryBuilder: false // alt grid'de filtre arayüzü otomatik oluşturulmaz (isterse harici eklenir)
    //    });
    //}
}