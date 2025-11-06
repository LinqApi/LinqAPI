// eventigg-calendar.js

import { openDB } from 'https://unpkg.com/idb?module';
// LinqSelect2 import yolunuzun projenize uygun olduğundan emin olun
 import { LinqSelect2 } from '../index.js';

async function getCacheDB() {
    return openDB('EventiggCacheDB', 2, {
        upgrade(db) {
            if (!db.objectStoreNames.contains('events')) db.createObjectStore('events');
        }
    });
}

class EventiggCalendar extends HTMLElement {
    constructor() {
        super();
        this.viewType = 'week';
        this.currentDate = new Date();
        this.cacheTTL = 3600 * 1000; // 1 saat
        this.mappings = {};
        this.pageState = { day: {}, week: {}, month: {} };
        this.pageSize = 5;
        this.userLocation = null;
        this.geoMode = false;
        this.maxDistanceKm = 50;
        this.useCache = true; // Cache varsayılan olarak aktif
    }

    async connectedCallback() {
        try {
            this.mappings = JSON.parse(this.getAttribute('mappings') || '{}');
            this.useCache = this.getAttribute('use-cache') !== 'false';
        } catch (e) {
            console.error('Geçersiz "mappings" JSON verisi:', e);
            this.innerHTML = '<div class="alert alert-danger">Takvim başlatılamadı: Yapılandırma hatası.</div>';
            return;
        }

        if (this.useCache) {
            await this.indexDbCleanUp();
        }

        this.innerHTML = `
            <div class="d-flex flex-wrap mb-3 gap-2 align-items-center">
                <div class="btn-group btn-group-sm">
                    <button id="prevBtn" class="btn btn-outline-primary" title="Önceki"><i class="bi bi-chevron-left"></i></button>
                    <button id="nextBtn" class="btn btn-outline-primary" title="Sonraki"><i class="bi bi-chevron-right"></i></button>
                </div>
                <div class="btn-group btn-group-sm">
                    <button id="dayBtn" class="btn btn-outline-primary">Gün</button>
                    <button id="weekBtn" class="btn btn-outline-primary active">Hafta</button>
                    <button id="monthBtn" class="btn btn-outline-primary">Ay</button>
                </div>
                <span id="label" class="flex-grow-1 text-center fw-bold fs-5"></span>
                <button id="filterBtn" class="btn btn-sm btn-outline-secondary" type="button" data-bs-toggle="collapse" data-bs-target="#filterBar" aria-expanded="false" aria-controls="filterBar">
                    <i class="bi bi-sliders me-1"></i> Filtreler
                </button>
            </div>
            <div class="collapse" id="filterBar">
                <div class="p-3 mb-3 border rounded bg-light">
                    <div class="row g-2 align-items-center">
                        <div class="col-md-4"><div id="groupWrapper"></div></div>
                        <div class="col-md-4"><div id="subWrapper"></div></div>
                        <div class="col-md-4"><select id="selType" class="form-select form-select-sm"><option value="">Tüm Kaynaklar</option><option value="Biletix">Biletix</option><option value="BuGece">BuGece</option><option value="Eventigg">Eventigg</option></select></div>
                        <div class="col-12 d-flex align-items-center gap-3 flex-wrap mt-2">
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" role="switch" id="locSwitch">
                                <label class="form-check-label small" for="locSwitch">Konuma Göre Filtrele</label>
                            </div>
                            <div id="distanceControls" class="d-flex align-items-center gap-2" style="display: none;">
                                <label for="txtDist" class="form-label mb-0 small">Mesafe:</label>
                                <input id="txtDist" type="range" class="form-range" min="5" max="200" step="5" style="width: 150px;">
                                <span id="distanceDisplay" class="badge bg-secondary" style="min-width: 55px;"></span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div id="calendarContainer"></div>`;

        this._init();
        this._attachListeners();
        await this._render();
    }

