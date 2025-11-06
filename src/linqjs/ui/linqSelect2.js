// LinqSelect2.js - DÜZELTİLMİŞ NİHAİ SÜRÜM
import { debounce } from "./linqUtil.js"; // Bu dosyanın projenizde olduğundan emin olun

export class LinqSelect2 {
	constructor(cfg) {
		this.cfg = {
			container: null,
			fetchMode: "client",
			localData: [],
			multiselect: false,
			disabled: false,
			placeholder: "Seçim yapınız...",
			searchProperty: "name",
			displayProperty: "name",
			valueField: "id",
			debounceDuration: 300,
			renderMode: "dropdown", // 'dropdown' veya 'checkbox'
			selectedItems: [],
			onChange: () => { },
			...cfg,
		};
		// Gelen selectedItems'ın bir kopyasını alarak başla
		this.selectedItems = Array.isArray(this.cfg.selectedItems) ? [...this.cfg.selectedItems] : [];
		this.data = { items: [] };

		this.init();
	}

	initDropdownMode() {
		this.container.style.position = "relative";
		this.container.innerHTML = `
			<div class="select2-wrapper position-relative">
				<input type="text" class="form-control form-control-sm" placeholder="${this.cfg.placeholder}" />
				<div class="dropdown-menu w-100" style="position: absolute; top: 100%; left: 0; z-index: 1050;"></div>
			</div>
			<div class="selected-items-container mt-1"></div>`;

		this.input = this.container.querySelector("input");
		this.dropdown = this.container.querySelector(".dropdown-menu");
		this.selectedContainer = this.container.querySelector(".selected-items-container");

		this.updateSelectedUI();
		this.addEventListeners();
	}

	/**
	 * Bileşenin HTML yapısını oluşturur, elemanları class değişkenlerine atar
	 * ve olay dinleyicilerini bağlar.
	 */
	init() {
		this.container = this.cfg.container;
		if (!this.container) {
			console.error("LinqSelect2 için bir 'container' elemanı belirtilmedi.");
			return;
		}

		// DÜZELTME 1: Render moduna göre farklı HTML ve mantık çalıştır
		if (this.cfg.renderMode === 'checkbox') {
			this.initCheckboxMode();
		} else {
			this.initDropdownMode();
		}

		if (this.cfg.disabled) {
			this.disable();
		}
	}



	/**
	 * Gerekli tüm olay dinleyicilerini elemanlara ekler.
	 */
	addEventListeners() {
		this.input.addEventListener("input", debounce(() => {
			if (this.cfg.disabled) return;
			this.fetchData(this.input.value);
		}, this.cfg.debounceDuration));

		this.input.addEventListener("click", () => {
			if (this.cfg.disabled) return;
			const isVisible = this.dropdown.classList.contains("show");
			if (isVisible) {
				this.dropdown.classList.remove("show");
			} else {
				this.fetchData("");
			}
		});

		document.addEventListener("click", (e) => {
			if (this.container && !this.container.contains(e.target)) {
				this.dropdown.classList.remove("show");
			}
		});
	}

	disable() {
		this.cfg.disabled = true;
		if (this.input) {
			this.input.disabled = true;
			this.dropdown.classList.remove("show");
			this.input.placeholder = this.cfg.disabledPlaceholder || 'Pasif';
		} else {
			this.container.querySelectorAll('input[type="checkbox"]').forEach(cb => cb.disabled = true);
		}
	}
	enable() {
		this.cfg.disabled = false;
		if (this.input) {
			this.input.disabled = false;
			this.input.placeholder = this.cfg.placeholder;
		} else {
			this.container.querySelectorAll('input[type="checkbox"]').forEach(cb => cb.disabled = false);
		}
	}
	async fetchData(searchQuery) {
		if (this.cfg.disabled) return;
		if (this.cfg.fetchMode === "client") {
			const q = searchQuery.toLowerCase();
			this.data = {
				items: this.cfg.localData.filter(i => this.getDisplayText(i).toLowerCase().includes(q))
			};
			this.renderDropdown();
		} else {
			console.warn("Sunucu modu (server mode) henüz tam olarak implemente edilmedi.");
		}
	}
	renderDropdown() {
		const selectedIds = new Set(this.selectedItems.map(item => item[this.cfg.valueField]));
		const itemsHtml = this.data.items.map((item, index) => {
			const displayText = this.getDisplayText(item);
			const isSelected = selectedIds.has(item[this.cfg.valueField]);
			return `<a href="#" class="dropdown-item d-flex justify-content-between align-items-center ${isSelected ? 'active text-white' : ''}" data-index="${index}">${displayText}${isSelected ? '<i class="bi bi-check"></i>' : ''}</a>`;
		}).join("");
		this.dropdown.innerHTML = itemsHtml || `<span class="dropdown-item disabled">Sonuç bulunamadı</span>`;
		this.dropdown.querySelectorAll("a.dropdown-item:not(.disabled)").forEach(el => {
			el.addEventListener("click", (e) => {
				e.preventDefault();
				e.stopPropagation();
				const index = parseInt(el.getAttribute("data-index"), 10);
				this.selectItem(this.data.items[index]);
			});
		});
		this.dropdown.classList.add("show");
	}


