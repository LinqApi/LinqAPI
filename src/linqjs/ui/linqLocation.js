// LinqLocation.js
// Tek dosya, bağımsız, yeniden kullanılabilir.
// Gereken tek dış bağımlılık: cfg.dataProvider (aşağıda interface'i var).
//
// dataProvider interface'i:
// {
//   async search(query: string): Promise<Array<{ placeId: string, label: string }>>,
//   async resolve(placeId: string): Promise<LocationModel>
// }
//
// LocationModel canonical shape:
// {
//   id: string|number|null,        // internal DB id varsa
//   providerId: string|null,       // external provider placeId / guid
//   label: string,                 // tek satır gösterilecek adres/metin
//   placeName?: string,            // varsa daha "insan" ismi
//   street?: string,
//   municipality?: string,
//   subRegion?: string,
//   postalCode?: string,
//   country?: string,
//   latitude: number,
//   longitude: number
// }
//
// Notlar:
// - Maplibre ve Amazon Location auth helper lazy-load edilir.
// - destroy() ile event listener'lar temizlenebilir.
// - readOnly / interactive modları destekleniyor.
// - status icon + input border feedback var.
// - map yükleme davranışı korunuyor.

export class LinqLocation {
    constructor(cfg = {}) {
        // --- CONFIG ---
        this.cfg = {
            container: null,               // required HTMLElement
            dataProvider: null,            // required provider (bak yukarı interface)
            debounceMs: 400,
            onSelect: null,                // callback(locModel)
            readOnly: false,
            initialLocation: null,         // LocationModel veya null

            // Harita / AWS location
            identityPoolId: null,
            region: 'eu-central-1',
            mapStyle: 'Standard',
            mapHeight: '350px',

            // Görsel opsiyonlar
            showDetails: true,
            detailFields: ['street', 'municipality', 'postalCode', 'subRegion', 'country'],
            showMapBtn: true,
            showDirectionsBtn: true,
            placeholderIcon: 'bi-geo-alt-fill',

            // Directions link üretimi
            getDirectionsUrl: loc =>
                `https://www.google.com/maps/search/?api=1&query=${loc.latitude},${loc.longitude}`,

            ...cfg
        };

        if (!this.cfg.container || !(this.cfg.container instanceof HTMLElement)) {
            throw new Error('LinqLocation: cfg.container bir HTMLElement olmalı.');
        }
        if (!this.cfg.dataProvider || typeof this.cfg.dataProvider.search !== 'function' || typeof this.cfg.dataProvider.resolve !== 'function') {
            throw new Error('LinqLocation: cfg.dataProvider geçerli değil (search/resolve metotları yok).');
        }

        // --- STATE ---
        this.map = null;
        this.currentLocation = this._normalizeLocation(this.cfg.initialLocation);
        this.instanceId = `ll-${Math.random().toString(36).substr(2, 9)}`;

        // event detach için referanslar
        this._boundDocClick = null;
        this._boundTabShown = null;

        // CSS sadece bir defa enjekte edilir
        this._injectCSS();

        // UI kur
        this._initUI();
    }

    // PUBLIC API ----------------------------------------------------------------

    /**
     * Widget'ı temizce yok etmek için çağır.
     * - document click listener vs kaldırır
     * - maplibre map destroy eder
     */
    destroy() {
        if (this._boundDocClick) {
            document.removeEventListener('click', this._boundDocClick);
            this._boundDocClick = null;
        }
        if (this._boundTabShown && this._mapTabButton) {
            this._mapTabButton.removeEventListener('shown.bs.tab', this._boundTabShown);
            this._boundTabShown = null;
        }
        if (this.map && this.map.remove) {
            this.map.remove();
            this.map = null;
        }
    }

    /**
     * Dışarıdan programatik olarak bir konum set etmek istersen:
     */
    setLocation(locModel) {
        const norm = this._normalizeLocation(locModel);
        this.currentLocation = norm;
        if (this.cfg.readOnly) {
            this._populateReadOnlyDetails(norm);
        } else {
            this._populateInteractiveDetails(norm);
            if (this.detailsWrapper) {
                this.detailsWrapper.style.display = 'block';
            }
        }
    }

    /**
     * Şu an seçilmiş konumu al.
     */
    getLocation() {
        return this.currentLocation || null;
    }

