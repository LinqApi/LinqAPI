// plugins/location.js
import { LinqLocation } from "../ui/linqLocation.js";

// küçük util
function esc(v) {
  return String(v ?? "")
    .replace(/&/g,"&amp;")
    .replace(/</g,"&lt;")
    .replace(/>/g,"&gt;")
    .replace(/"/g,"&quot;")
    .replace(/'/g,"&#039;");
}
const escAttr = esc;

/**
 * Instance-scoped kurulum.
 * @param {FieldRegistry} registry - tabloya özel registry instance'ı
 */
export function install(registry) {
  // IMPORTANT: baseType/type eşlemesi lower-case çalışır => "location"
  registry.registerCustomType("location", {

    shouldShowColumn(/*propMeta*/) {
      // complex default'ta gizli ama location'ı ana tabloda göstermek istiyorsan true dön
      return true;
    },

    renderInline(value /* location obj */) {
      if (!value || typeof value !== "object") {
        return `<span class="text-muted">—</span>`;
      }
      const label = value.placeName || value.label ||
                    `${value.latitude ?? "?"},${value.longitude ?? "?"}`;
      return `<span class="badge bg-light text-dark">${esc(label)}</span>`;
    },

    renderEditor(value, propMeta, opts = {}) {
      const fieldName = opts.fieldName;
      const row       = opts.fullRow || {};
      const fkName    = fieldName + "Id";
      const fkVal     = row[fkName] ?? "";

      const hostId = `loc-host-${Math.random().toString(36).slice(2)}`;

      return `
        <div class="linq-location-editblock" data-linq-location-block="${hostId}">
          <div id="${hostId}"
               class="border rounded p-2 bg-white"
               data-location-widget
               data-initial='${escAttr(JSON.stringify(value || {}))}'
               data-fk-input="${fkName}">
            <div class="text-muted small">Konum seç / ara…</div>
          </div>

          <input
            type="hidden"
            name="${fkName}"
            data-field-type="fk-hidden"
            value="${escAttr(fkVal)}"
          >
        </div>
      `;
    },

    afterTableRender(rootEl /* table or modal root */, controller) {
      const blocks = rootEl.querySelectorAll("[data-linq-location-block]");
      blocks.forEach(block => {
        const host = block.querySelector("[data-location-widget]");
        if (!host || host.__linqLocationMounted) return;

        // initial parse
        let initialVal = null;
        try { initialVal = JSON.parse(host.getAttribute("data-initial") || "{}"); }
        catch { initialVal = null; }

        const fkName = host.getAttribute("data-fk-input");
        const hiddenFkInput = block.querySelector(`input[name="${fkName}"]`);

        // data provider — dışarıdan konfig okur (window.APP_CONFIG.geoApiBaseUrl)
        const dataProvider = {
          async search(query) {
            const base = window.APP_CONFIG?.geoApiBaseUrl;
            if (!base) throw new Error("geoApiBaseUrl missing");
            const res = await fetch(`${base}/search?query=${encodeURIComponent(query)}`, {
              headers: { "Accept": "application/json" }
            });
            if (!res.ok) throw new Error("search failed");
            return res.json();
          },
          async resolve(placeId) {
            const base = window.APP_CONFIG?.geoApiBaseUrl;
            if (!base) throw new Error("geoApiBaseUrl missing");
            const res = await fetch(`${base}/resolve?placeId=${encodeURIComponent(placeId)}`, {
              headers: { "Accept": "application/json" }
            });
            if (!res.ok) throw new Error("resolve failed");
            return res.json();
          }
        };

        // widget mount
        const widget = new LinqLocation({
          container: host,
          dataProvider,
          initialLocation: initialVal,
          onSelect: (loc) => {
            if (!hiddenFkInput) return;
            // deterministik FK atama:
            if (loc && loc.id != null && loc.id !== "") {
              hiddenFkInput.value = String(loc.id);
            } else if (loc && loc.providerId) {
              hiddenFkInput.value = `ext:${loc.providerId}`;
            } else {
              hiddenFkInput.value = "";
            }
          }
        });

        host.__linqLocationMounted = widget;
      });
    },

    postProcessDto(dtoRaw, props) {
      let dto = { ...dtoRaw };

      const locProps = (props || []).filter(p =>
        p.kind === "complex" &&
        ((p.baseType || p.type || "").toLowerCase() === "location")
      );

      locProps.forEach(p => {
        const base = p.name;      // "location"
        const fk   = base + "Id"; // "locationId"

        if (fk in dto) {
          delete dto[base];

          if (dto[fk] === "") {
            dto[fk] = null;
          } else {
            const rawVal = dto[fk];
            if (/^ext:/.test(rawVal)) {
              // string olarak bırak — backend çözecek
            } else {
              const n = Number(rawVal);
              if (!Number.isNaN(n)) dto[fk] = n;
            }
          }
        }
      });

      return dto;
    }
  });
}

/** Opsiyonel: plugin'i sökmek istersen */
export function uninstall(registry) {
  registry.unregisterCustomType("location");
}