	initCheckboxMode() {
		// Checkbox modu için arama input'una veya dropdown'a ihtiyacımız yok.
		// Sadece bir container yeterli.
		this.container.style.maxHeight = '200px'; // Örn: Çok uzarsa scroll çıksın
		this.container.style.overflowY = 'auto';
		this.renderCheckboxes();
	}

	renderCheckboxes() {
		const selectedIds = new Set(this.selectedItems.map(item => item[this.cfg.valueField]));

		const itemsHtml = this.cfg.localData.map((item, index) => {
			const displayText = this.getDisplayText(item);
			const value = item[this.cfg.valueField];
			const isSelected = selectedIds.has(value);
			const uniqueId = `linq-cb-${value}-${index}`;

			return `
				<div class="form-check">
					<input class="form-check-input" type="checkbox" value="${value}" id="${uniqueId}" data-index="${index}" ${isSelected ? 'checked' : ''}>
					<label class="form-check-label" for="${uniqueId}">
						${displayText}
					</label>
				</div>
			`;
		}).join("");

		this.container.innerHTML = itemsHtml;

		// Checkbox'lara olay dinleyicisi ekle
		this.container.querySelectorAll('.form-check-input').forEach(checkbox => {
			checkbox.addEventListener('change', (e) => {
				const index = parseInt(e.target.getAttribute('data-index'), 10);
				this.selectItem(this.cfg.localData[index]);
			});
		});
	}

	/**
	 * Bir öğe seçildiğinde veya seçimden kaldırıldığında çalışır.
	 * @param {object} item - Seçilen öğe.
	 */
	selectItem(item) {
		const value = item[this.cfg.valueField];
		const itemIndex = this.selectedItems.findIndex(i => i[this.cfg.valueField] === value);

		if (this.cfg.multiselect) {
			if (itemIndex > -1) {
				this.selectedItems.splice(itemIndex, 1);
			} else {
				this.selectedItems.push(item);
			}
		} else {
			this.selectedItems = [item];
			if (this.cfg.renderMode === 'dropdown') {
				this.dropdown.classList.remove("show");
			}
		}

		// DÜZELTME 2: Hangi modda olursak olalım UI'ı güncelle
		if (this.cfg.renderMode === 'checkbox') {
			// Checkbox modunda sadece onChange'i tetikle, UI zaten kendini güncelledi (checked/unchecked).
			this.cfg.onChange(this.getValue());
		} else {
			this.updateSelectedUI();
			this.renderDropdown();
		}
	}

	updateSelectedUI() {
		// Bu metot sadece dropdown modunda anlamlı
		if (this.cfg.renderMode === 'checkbox') return;

		if (!this.cfg.multiselect) {
			this.input.value = this.selectedItems.length > 0 ? this.getDisplayText(this.selectedItems[0]) : "";
			this.selectedContainer.innerHTML = "";
		} else {
			this.input.value = "";
			this.selectedContainer.innerHTML = this.selectedItems.map(item =>
				`<span class="badge bg-primary me-1 fw-normal">${this.getDisplayText(item)} <span class="remove-tag ms-1" data-id="${item[this.cfg.valueField]}" style="cursor: pointer;">&times;</span></span>`
			).join("");

			this.selectedContainer.querySelectorAll(".remove-tag").forEach(span => {
				span.addEventListener("click", (e) => {
					e.stopPropagation();
					this.removeSelected(span.dataset.id);
				});
			});
		}

		if (typeof this.cfg.onChange === 'function') {
			this.cfg.onChange(this.getValue());
		}
	}

	removeSelected(id) {
		this.selectedItems = this.selectedItems.filter(
			i => i[this.cfg.valueField].toString() !== id.toString()
		);

		if (this.cfg.renderMode === 'checkbox') {
			this.renderCheckboxes(); // Checkbox'ları yeniden çizerek doğru durumu yansıt
			this.cfg.onChange(this.getValue());
		} else {
			this.updateSelectedUI();
			this.renderDropdown();
		}
	}
	// --- DIŞARIDAN ERİŞİM İÇİN METOTLAR ---

	getValue() {
		return this.cfg.multiselect ? this.selectedItems : (this.selectedItems[0] || null);
	}


	getDisplayText(item) {
		if (!item) return "";
		if (Array.isArray(this.cfg.displayProperty)) {
			return this.cfg.displayProperty.map(prop => item[prop]).filter(Boolean).join(" - ");
		}
		return item[this.cfg.displayProperty] ?? "";
	}
}