    // INTERNALS -----------------------------------------------------------------

    _injectCSS() {
        const styleId = 'linq-location-styles';
        if (document.getElementById(styleId)) {
            return;
        }

        const css = `
            .linq-location-widget .linq-location-map-container {
                width: 100%;
                border-radius: var(--bs-card-inner-border-radius, 0.375rem);
                border: 1px solid var(--bs-border-color, #dee2e6);
                overflow: hidden;
            }

            /* maplibre ctrl'lerinin boyutu biraz daha uyumlu olsun */
            .linq-location-widget .maplibregl-ctrl-group button {
                width: 30px !important;
                height: 30px !important;
            }
            
            /* dropdown suggestion listesi modal vs üstünde kalsın */
            .linq-location-widget .list-group {
                z-index: 1055;
            }

            /* input state color feedback */
            .linq-location-input.valid {
                border-color: var(--bs-success) !important;
                box-shadow: 0 0 0 .2rem rgba(25, 135, 84, .25);
            }
            .linq-location-input.invalid {
                border-color: var(--bs-warning) !important;
                box-shadow: 0 0 0 .2rem rgba(255, 193, 7, .25);
            }
        `;

        const style = document.createElement('style');
        style.id = styleId;
        style.type = 'text/css';
        style.innerHTML = css;
        document.head.appendChild(style);
    }

    _initUI() {
        this.cfg.container.innerHTML = '';

        if (this.cfg.readOnly) {
            this._initReadOnlyUI();
            if (this.currentLocation) {
                this._populateReadOnlyDetails(this.currentLocation);
            }
            return;
        }

        this._initInteractiveUI();
    }

    // READ ONLY MODE ------------------------------------------------------------

    _initReadOnlyUI() {
        const { placeholderIcon, showMapBtn, showDirectionsBtn, mapHeight } = this.cfg;

        const html = `
            <div class="card linq-location-widget">
                <div class="card-body">
                    <h5 class="card-title d-flex align-items-center gap-2">
                        <i class="bi ${placeholderIcon} text-primary"></i>
                        <span id="${this.instanceId}-label">Konum bilgisi yükleniyor...</span>
                    </h5>
                    ${this.cfg.showDetails
                        ? `<ul class="list-unstyled small text-muted mt-2 mb-3" id="${this.instanceId}-details"></ul>`
                        : ''
                    }
                    <div class="d-flex gap-2">
                        ${showMapBtn
                            ? `<button id="${this.instanceId}-mapbtn" class="btn btn-sm btn-outline-secondary">
                                   <i class="bi bi-map me-1"></i>Haritayı Göster
                               </button>`
                            : ''
                        }
                        ${showDirectionsBtn
                            ? `<button id="${this.instanceId}-dirbtn" class="btn btn-sm btn-outline-success" style="display:none;">
                                   <i class="bi bi-google me-1"></i>Yol Tarifi Al
                               </button>`
                            : ''
                        }
                    </div>
                </div>

                ${showMapBtn
                    ? `<div id="${this.instanceId}-map"
                           class="linq-location-map-container d-none"
                           style="height: ${mapHeight};"></div>`
                    : ''
                }
            </div>`;

        this.cfg.container.innerHTML = html;

        if (showMapBtn) {
            const mapBtn = this.cfg.container.querySelector(`#${this.instanceId}-mapbtn`);
            mapBtn.addEventListener('click', () => this._toggleMap());
        }
    }

    _populateReadOnlyDetails(loc) {
        // Label
        const labelEl = this.cfg.container.querySelector(`#${this.instanceId}-label`);
        if (labelEl) {
            labelEl.textContent = loc.placeName || loc.label || 'Adres';
        }

        // Details list
        if (this.cfg.showDetails) {
            const ul = this.cfg.container.querySelector(`#${this.instanceId}-details`);
            if (ul) {
                ul.innerHTML = this.cfg.detailFields
                    .map(f => loc[f] ? `<li><strong>${this._formatLabel(f)}:</strong> ${loc[f]}</li>` : null)
                    .filter(Boolean)
                    .join('');
            }
        }

        // Directions Button
        if (this.cfg.showDirectionsBtn) {
            const dirBtn = this.cfg.container.querySelector(`#${this.instanceId}-dirbtn`);
            if (dirBtn) {
                dirBtn.style.display = 'inline-block';
                dirBtn.onclick = () => window.open(this.cfg.getDirectionsUrl(loc), '_blank');
            }
        }
    }

