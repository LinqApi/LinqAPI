// LinqUpload.js (final sade versiyon)

export class LinqUpload {
    constructor(cfg = {}) {
        this.cfg = {
            container: null,          // required HTMLElement root
            dataProvider: null,       // required { getPresign(file), pollStatus(assetId) }
            cdnBaseUrl: "",           // optional string. if provided, we'll prefix keys with it
            allowImage: true,
            allowGif: false,
            allowWebp: false,
            allowVideo: false,
            multiple: false,
            initialAsset: null,       // optional MediaAsset
            onSelectAsset: null,      // optional cb(MediaAsset or null)
            ...cfg
        };

        if (!this.cfg.container || !(this.cfg.container instanceof HTMLElement)) {
            throw new Error("LinqUpload: cfg.container HTMLElement gerekli");
        }
        if (!this.cfg.dataProvider
            || typeof this.cfg.dataProvider.getPresign !== "function"
            || typeof this.cfg.dataProvider.pollStatus !== "function") {
            throw new Error("LinqUpload: cfg.dataProvider {getPresign,pollStatus} sağlamalı");
        }

        this.currentAsset = this._normalizeAsset(this.cfg.initialAsset);

        this._injectCSS();
        this._initUI();
    }

    destroy() { }

    setAsset(asset) {
        this.currentAsset = this._normalizeAsset(asset);
        this._renderPreviewSection();
        if (typeof this.cfg.onSelectAsset === "function") {
            this.cfg.onSelectAsset(this.currentAsset);
        }
    }

    getAsset() {
        return this.currentAsset;
    }

    // --- setup / ui ---

    _injectCSS() {
        const styleId = 'linq-upload-styles';
        if (document.getElementById(styleId)) return;

        const css = `
        .linqupload-widget {
            border: 1px solid var(--bs-border-color, #dee2e6);
            border-radius: .5rem;
            background-color: #fff;
        }

        .linqupload-dropzone {
            border: 2px dashed var(--bs-border-color, #ccc);
            border-radius: .5rem;
            cursor: pointer;
            transition: background-color .15s ease-in-out, border-color .15s ease-in-out;
        }
        .linqupload-dropzone:hover {
            background-color: rgba(0,0,0,0.03);
            border-color: var(--bs-primary, #0d6efd);
        }

        .linqupload-previewthumb img {
            max-width: 100%;
            border-radius: .375rem;
            border: 1px solid var(--bs-border-color, #dee2e6);
            object-fit: cover;
        }

        .linqupload-meta dt {
            font-size: .75rem;
            font-weight: 600;
            color: var(--bs-secondary-color, #6c757d);
        }
        .linqupload-meta dd {
            font-size: .75rem;
            margin-bottom: .25rem;
        }

        .linqupload-status-badge {
            font-size: .7rem;
        }
        `;
        const style = document.createElement('style');
        style.id = styleId;
        style.type = 'text/css';
        style.innerHTML = css;
        document.head.appendChild(style);
    }