    _init() {
        const qs = (selector) => this.querySelector(selector);
        this.prevBtn = qs('#prevBtn');
        this.nextBtn = qs('#nextBtn');
        this.dayBtn = qs('#dayBtn');
        this.weekBtn = qs('#weekBtn');
        this.monthBtn = qs('#monthBtn');
        this.labelEl = qs('#label');
        this.container = qs('#calendarContainer');
        this.selType = qs('#selType');
        this.locSwitch = qs('#locSwitch');
        this.distanceControls = qs('#distanceControls');
        this.distanceDisplay = qs('#distanceDisplay');
        this.txtDist = qs('#txtDist');

        this._setupSelect2(qs('#groupWrapper'), qs('#subWrapper'));
        this._updateDistanceUI();
    }

    _setupSelect2(groupContainer, subContainer) {
    if (typeof LinqSelect2 === 'undefined') {
        // ... hata durumu aynı kalacak ...
        return;
    }

    this.groupSelect = new LinqSelect2({
        container: groupContainer,
        multiselect: true,
        placeholder: 'Tüm Gruplar',
        localData: Object.entries(this.mappings.groupMap || {}).map(([id, name]) => ({ id: +id, name })),
        fetchMode: 'client',
        onChange: () => {
            this._updateSubOptions();
            this._render();
        }
    });

        this.subSelect = new LinqSelect2({
            container: subContainer,
            multiselect: true,
            placeholder: 'Önce Grup Seçiniz', // Kullanıcıyı yönlendirir
            localData: [],
            fetchMode: 'client',
            disabled: true, // Kural: Başlangıçta hiçbir grup seçili olmadığı için alt kategori pasif başlar.
            onChange: () => this._render()
        });
}

    _attachListeners() {
        this.prevBtn.addEventListener('click', () => this._nav(-1));
        this.nextBtn.addEventListener('click', () => this._nav(1));
        this.dayBtn.addEventListener('click', () => this._switchView('day'));
        this.weekBtn.addEventListener('click', () => this._switchView('week'));
        this.monthBtn.addEventListener('click', () => this._switchView('month'));
        this.selType.addEventListener('change', () => this._render());

        this.locSwitch.addEventListener('change', async () => {
            const isEnabled = this.locSwitch.checked;
            if (isEnabled && !this.userLocation) {
                await this._ensureLocation();
            } else {
                this.geoMode = isEnabled;
            }
            if (!this.userLocation) { this.geoMode = false; }
            this._updateDistanceUI();
            this._render();
        });

        this.txtDist.addEventListener('input', () => {
            this.maxDistanceKm = parseInt(this.txtDist.value, 10);
            this._updateDistanceUI();
        });
        this.txtDist.addEventListener('change', () => this._render());
    }

    // EventiggCalendar.js -> _updateSubOptions fonksiyonu

    _updateSubOptions() {
        if (!this.groupSelect || !this.subSelect) return;

        const selectedGroups = this.groupSelect.getValue()?.map(g => +g.id) || [];

        if (selectedGroups.length > 0) {
            // --- DURUM 2: BİR VEYA DAHA FAZLA GRUP SEÇİLDİ ---
            // Alt kategori dropdown'ını aktif et ve sadece ilgili alt kategorilerle doldur.
            const subs = new Map();
            selectedGroups.forEach(gid => {
                (this.mappings.categoryTree?.[gid] || []).forEach(subId => {
                    subs.set(subId, this.mappings.subMap?.[subId] || `ID: ${subId}`);
                });
            });
            const localData = Array.from(subs.entries()).map(([id, name]) => ({ id, name }));

            this.subSelect.cfg.localData = localData;
            this.subSelect.cfg.disabled = false; // Aktif et
            this.subSelect.input.disabled = false;
            this.subSelect.input.placeholder = 'Alt Kategori Seçiniz';
            this.subSelect.fetchData(''); // Listeyi doldur
        } else {
            // --- DURUM 1: HİÇBİR GRUP SEÇİLMEDİ ('Tüm Gruplar' durumu) ---
            // Alt kategori dropdown'ını pasif yap ve içini boşalt.
            this.subSelect.cfg.localData = [];
            this.subSelect.cfg.disabled = true; // Pasif et
            this.subSelect.input.disabled = true;
            this.subSelect.input.placeholder = 'Önce Grup Seçiniz';
            this.subSelect.selectedItems = []; // Temizle
            this.subSelect.updateSelectedUI();
            this.subSelect.fetchData('');
        }
    }