    // INTERACTIVE MODE ----------------------------------------------------------

    _initInteractiveUI() {
        const html = `
            <div class="linq-location-widget position-relative">
                <div class="input-group">
                    <span class="input-group-text"><i class="bi bi-search"></i></span>
                    <input type="text"
                           id="${this.instanceId}-input"
                           class="form-control linq-location-input"
                           placeholder="Adres, yer veya mekan arayın...">
                    <span class="input-group-text bg-transparent border-start-0" id="${this.instanceId}-status"></span>
                </div>

                <ul id="${this.instanceId}-sug"
                    class="list-group position-absolute w-100 z-3"
                    style="display:none;"></ul>
                
                <div id="${this.instanceId}-details-wrapper"
                     class="mt-2"
                     style="display:none;">
                     <div class="card">
                        <div class="card-header p-2">
                            <ul class="nav nav-tabs card-header-tabs" role="tablist">
                                <li class="nav-item" role="presentation">
                                    <button class="nav-link active"
                                            data-bs-toggle="tab"
                                            data-bs-target="#${this.instanceId}-info-pane"
                                            type="button"
                                            role="tab">
                                        Bilgiler
                                    </button>
                                </li>
                                <li class="nav-item" role="presentation">
                                    <button class="nav-link"
                                            data-bs-toggle="tab"
                                            data-bs-target="#${this.instanceId}-map-pane"
                                            type="button"
                                            role="tab">
                                        Harita
                                    </button>
                                </li>
                            </ul>
                        </div>
                        <div class="card-body">
                            <div class="tab-content">
                                <div class="tab-pane fade show active"
                                     id="${this.instanceId}-info-pane"
                                     role="tabpanel"></div>
                                <div class="tab-pane fade"
                                     id="${this.instanceId}-map-pane"
                                     role="tabpanel">
                                    <div class="linq-location-map-container"
                                         style="height: ${this.cfg.mapHeight};"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>`;

        this.cfg.container.innerHTML = html;

        // cache dom refs
        this.input          = this.cfg.container.querySelector(`#${this.instanceId}-input`);
        this.sugList        = this.cfg.container.querySelector(`#${this.instanceId}-sug`);
        this.statusIcon     = this.cfg.container.querySelector(`#${this.instanceId}-status`);
        this.detailsWrapper = this.cfg.container.querySelector(`#${this.instanceId}-details-wrapper`);
        this.infoPane       = this.cfg.container.querySelector(`#${this.instanceId}-info-pane`);
        this.mapPane        = this.cfg.container.querySelector(`#${this.instanceId}-map-pane`);
        this._mapTabButton  = this.cfg.container.querySelector(`[data-bs-target="#${this.instanceId}-map-pane"]`);

        // event: input typing (debounced)
        this.input.addEventListener('input', this._debounce(async () => {
            await this._fetchSuggestions(this.input.value);
        }, this.cfg.debounceMs));

        // event: outside click -> suggestion kapanır
        this._boundDocClick = (e) => {
            if (!this.cfg.container.contains(e.target)) {
                this.sugList.style.display = 'none';
            }
        };
        document.addEventListener('click', this._boundDocClick);

        // event: map tab açıldığında haritayı renderla
        this._boundTabShown = async () => {
            const mapContainer = this.mapPane.querySelector('.linq-location-map-container');
            await this._renderMap(mapContainer);
        };
        if (this._mapTabButton) {
            this._mapTabButton.addEventListener('shown.bs.tab', this._boundTabShown);
        }

        // Eğer initialLocation varsa dolduralım
        if (this.currentLocation) {
            this.input.value = this.currentLocation.label || '';
            this._populateInteractiveDetails(this.currentLocation);
            this.detailsWrapper.style.display = 'block';
            this._setStatus('valid');
        }
    }