    _initUI() {
        this.cfg.container.innerHTML = `
            <div class="linqupload-widget p-2 d-flex flex-column gap-2">
                <div class="d-flex flex-row flex-wrap gap-3 align-items-start">
                    <div class="linqupload-previewthumb" style="width:120px; flex:0 0 auto;">
                        <!-- thumbnail img -->
                    </div>

                    <div class="flex-grow-1 small">
                        <div class="linqupload-previewmeta">
                            <!-- meta/status/buttons -->
                        </div>
                    </div>
                </div>

                <div class="linqupload-uploadarea">
                    <div class="linqupload-dropzone text-center p-3" tabindex="0">
                        <div class="text-muted">
                            <i class="bi bi-cloud-arrow-up-fill fs-4 d-block mb-1"></i>
                            <div class="fw-semibold">Sürükle & Bırak / Tıkla Yükle</div>
                            <div class="small text-muted">
                                JPEG, PNG${this.cfg.allowGif ? ", GIF" : ""}${this.cfg.allowVideo ? ", Video" : ""}
                            </div>
                        </div>
                    </div>
                    <input type="file"
                           class="linqupload-input d-none"
                           ${this.cfg.multiple ? "multiple" : ""}
                           accept="${this._buildAcceptAttr()}">
                    <div class="linqupload-progresslist small mt-2"></div>
                </div>
            </div>
        `;

        this.thumbEl = this.cfg.container.querySelector('.linqupload-previewthumb');
        this.metaEl = this.cfg.container.querySelector('.linqupload-previewmeta');
        this.dropzoneEl = this.cfg.container.querySelector('.linqupload-dropzone');
        this.fileInputEl = this.cfg.container.querySelector('.linqupload-input');
        this.progressListEl = this.cfg.container.querySelector('.linqupload-progresslist');

        this._renderPreviewSection();

        // events
        this.dropzoneEl.addEventListener('click', () => this.fileInputEl.click());
        this.dropzoneEl.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ' ') this.fileInputEl.click();
        });
        this.dropzoneEl.addEventListener('dragover', e => e.preventDefault());
        this.dropzoneEl.addEventListener('drop', e => {
            e.preventDefault();
            this._handleFiles(e.dataTransfer.files);
        });
        this.fileInputEl.addEventListener('change', () => {
            this._handleFiles(this.fileInputEl.files);
        });
    }

    _renderPreviewSection() {
        const asset = this.currentAsset;

        if (!asset) {
            // Boş state
            this.thumbEl.innerHTML = `
                <div class="text-center text-muted border rounded w-100 h-100 d-flex align-items-center justify-content-center"
                     style="aspect-ratio:1/1; min-height:80px;">
                    <i class="bi bi-image fs-3 text-secondary"></i>
                </div>`;
            this.metaEl.innerHTML = `
                <div class="text-muted small">
                    Henüz bir medya yok.
                </div>`;
            return;
        }

        const {
            id,
            statusLabel,
            thumbnailUrl,
            smallUrl,
            mediumUrl,
            largeUrl
        } = this._buildAssetUrls(asset);

        this.thumbEl.innerHTML = `
            <a href="${largeUrl || mediumUrl || smallUrl || thumbnailUrl || '#'}"
               target="_blank"
               class="d-block"
               style="max-width:120px;">
                <img src="${thumbnailUrl || smallUrl || mediumUrl || largeUrl || ''}"
                     alt="thumbnail"
                     class="img-fluid">
            </a>
        `;

        this.metaEl.innerHTML = `
            <div class="d-flex flex-wrap align-items-center gap-2 mb-2">
                <code class="small text-break">${id || '-'}</code>
                <span class="badge bg-primary-subtle text-primary-emphasis linqupload-status-badge">
                    ${statusLabel}
                </span>
            </div>

            <div class="linqupload-meta">
                <dl class="row mb-2">
                    <dt class="col-4">Thumb</dt>
                    <dd class="col-8">${thumbnailUrl ? `<a href="${thumbnailUrl}" target="_blank">Görüntüle</a>` : '-'}</dd>

                    <dt class="col-4">Small</dt>
                    <dd class="col-8">${smallUrl ? `<a href="${smallUrl}" target="_blank">Görüntüle</a>` : '-'}</dd>

                    <dt class="col-4">Medium</dt>
                    <dd class="col-8">${mediumUrl ? `<a href="${mediumUrl}" target="_blank">Görüntüle</a>` : '-'}</dd>

                    <dt class="col-4">Large</dt>
                    <dd class="col-8">${largeUrl ? `<a href="${largeUrl}" target="_blank">Görüntüle</a>` : '-'}</dd>
                </dl>
            </div>
        `;
    }

    // --- upload flow ---

    _buildAcceptAttr() {
        const types = [];
        if (this.cfg.allowImage) {
            types.push('image/jpeg', 'image/png', 'image/bmp', 'image/tiff');
            if (this.cfg.allowWebp) types.push('image/webp');
        }
        if (this.cfg.allowGif) types.push('image/gif');
        if (this.cfg.allowVideo) types.push('video/*');
        return types.join(',');
    }

    _isAllowed(file) {
        const type = file.type;

        if (type.startsWith('video/')) {
            return this.cfg.allowVideo;
        }

        if (type.startsWith('image/')) {
            if (type === 'image/gif') return this.cfg.allowGif;
            if (type === 'image/webp') return this.cfg.allowWebp;
            return (
                ['image/jpeg', 'image/png', 'image/bmp', 'image/tiff'].includes(type)
                && this.cfg.allowImage
            );
        }

        return false;
    }

    _handleFiles(fileList) {
        const files = Array.from(fileList);
        files.forEach(file => {
            if (!this._isAllowed(file)) return;
            this._uploadFile(file);
        });
    }

    async _uploadFile(file) {
        const row = document.createElement('div');
        row.className = 'd-flex align-items-center gap-2 small text-muted mb-1';
        row.innerHTML = `
            <i class="bi bi-arrow-repeat animate-spin text-secondary"></i>
            <span>${this._esc(file.name)}</span>
            <span class="linqupload-uploadstatus">yükleniyor...</span>
        `;
        this.progressListEl.appendChild(row);

        try {
            // 1. presign bilgisi al (Bunu bizim widget bilmiyor, dataProvider biliyor)
            // getPresign(file) -> { uploadUrl, assetId, mimeType }
            const { uploadUrl, contentId, mimeType } =
                await this.cfg.dataProvider.getPresign(file);

            // 2. PUT presigned URL
            const putRes = await fetch(uploadUrl, {
                method: 'PUT',
                headers: { 'Content-Type': mimeType },
                body: file
            });
            if (!putRes.ok) throw new Error("Upload failed");

            // 3. pollStatus(assetId) -> MediaAsset
            const finalAsset = await this.cfg.dataProvider.pollStatus(contentId);

            // 4. set + callback
            this.setAsset(finalAsset);

            // UI feedback
            row.querySelector('.linqupload-uploadstatus').textContent = 'tamamlandı';
            const icon = row.querySelector('.bi-arrow-repeat');
            if (icon) {
                icon.classList.remove('animate-spin', 'text-secondary', 'bi-arrow-repeat');
                icon.classList.add('bi-check-circle-fill', 'text-success');
            }

        } catch (err) {
            console.error("LinqUpload upload error:", err);
            row.querySelector('.linqupload-uploadstatus').textContent = 'hata';
            const icon = row.querySelector('.bi-arrow-repeat');
            if (icon) {
                icon.classList.remove('animate-spin', 'text-secondary', 'bi-arrow-repeat');
                icon.classList.add('bi-x-circle-fill', 'text-danger');
            }
        }
    }

    // --- helpers ---

    _normalizeAsset(raw) {
        if (!raw || typeof raw !== "object") return null;
        return {
            id: raw.id ?? raw.Id ?? null,
            status: raw.status ?? raw.Status ?? 0,
            assetType: raw.assetType ?? raw.AssetType ?? null,
            thumbnail: raw.thumbnail ?? raw.Thumbnail ?? null,
            small: raw.small ?? raw.Small ?? null,
            medium: raw.medium ?? raw.Medium ?? null,
            large: raw.large ?? raw.Large ?? null
        };
    }

    _extractKeyOrUrl(s3content) {
        if (!s3content || typeof s3content !== "object") return { key: "", url: "" };

        return {
            key: s3content.key
                || s3content.objectKey
                || s3content.s3Key
                || s3content.path
                || "",
            url: s3content.url || ""
        };
    }

    _fullUrlFor(s3c) {
        const { key, url } = this._extractKeyOrUrl(s3c);

        // öncelik 1: cdnBaseUrl + key (senin dediğin gibi)
        if (key && this.cfg.cdnBaseUrl) {
            const base = this.cfg.cdnBaseUrl.replace(/\/+$/, '');
            const tail = key.replace(/^\/+/, '');
            return `${base}/${tail}`;
        }

        // öncelik 2: backend zaten tam url döndürmüş olabilir
        if (url) {
            return url;
        }

        // fallback
        return "";
    }

    _buildAssetUrls(asset) {
        const statusLabel = this._statusToText(asset.status);

        return {
            id: asset.id,
            statusLabel,
            thumbnailUrl: this._fullUrlFor(asset.thumbnail),
            smallUrl: this._fullUrlFor(asset.small),
            mediumUrl: this._fullUrlFor(asset.medium),
            largeUrl: this._fullUrlFor(asset.large)
        };
    }

    _statusToText(st) {
        const map = {
            0: "Unknown",
            10: "Uploaded",
            20: "AiChecked",
            30: "AiRejected",
            40: "AiApproved",
            50: "ResizeStarted",
            60: "ResizeError",
            100: "ReadyToUse"
        };
        return map[st] || `Status ${st}`;
    }

    _esc(v) {
        return String(v ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }
}