    // --- TEMEL TAKVİM FONKSİYONLARI ---

    async _nav(delta) {
        const today0 = new Date().setHours(0, 0, 0, 0);
        let d = new Date(this.currentDate);
        if (this.viewType === 'day') d.setDate(d.getDate() + delta);
        if (this.viewType === 'week') d.setDate(d.getDate() + 7 * delta);
        if (this.viewType === 'month') d.setMonth(d.getMonth() + delta);
        this.currentDate = new Date(Math.max(d.getTime(), today0));
        await this._render();
    }

    async _switchView(view) {
        if (this.viewType === view) return;
        this.viewType = view;
        [this.dayBtn, this.weekBtn, this.monthBtn].forEach(btn => btn.classList.remove('active'));
        this.querySelector(`#${view}Btn`).classList.add('active');
        this.pageState[view] = {};
        await this._render();
    }

    async _render() {
        this.container.innerHTML = '<div class="text-center p-5"><div class="spinner-border" role="status"><span class="visually-hidden">Yükleniyor...</span></div></div>';
        try {
            const range = this._getRange();
            await this._renderBlocks(range);
        } catch (error) {
            console.error("Render hatası:", error);
            this.container.innerHTML = '<div class="alert alert-danger">Etkinlikler gösterilirken bir hata oluştu.</div>';
        }
    }

    _getRange() {
        const cd = new Date(this.currentDate);
        let start, end, label;
        const locale = 'tr-TR';
        if (this.viewType === 'day') {
            start = end = new Date(cd);
            label = start.toLocaleDateString(locale, { day: 'numeric', month: 'long', year: 'numeric' });
        } else if (this.viewType === 'week') {
            const dayOfWeek = cd.getDay() === 0 ? 6 : cd.getDay() - 1;
            start = new Date(cd);
            start.setDate(cd.getDate() - dayOfWeek);
            end = new Date(start);
            end.setDate(start.getDate() + 6);
            label = `${start.toLocaleDateString(locale, { day: 'numeric', month: 'short' })} - ${end.toLocaleDateString(locale, { day: 'numeric', month: 'short', year: 'numeric' })}`;
        } else {
            start = new Date(cd.getFullYear(), cd.getMonth(), 1);
            end = new Date(cd.getFullYear(), cd.getMonth() + 1, 0);
            label = start.toLocaleDateString(locale, { month: 'long', year: 'numeric' });
        }
        this.labelEl.textContent = label;
        return {
            start: this._formatDate(start),
            end: this._formatDate(end)
        };
    }

    // --- VERİ YÜKLEME VE FİLTRELEME ---

    _buildFilterPayload({ start, end }, page, pageSize) {
        const base = {
            startDate: `${start}T00:00:00`,
            endDate: `${end}T23:59:59`,
            eventType: this.selType.value || null,
            categoryGroups: this.groupSelect?.getValue()?.map(g => +g.id) || null,
            categorySubs: this.subSelect?.getValue()?.map(s => +s.id) || null,
            page,
            pageSize
        };
        if (this.geoMode && this.userLocation) {
            return {
                ...base,
                latitude: this.userLocation.latitude,
                longitude: this.userLocation.longitude,
                maxDistanceKm: this.maxDistanceKm
            };
        }
        return base;
    }

