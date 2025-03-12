(function (global) {
    class LinqTable {
        constructor(selector, options) {
            this.selector = selector;
            this.options = options || {};
            this.init();
        }

        init() {
            this.container = document.querySelector(this.selector);
            if (!this.container) {
                console.error("Container not found");
                return;
            }
            this.createUI();
        }

        createUI() {
            this.container.innerHTML = `
                <div class="linq-toolbar">
                    <input type="text" class="linq-search" placeholder="Search...">
                    <button class="linq-filter-btn">Filter</button>
                    <button class="linq-groupby-btn">Group By</button>
                    <button class="linq-select-btn">Select</button>
                </div>
                <div class="linq-grid-container">
                    <table class="linq-table">
                        <thead><tr></tr></thead>
                        <tbody></tbody>
                    </table>
                </div>
            `;
            this.attachEvents();
        }

        attachEvents() {
            this.container.querySelector(".linq-search").addEventListener("input", (e) => this.filterData(e.target.value));
            this.container.querySelector(".linq-filter-btn").addEventListener("click", () => this.showFilterPanel());
            this.container.querySelector(".linq-groupby-btn").addEventListener("click", () => this.groupByColumn());
            this.container.querySelector(".linq-select-btn").addEventListener("click", () => this.showSelectPanel());
        }

        filterData(query) {
            console.log("Filtering data with query: ", query);
            // Implement filtering logic
        }

        showFilterPanel() {
            console.log("Showing filter panel");
            // Implement filter UI
        }

        groupByColumn() {
            console.log("Grouping by column");
            // Implement group by logic
        }

        showSelectPanel() {
            console.log("Showing select panel");
            // Implement select UI
        }

        loadData(data) {
            this.data = data;
            this.renderTable();
        }

        renderTable() {
            let thead = this.container.querySelector(".linq-table thead tr");
            let tbody = this.container.querySelector(".linq-table tbody");
            thead.innerHTML = "";
            tbody.innerHTML = "";

            if (!this.data || this.data.length === 0) {
                tbody.innerHTML = `<tr><td colspan="10">No data available</td></tr>`;
                return;
            }

            let columns = Object.keys(this.data[0]);
            columns.forEach(col => {
                let th = document.createElement("th");
                th.innerText = col;
                th.dataset.column = col;
                th.addEventListener("click", () => this.sortBy(col));
                thead.appendChild(th);
            });

            this.data.forEach(row => {
                let tr = document.createElement("tr");
                columns.forEach(col => {
                    let td = document.createElement("td");
                    td.innerText = row[col];
                    tr.appendChild(td);
                });
                tbody.appendChild(tr);
            });
        }

        sortBy(column) {
            console.log("Sorting by column: ", column);
            // Implement sorting logic
        }
    }

    global.LinqTable = LinqTable;
})(window);