    async _fetchSuggestions(query) {
        const q = query.trim();
        if (!q) {
            this.sugList.style.display = 'none';
            return;
        }

        try {
            const items = await this.cfg.dataProvider.search(q);
            // items: [{ placeId, label }, ...]

            this.sugList.innerHTML = items.map(item =>
                `<li class="list-group-item list-group-item-action"
                     data-place-id="${this._escAttr(item.placeId)}"
                     data-label="${this._escAttr(item.label)}"
                     style="cursor:pointer;">
                    ${this._esc(item.label)}
                 </li>`
            ).join('');

            this.sugList.querySelectorAll('li').forEach(li => {
                li.addEventListener('click', (e) => this._selectSuggestion(e.currentTarget.dataset));
            });

            this.sugList.style.display = 'block';
        } catch (e) {
            console.error('[LinqLocation] Öneri alınırken hata:', e);
        }
    }

    async _selectSuggestion(dataset) {
        const { placeId, label } = dataset;
        this.input.value = label || '';
        this.sugList.style.display = 'none';
        this._setStatus('loading');

        try {
            // provider'dan full konum bilgisini çek
            const loc = await this.cfg.dataProvider.resolve(placeId);
            const norm = this._normalizeLocation(loc);

            // state güncelle
            this.currentLocation = norm;

            // önceki haritayı temizle
            if (this.map && this.map.remove) {
                this.map.remove();
                this.map = null;
            }

            // detay panelini doldur
            this._populateInteractiveDetails(norm);
            this.detailsWrapper.style.display = 'block';
            this._setStatus('valid');

            // dışarı bildir
            if (typeof this.cfg.onSelect === 'function') {
                this.cfg.onSelect(this.currentLocation);
            }

            // eğer harita tabı zaten aktifse hemen çiz
            if (this._mapTabButton &&
                this._mapTabButton.classList.contains('active')) {
                const mapContainer = this.mapPane.querySelector('.linq-location-map-container');
                await this._renderMap(mapContainer);
            }

        } catch (e) {
            console.error('[LinqLocation] Detay alınırken hata:', e);
            this._setStatus('invalid');
        }
    }

    _populateInteractiveDetails(loc) {
        // Adres alanları
        const detailsHtml = this.cfg.detailFields
            .map(f => loc[f] ? `<p class="mb-1"><small><strong>${this._formatLabel(f)}:</strong> ${this._esc(loc[f])}</small></p>` : null)
            .filter(Boolean)
            .join('');

        // Yol tarifi butonu
        const buttonsHtml = `
            <div class="mt-3">
                <a href="${this.cfg.getDirectionsUrl(loc)}"
                   target="_blank"
                   class="btn btn-sm btn-success">
                    <i class="bi bi-google me-1"></i> Yol Tarifi Al
                </a>
            </div>`;

        if (this.infoPane) {
            this.infoPane.innerHTML = detailsHtml + buttonsHtml;
        }
    }

    // MAP LOGIC -----------------------------------------------------------------

    async _toggleMap() {
        if (!this.currentLocation) return;

        const mapContainer = this.cfg.container.querySelector(`#${this.instanceId}-map`);
        const mapBtn = this.cfg.container.querySelector(`#${this.instanceId}-mapbtn`);
        const isHidden = mapContainer.classList.toggle('d-none');

        mapBtn.innerHTML = `<i class="bi bi-map me-1"></i>${isHidden ? 'Haritayı Göster' : 'Haritayı Gizle'}`;

        if (!isHidden && !this.map) {
            // ilk kez açıyoruz → yükle
            mapBtn.disabled = true;
            mapBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span>Yükleniyor...`;

            await this._renderMap(mapContainer);

            mapBtn.disabled = false;
            mapBtn.innerHTML = `<i class="bi bi-map me-1"></i>Haritayı Gizle`;
        }
    }

    async _renderMap(containerEl) {
        if (!this.currentLocation || !containerEl) return;

        try {
            await this._ensureMapLib();

            const loc = this.currentLocation;
            const auth = await window.amazonLocationAuthHelper.withIdentityPoolId(this.cfg.identityPoolId);

            // Eski map duruyorsa temizle (safety)
            if (this.map && this.map.remove) {
                this.map.remove();
                this.map = null;
            }

            this.map = new window.maplibregl.Map({
                container: containerEl,
                center: [loc.longitude, loc.latitude],
                zoom: 14,
                style: `https://maps.geo.${this.cfg.region}.amazonaws.com/v2/styles/${this.cfg.mapStyle}/descriptor`,
                ...auth.getMapAuthenticationOptions()
            });