    async _loadEvents({ start, end }, page, pageSize) {
        const payload = this._buildFilterPayload({ start, end }, page, pageSize);
        const keyParts = [
            `Evt`, start, end, `p${page}`, payload.eventType,
            payload.categoryGroups?.join(','), payload.categorySubs?.join(',')
        ];
        if (payload.latitude) {
            keyParts.push(`geo${payload.latitude.toFixed(4)}${payload.longitude.toFixed(4)}d${payload.maxDistanceKm}`);
        }
        const key = keyParts.filter(Boolean).join('_');

        if (this.useCache) {
            const db = await getCacheDB();
            const rec = await db.get('events', key);
            if (rec && Date.now() - rec.ts < this.cacheTTL) return rec.data;
        }

        const url = '/api/Cikboard/EventCalendar/geo-filter';
        try {
            const res = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            if (!res.ok) throw new Error(`API Hatası: ${res.status}`);
            const json = await res.json();
            const data = {
                items: Array.isArray(json.items) ? json.items : [],
                totalRecords: typeof json.totalRecords === 'number' ? json.totalRecords : (json.items?.length || 0)
            };
            if (this.useCache) {
                const db = await getCacheDB();
                await db.put('events', { ts: Date.now(), data }, key);
            }
            return data;
        } catch (error) {
            console.error("Etkinlikler yüklenirken hata:", error);
            return { items: [], totalRecords: 0 };
        }
    }

    // --- GÖRSELLEŞTİRME BLOKLARI ---

    async _renderBlocks({ start, end }) {
        const row = document.createElement('div');
        row.className = 'row g-3';
        this.container.innerHTML = '';
        this.container.append(row);

        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const dates = [];
        for (let d = new Date(start + 'T00:00:00'); d <= new Date(end + 'T23:59:59'); d.setDate(d.getDate() + 1)) {
            if (d < today) continue;
            dates.push(this._formatDate(d));
        }

        if (dates.length === 0) {
            this.container.innerHTML = '<div class="alert alert-info">Seçili aralıkta gösterilecek ileri tarihli bir gün bulunmuyor.</div>';
            return;
        }

        const promises = dates.map(date => this._loadEvents({ start: date, end: date }, 1, this.pageSize).then(data => ({ date, ...data })));
        const results = await Promise.all(promises);

        if (results.every(r => r.totalRecords === 0)) {
            this.container.innerHTML = '<div class="alert alert-secondary">Bu tarihlerde ve filtrelerde kayıtlı etkinlik bulunamadı.</div>';
            return;
        }

        results.forEach(({ date, items, totalRecords }) => {
            if (items.length === 0 && totalRecords === 0) return;

            const d = new Date(date + 'T00:00:00');
            const dayStr = `${d.getDate()} ${d.toLocaleDateString('tr-TR', { month: 'short' })}`;
            const dayOfWeek = d.toLocaleDateString('tr-TR', { weekday: 'short' });

            const col = document.createElement('div');
            col.className = 'col-sm-6 col-md-4 col-lg-3 d-flex';
            const card = document.createElement('div');
            card.className = 'card h-100 flex-fill shadow-sm';

            card.innerHTML = `
                <div class="card-header d-flex justify-content-between align-items-center small fw-bold">
                    <span title="${date}">${dayStr} <span class="fw-normal text-muted">${dayOfWeek}</span></span>
                    <span class="badge bg-primary rounded-pill">${totalRecords}</span>
                </div>`;

            const listGroup = document.createElement('div');
            listGroup.className = 'list-group list-group-flush p-1';
            items.forEach(ev => listGroup.append(this._createEventButton(ev)));

            const body = document.createElement('div');
            body.className = 'card-body p-0';
            body.append(listGroup);
            card.append(body);

            if (totalRecords > this.pageSize) {
                const footer = document.createElement('div');
                footer.className = 'card-footer p-1 text-center mt-auto';
                const moreBtn = document.createElement('button');
                moreBtn.className = 'btn btn-link btn-sm py-0';
                moreBtn.textContent = `+${totalRecords - this.pageSize} daha göster`;
                moreBtn.onclick = () => this._loadMore(date, listGroup, footer, totalRecords);
                footer.append(moreBtn);
                card.append(footer);
            }
            col.append(card);
            row.append(col);
        });
    }

