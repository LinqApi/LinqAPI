// /js/linqMenu.js

import { LinqDataTable } from './linqDataTable.js'; // Veya LinqDataTable'ın bulunduğu doğru yol

export class LinqMenu {
    constructor(options) {
        this.options = {
            container: null,        // Menünün render edileceği container
            dataTableContainer: null, // DataTable'ın render edileceği container
            menuItems: [],          // Menü elemanları [{ title, entity, icon, items? }]
            dataTableConfig: {}     // Tüm DataTable'lara geçilecek ortak config
        };
        Object.assign(this.options, options);

        if (!this.options.container || !this.options.dataTableContainer) {
            throw new Error("LinqMenu: 'container' ve 'dataTableContainer' gereklidir.");
        }

        this.dtInstance = null;
        this.render();
        this.attachEventListeners();
    }

    render() {
        const generateItems = (items) => {
            let html = '';
            items.forEach(item => {
                if (item.isHeader) {
                    html += `<li class="list-group-item bg-secondary text-white mt-3">${item.title}</li>`;
                } else {
                    html += `
                        <li class="list-group-item p-2">
                            <a href="#" class="linq-api-link" data-entity="${item.entity}">
                                <i class="bi ${item.icon || 'bi-list'} me-2"></i>${item.title}
                            </a>
                        </li>`;
                }
            });
            return html;
        };

        this.options.container.innerHTML = `
            <ul class="list-group list-group-flush">${generateItems(this.options.menuItems)}</ul>
        `;
    }

    attachEventListeners() {
        this.options.container.addEventListener('click', e => {
            if (e.target && e.target.closest('.linq-api-link')) {
                e.preventDefault();
                const link = e.target.closest('.linq-api-link');
                const entity = link.dataset.entity;

                // Active state yönetimi
                this.options.container.querySelectorAll('.list-group-item.active').forEach(li => li.classList.remove('active'));
                link.closest('li').classList.add('active');

                this.loadDataTable(entity);
            }
        });
    }

    loadDataTable(entity) {
        this.options.dataTableContainer.innerHTML = '';

        const config = {
            ...this.options.dataTableConfig, // Ortak config'i al
            controller: entity,              // Tıklanan entity'yi ata
            container: this.options.dataTableContainer
        };

        this.dtInstance = new LinqDataTable(config);
    }

    loadFirstItem() {
        const firstLink = this.options.container.querySelector('.linq-api-link');
        if (firstLink) {
            firstLink.click();
        }
    }
}