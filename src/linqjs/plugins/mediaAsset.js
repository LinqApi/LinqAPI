// plugins/mediaAsset.js
import { LinqUpload } from "../ui/linqUpload.js";

function escAttr(v) {
  return String(v ?? "")
    .replace(/&/g,"&amp;")
    .replace(/</g,"&lt;")
    .replace(/>/g,"&gt;")
    .replace(/"/g,"&quot;")
    .replace(/'/g,"&#039;");
}

export function install(registry) {
  registry.registerCustomType("mediaasset", {
    // Render edilmiş alanları not etmek istersek dursun (ama artık şart değil)
    _seenFields: new Set(),

    shouldShowColumn() { return true; },

    // ---- helpers ----
    _cdnBase() {
      const raw = window.APP_CONFIG?.cdnBaseUrl || "";
      return String(raw).replace(/\/$/, "");
    },
    _resolveUrlFromPart(obj) {
      if (!obj) return "";
      if (obj.url) return String(obj.url);
      if (obj.Url) return String(obj.Url);
      const key = obj.key || obj.Key;
      if (key) {
        const base = this._cdnBase();
        const norm = String(key).replace(/^\//, "");
        return base ? `${base}/${norm}` : `/${norm}`;
      }
      return "";
    },
    _pickPart(asset, pref = ["thumbnail","small","medium","large"]) {
      if (!asset || typeof asset !== "object") return null;
      for (const k of pref) if (asset[k]) return { part: asset[k], name: k };
      return null;
    },

    // ---- inline ----
    renderInline(value) {
      if (!value || typeof value !== "object") {
        return `<span class="text-muted">—</span>`;
      }
      const idFull  = String(value.id ?? value.Id ?? "");
      const idShort = idFull ? idFull.slice(0,8) : "?";
      const status  = (typeof value.status === "number" ? value.status : value.Status);
      const statusTx =
        status === 100 ? "ReadyToUse" :
        status === 60  ? "ResizeError" :
        status === 30  ? "AiRejected"  :
        (status != null ? String(status) : "-");

      const prev = this._pickPart(value, ["thumbnail","small","medium","large"]);
      const big  = this._pickPart(value, ["large","medium","small","thumbnail"]);

      let prevUrl = prev ? this._resolveUrlFromPart(prev.part) : "";
      let bigUrl  = big  ? this._resolveUrlFromPart(big.part)  : "";

      if (!prevUrl) {
        const rootUrl = this._resolveUrlFromPart(value);
        if (rootUrl) {
          if (!bigUrl) bigUrl = rootUrl;
          prevUrl = rootUrl;
        }
      }

      if (prevUrl) {
        if (!bigUrl) bigUrl = prevUrl;
        return `
          <div class="d-inline-flex align-items-center gap-2">
            <a href="${escAttr(bigUrl)}" target="_blank" class="d-inline-block" title="${escAttr(idFull)}">
              <img src="${escAttr(prevUrl)}" alt="media" loading="lazy"
                   style="width:40px;height:40px;object-fit:cover;border-radius:6px;border:1px solid rgba(0,0,0,0.08);">
            </a>
            <span class="badge bg-primary-subtle text-primary-emphasis">${escAttr(statusTx)}</span>
            <code class="small text-muted">${escAttr(idShort)}</code>
          </div>
        `;
      }

      return `<span class="badge bg-light text-dark">Media ${escAttr(idShort)} (${escAttr(statusTx)})</span>`;
    },

    // ---- editor ----
    renderEditor(value, propMeta, opts = {}) {
      const fieldName = opts.fieldName;           // base (örn "profilePhotoMedia")
      const fkName    = `${fieldName}Id`;         // "profilePhotoMediaId"
      const hostId    = `upload-host-${Math.random().toString(36).slice(2)}`;

      if (fieldName) this._seenFields.add(fieldName);

      const existingAssetId =
        (value && (value.id || value.Id)) ? (value.id || value.Id) : "";

      return `
        <div class="linq-upload-editblock" data-linq-upload-block="${escAttr(hostId)}">
          <div id="${escAttr(hostId)}"
               data-upload-widget
               class="bg-white"
               data-initial='${escAttr(JSON.stringify(value || {}))}'
               data-fk-input="${escAttr(fkName)}">
          </div>

          <input type="hidden"
                 name="${escAttr(fkName)}"
                 data-field-type="fk-hidden-media"
                 value="${escAttr(existingAssetId)}">
        </div>
      `;
    },

    // ---- mount ----
    afterTableRender(rootEl) {
      const blocks = rootEl.querySelectorAll("[data-linq-upload-block]");
      blocks.forEach(block => {
        const host = block.querySelector("[data-upload-widget]");
        if (!host || host.__linqUploadMounted) return;

        // parse initial
        let initialAsset = null;
        try { initialAsset = JSON.parse(host.getAttribute("data-initial") || "{}"); }
        catch { initialAsset = null; }

        const fkName = host.getAttribute("data-fk-input");
        const hiddenFkInput = block.querySelector(
          `input[name="${fkName}"][data-field-type="fk-hidden-media"]`
        );

        // fallback’dan gelen muhtemel duplicate FK input’ları temizle
        const dup = block.querySelectorAll(
          `input[name="${fkName}"]:not([data-field-type="fk-hidden-media"])`
        );
        dup.forEach(x => x.remove());

        const cdnBaseRaw = window.APP_CONFIG?.cdnBaseUrl || "";
        const cdnBaseUrl = String(cdnBaseRaw).replace(/\/$/, "");
        const resolveCdnUrl = (obj) => {
          if (!obj) return "";
          if (obj.url) return obj.url;
          if (obj.key) {
            const key = String(obj.key).replace(/^\//,"");
            return cdnBaseUrl ? `${cdnBaseUrl}/${key}` : `/${key}`;
          }
          return "";
        };

        const upApi = window.APP_CONFIG?.uploadApi || {};
        const presignUrl = upApi?.presign;
        const statusUrl  = upApi?.status;
        const ownerByContentUrl = upApi?.ownerByContent;

        const dataProvider = {
          async getPresign(file) {
            if (!presignUrl) throw new Error("uploadApi.presign missing");
            const res = await fetch(
              `${presignUrl}?fileName=${encodeURIComponent(file.name)}`,
              { headers: { "Accept": "application/json" } }
            );
            if (!res.ok) throw new Error("presign failed");
            return res.json();
          },
          async pollStatus(contentIdOrAssetId) {
            if (!statusUrl) throw new Error("uploadApi.status missing");
            const max = 20, delay = 3000;
            for (let i=0;i<max;i++) {
              const res = await fetch(
                `${statusUrl}?contentId=${encodeURIComponent(contentIdOrAssetId)}`,
                { headers: { "Accept": "application/json" } }
              );
              if (!res.ok) throw new Error("status failed");
              const asset = await res.json();
              if (asset.status === 100 || asset.status === 60 || asset.status === 30) {
                ["thumbnail","small","medium","large"].forEach(k => {
                  if (asset[k]) asset[k].url = resolveCdnUrl(asset[k]);
                });
                return asset;
              }
              // eslint-disable-next-line no-await-in-loop
              await new Promise(r => setTimeout(r, delay));
            }
            return { id: null, status: -1 };
          },
          async resolveOwnerByContent(contentId) {
            if (!ownerByContentUrl) return { assetId: null };
            const res = await fetch(
              `${ownerByContentUrl}?contentId=${encodeURIComponent(contentId)}`,
              { headers: { "Accept": "application/json" } }
            );
            if (!res.ok) throw new Error("resolve owner failed");
            return res.json(); // { assetId: "<GUID>" }
          }
        };

        const widget = new LinqUpload({
          container: host,
          dataProvider,
          cdnBaseUrl,
          initialAsset,
          onSelectAsset: async (assetModel) => {
            if (!hiddenFkInput) return;

            // Öncelik: doğrudan asset id
            if (assetModel && (assetModel.id || assetModel.Id)) {
              hiddenFkInput.value = String(assetModel.id || assetModel.Id);
              return;
            }
            // İçerikten sahip çöz
            if (assetModel && (assetModel.contentId || assetModel.ContentId)) {
              try {
                const ow = await dataProvider.resolveOwnerByContent(assetModel.contentId || assetModel.ContentId);
                if (ow && ow.assetId) {
                  hiddenFkInput.value = String(ow.assetId);
                  return;
                }
              } catch { /* sessiz geç */ }
            }
            hiddenFkInput.value = "";
          }
        });

        host.__linqUploadMounted = widget;
      });
    },

    // ---- dto normalize ----
    postProcessDto(dtoRaw, props) {
      let dto = { ...dtoRaw };

      // MediaAsset complex alanlarını props’tan çıkarıp tek tek işle
      const mediaProps = (props || []).filter(p =>
        (p.kind === "complex" || p.kind === "complexList") &&
        String(p.baseType || p.type || "").toLowerCase() === "mediaasset"
      );

      mediaProps.forEach(p => {
        const base = p.name || p.Name;
        if (!base) return;
        const fk = `${base}Id`;

        const hasBaseObj = Object.prototype.hasOwnProperty.call(dto, base);
        const hasFk      = Object.prototype.hasOwnProperty.call(dto, fk);

        // 1) FK varsa normalize et
        if (hasFk) {
          if (dto[fk] === "") {
            dto[fk] = null;
          } else if (dto[fk] != null) {
            dto[fk] = String(dto[fk]);
          }
        }

        // 2) FK yok/boşsa, complex objeden türet
        const fkEmpty = !hasFk || dto[fk] == null || dto[fk] === "";
        if (fkEmpty && hasBaseObj) {
          const v = dto[base];
          if (v && typeof v === "object") {
            const cand =
              v.id ?? v.Id ??
              v.assetId ?? v.AssetId ??
              v.contentId ?? v.ContentId ?? null;
            if (cand != null && cand !== "") {
              dto[fk] = String(cand);
            }
          }
        }

        // 3) Complex objeyi DTO’dan temizle
        if (hasBaseObj) delete dto[base];
      });

      // Eski davranışla uyumlu kalsın diye temizleyelim (ama gerekmez)
      this._seenFields?.clear?.();

      return dto;
    }
  });
}

export function uninstall(registry) {
  registry.unregisterCustomType("mediaasset");
}