    async _loadMore(date, listGroup, footer, totalRecords) {
        const ps = this.pageState[this.viewType] ||= {};
        ps[date] = (ps[date] || 1) + 1;

        const { items } = await this._loadEvents({ start: date, end: date }, ps[date], this.pageSize);
        items.forEach(ev => listGroup.append(this._createEventButton(ev)));

        const loadedCount = listGroup.children.length;
        const remaining = totalRecords - loadedCount;

        if (remaining > 0) {
            footer.querySelector('button').textContent = `+${remaining} daha göster`;
        } else {
            footer.remove();
        }
    }

    _createEventButton(ev) {
        const a = document.createElement('a');
        a.href = '#';
        a.className = 'list-group-item list-group-item-action small text-start border-0';
        a.textContent = ev.name;
        a.addEventListener('click', (e) => { e.preventDefault(); this.showDetail(ev); });
        return a;
    }

    // --- YARDIMCI VE MODAL FONKSİYONLARI ---

    _formatDate(d) {
        const date = new Date(d);
        const pad = n => String(n).padStart(2, '0');
        return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
    }

    _updateDistanceUI() {
        this.locSwitch.checked = this.geoMode;
        this.distanceControls.style.display = this.geoMode ? 'flex' : 'none';
        this.distanceDisplay.textContent = `${this.maxDistanceKm} km`;
        this.txtDist.value = this.maxDistanceKm;
    }

    async _ensureLocation() {
        if (!navigator.geolocation) {
            alert('Tarayıcınızda konum desteği bulunmuyor.');
            return;
        }
        return new Promise(resolve => {
            navigator.geolocation.getCurrentPosition(
                pos => {
                    this.userLocation = { latitude: pos.coords.latitude, longitude: pos.coords.longitude };
                    this.geoMode = true;
                    resolve();
                },
                () => {
                    alert('Konum izni verilmedi veya bir hata oluştu.');
                    this.geoMode = false;
                    this._updateDistanceUI();
                    resolve();
                }
            );
        });
    }

    async showDetail(ev) {
        // Bu fonksiyon, detay modal'ını oluşturur ve gösterir.
        // Karmaşıklığı önlemek için içeriği kısaltılmıştır, ancak mantık aynıdır.
        // Gerekirse tam içeriği önceki cevaplardan alınabilir.
        await this._ensureModal();
        const modalEl = document.getElementById('eventDetailModal');
        modalEl.querySelector('#detailTitle').textContent = ev.name;
        modalEl.querySelector('.modal-body').innerHTML = `
            <p><strong>Tarih:</strong> ${new Date(ev.startAt).toLocaleString('tr-TR')}</p>
            <p><strong>Mekan:</strong> ${ev.venue?.name || 'Belirtilmemiş'}</p>
            <hr>
            <div>${ev.description || 'Açıklama yok.'}</div>`;
        new bootstrap.Modal(modalEl).show();
    }

    _ensureModal() {
        if (document.getElementById('eventDetailModal')) return;
        document.body.insertAdjacentHTML('beforeend', `
            <div class="modal fade" id="eventDetailModal" tabindex="-1">
              <div class="modal-dialog modal-lg modal-dialog-scrollable">
                <div class="modal-content">
                  <div class="modal-header">
                    <h5 class="modal-title" id="detailTitle"></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                  </div>
                  <div class="modal-body">Yükleniyor...</div>
                </div>
              </div>
            </div>`);
    }

    async indexDbCleanUp(maxAge = 7 * 24 * 3600 * 1000) {
        try {
            const db = await getCacheDB();
            const tx = db.transaction('events', 'readwrite');
            let cursor = await tx.objectStore('events').openCursor();
            while (cursor) {
                if (cursor.value.ts && Date.now() - cursor.value.ts > maxAge) {
                    await cursor.delete();
                }
                cursor = await cursor.continue();
            }
        } catch (error) {
            console.error("Cache temizlenirken hata oluştu:", error);
        }
    }
}

customElements.define('eventigg-calendar', EventiggCalendar);