            this.map.addControl(new window.maplibregl.NavigationControl(), 'top-left');
            new window.maplibregl.Marker().setLngLat([loc.longitude, loc.latitude]).addTo(this.map);

        } catch (e) {
            console.error('[LinqLocation] Harita oluşturulurken hata:', e);
            containerEl.innerHTML = '<div class="alert alert-danger small">Harita yüklenemedi.</div>';
        }
    }

    async _ensureMapLib() {
        // maplibre-gl ve amazonLocationAuthHelper globalde var mı?
        if (window.maplibregl && window.amazonLocationAuthHelper) {
            // CSS var mı kontrol et yine de
            this._ensureMapLibreCss();
            return;
        }

        await Promise.all([
            (!window.maplibregl) && new Promise(res => {
                const s = document.createElement('script');
                s.src = 'https://unpkg.com/maplibre-gl@3.x/dist/maplibre-gl.js';
                s.onload = res;
                document.head.appendChild(s);
            }),
            (!window.amazonLocationAuthHelper) && new Promise(res => {
                const s = document.createElement('script');
                s.src = 'https://unpkg.com/@aws/amazon-location-utilities-auth-helper@1.x/dist/amazonLocationAuthHelper.js';
                s.onload = res;
                document.head.appendChild(s);
            })
        ]);

        this._ensureMapLibreCss();
    }

    _ensureMapLibreCss() {
        if (!document.querySelector('link[href*="maplibre-gl.css"]')) {
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = 'https://unpkg.com/maplibre-gl@3.x/dist/maplibre-gl.css';
            document.head.appendChild(link);
        }
    }

    // HELPERS -------------------------------------------------------------------

    _setStatus(state) {
        if (!this.statusIcon || !this.input) return;

        const icons = {
            loading: '<span class="spinner-border spinner-border-sm"></span>',
            valid:   '<i class="bi bi-check-circle-fill text-success"></i>',
            invalid: '<i class="bi bi-geo-alt text-secondary"></i>'
        };

        this.statusIcon.innerHTML = icons[state] || icons.invalid;

        // input border feedback
        this.input.classList.remove('valid', 'invalid');

        if (state === 'valid') {
            this.input.classList.add('valid');
        } else if (state === 'invalid') {
            this.input.classList.add('invalid');
        }
    }

    _formatLabel(key) {
        const labels = {
            placeName: 'Yer Adı',
            street: 'Sokak',
            municipality: 'İlçe',
            postalCode: 'Posta Kodu',
            subRegion: 'İl',
            country: 'Ülke',
            longitude: 'Boylam',
            latitude: 'Enlem'
        };
        if (labels[key]) return labels[key];

        // fallback: camelCase -> "Camel Case"
        return key
            .replace(/([A-Z])/g, ' $1')
            .replace(/^./, str => str.toUpperCase());
    }

    _normalizeLocation(raw) {
        if (!raw || typeof raw !== 'object') return null;

        // Canonicalize field names
        const loc = {
            id: raw.id ?? raw.internalId ?? null,
            providerId: raw.providerId ?? raw.placeId ?? null,
            label: raw.label ?? raw.placeName ?? `${raw.latitude ?? "?"},${raw.longitude ?? "?"}`,
            placeName: raw.placeName ?? raw.label ?? null,
            street: raw.street ?? raw.addressLine1 ?? null,
            municipality: raw.municipality ?? raw.district ?? null,
            subRegion: raw.subRegion ?? raw.region ?? raw.state ?? null,
            postalCode: raw.postalCode ?? raw.zip ?? null,
            country: raw.country ?? raw.countryCode ?? null,
            latitude: Number(raw.latitude),
            longitude: Number(raw.longitude)
        };

        return loc;
    }

    _debounce(fn, delay) {
        let timer = null;
        return (...args) => {
            clearTimeout(timer);
            timer = setTimeout(() => fn.apply(this, args), delay);
        };
    }

    _esc(v) {
        return String(v ?? "")
            .replace(/&/g,"&amp;")
            .replace(/</g,"&lt;")
            .replace(/>/g,"&gt;")
            .replace(/"/g,"&quot;")
            .replace(/'/g,"&#039;");
    }

    _escAttr(v) {
        return this._esc(v);
    }
}
