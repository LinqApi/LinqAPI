// core/FieldRegistry.js
import { LinqSelect2 } from "../../ui/linqSelect2.js";
// Basit HTML escape util
function esc(s) { return String(s ?? "").replace(/[&<>"']/g, m => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[m])); }


// ----------------------------------------------------
// DEFAULT RENDERERLAR (fallback, boolean, enum, relation, upload, basic location-fallback)
// bunları zaten sende vardı; özetliyim kısaca minimal halleri.
// Burada önemli olan: bunlar "afterTableRender" vs implement ETMEK ZORUNDA DEĞİL.
// Sadece interface'e uyacak property'leri opsiyonel taşıyacaklar.
// ----------------------------------------------------

export class FallbackFieldRenderer {
    renderInline(value) {
        return esc(value);
    }
    renderEditor(value, propMeta, opts = {}) {
        const safeVal = value != null ? String(value) : "";
        const requiredAttr = propMeta.isRequired ? "required" : "";
        const maxAttr = propMeta.maxLength ? `maxlength="${propMeta.maxLength}"` : "";
        const placeholderAttr = opts.placeholderText
            ? `placeholder="${esc(opts.placeholderText)}"`
            : "";

        return `
            <input
                type="text"
                class="form-control form-control-sm"
                name="${esc(opts.fieldName)}"
                data-field-type="text"
                value="${esc(safeVal)}"
                ${placeholderAttr}
                ${requiredAttr}
                ${maxAttr}
            >
        `;
    }
}

export class BooleanFieldRenderer {
    renderInline(value) {
        return value
            ? `<span class="badge bg-success">Yes</span>`
            : `<span class="badge bg-secondary">No</span>`;
    }
    renderEditor(value, propMeta, opts = {}) {
        const checked = value === true ? "checked" : "";
        const labelText = opts.placeholderText || "";

        return `
            <div class="form-check form-switch">
                <input class="form-check-input"
                    type="checkbox"
                    name="${esc(opts.fieldName)}"
                    data-field-type="boolean"
                    ${checked}
                >
                <label class="form-check-label small">${esc(labelText)}</label>
            </div>
        `;
    }
}


export class EnumFieldRenderer {
    renderInline(value, propMeta) {
        if (!propMeta.display || !Array.isArray(propMeta.display.values)) {
            return esc(value);
        }
        const hit = propMeta.display.values.find(v => String(v.value) === String(value));
        const label = hit ? (hit.displayName || hit.name || hit.value) : value;
        return `<span class="badge bg-info-subtle text-dark">${esc(label)}</span>`;
    }

    renderEditor(value, propMeta, opts = {}) {
        const values = (propMeta.display && Array.isArray(propMeta.display.values))
            ? propMeta.display.values
            : [];

        const placeholderText = opts.placeholderText || "Select...";
        const multipleAttr = opts.multiple ? "multiple" : "";
        const requiredAttr = propMeta.isRequired ? "required" : "";

        // seçili değerleri işaretle
        const optionsHtml = values.map(v => {
            const vVal = v.value;
            const vText = v.displayName || v.name || String(vVal);
            const selected = Array.isArray(value)
                ? value.includes(vVal)
                : (String(value) === String(vVal));
            return `<option value="${esc(vVal)}" ${selected ? "selected" : ""}>${esc(vText)}</option>`;
        }).join("");

        // ilk option dummy placeholder (multiple olmadığı durumda mantıklı)
        const placeholderOption = opts.multiple
            ? ""
            : `<option value="" disabled ${value == null ? "selected" : ""}>
                   ${esc(placeholderText)}
               </option>`;

        return `
            <select
                name="${esc(opts.fieldName)}"
                class="form-select form-select-sm"
                data-field-type="enum"
                ${multipleAttr}
                ${requiredAttr}>
                ${placeholderOption}
                ${optionsHtml}
            </select>
        `;
    }
}


export class UploadFieldRenderer {
    renderInline(value) {
        if (!value) {
            return `<span class="text-muted small fst-italic">no file</span>`;
        }
        return `<code class="small">${esc(value.fileName || value.name || "file")}</code>`;
    }

    renderEditor(value, propMeta, opts = {}) {
        const requiredAttr = propMeta.isRequired ? "required" : "";
        const placeholderText = opts.placeholderText || "Choose file";

        return `
            <input
                type="file"
                class="form-control form-control-sm"
                data-field-type="upload"
                name="${esc(opts.fieldName)}"
                aria-label="${esc(placeholderText)}"
                ${requiredAttr}
            >
        `;
    }
}

export class LocationFieldRendererBasic {
    renderInline(value) {
        if (!value || typeof value !== "object") {
            return `<span class="text-muted">—</span>`;
        }
        const label = value.placeName || value.label ||
            `${value.latitude ?? "?"},${value.longitude ?? "?"}`;
        return `<span class="badge bg-light text-dark">${esc(label)}</span>`;
    }

    renderEditor(value, propMeta, opts = {}) {
        const valStr = (value && value.latitude != null && value.longitude != null)
            ? `${value.latitude},${value.longitude}`
            : "";

        const placeholderAttr = opts.placeholderText
            ? `placeholder="${esc(opts.placeholderText)}"`
            : `placeholder="lat,lng"`;

        return `
            <input
                type="text"
                class="form-control form-control-sm"
                data-field-type="location"
                name="${esc(opts.fieldName)}"
                ${placeholderAttr}
                value="${esc(valStr)}">
            <div class="form-text small text-muted">
                Konum seçildiğinde burası otomatik dolar (plugin ile gelişecek)
            </div>
        `;
    }
}

export class DateFieldRenderer {
    renderInline(value, propMeta) {
        // inline tabloda gösterim (list view vb)
        // value ISO "2025-10-29T12:34:00Z" gibi geliyorsa sadece YYYY-MM-DD alalım
        if (!value) return `<span class="text-muted">—</span>`;
        // normalize
        let d = new Date(value);
        if (isNaN(d.getTime())) {
            // belki zaten YYYY-MM-DD geliyor
            return `<span>${esc(String(value))}</span>`;
        }
        const yyyy = d.getFullYear();
        const mm = String(d.getMonth() + 1).padStart(2, "0");
        const dd = String(d.getDate()).padStart(2, "0");
        return `<span>${esc(`${yyyy}-${mm}-${dd}`)}</span>`;
    }

    renderEditor(value, propMeta, opts = {}) {
        // value -> Date veya ISO string olabilir, biz input[type=date] value'su için YYYY-MM-DD isteriz
        let dateVal = "";
        if (value) {
            const d = new Date(value);
            if (!isNaN(d.getTime())) {
                const yyyy = d.getFullYear();
                const mm = String(d.getMonth() + 1).padStart(2, "0");
                const dd = String(d.getDate()).padStart(2, "0");
                dateVal = `${yyyy}-${mm}-${dd}`;
            } else {
                // backend zaten YYYY-MM-DD veriyorsa onu direkt kullan
                dateVal = String(value);
            }
        }

        const requiredAttr = propMeta.isRequired ? "required" : "";
        const placeholderAttr = opts.placeholderText
            ? `placeholder="${esc(opts.placeholderText)}"`
            : "";

        return `
            <input
                type="date"
                class="form-control form-control-sm"
                name="${esc(opts.fieldName)}"
                data-field-type="date"
                value="${esc(dateVal)}"
                ${placeholderAttr}
                ${requiredAttr}
            >
        `;
    }
}

export class DateTimeFieldRenderer {
    renderInline(value, propMeta) {
        if (!value) return `<span class="text-muted">—</span>`;
        const d = new Date(value);
        if (isNaN(d.getTime())) {
            return `<span>${esc(String(value))}</span>`;
        }
        const yyyy = d.getFullYear();
        const mm = String(d.getMonth() + 1).padStart(2, "0");
        const dd = String(d.getDate()).padStart(2, "0");
        const HH = String(d.getHours()).padStart(2, "0");
        const MM = String(d.getMinutes()).padStart(2, "0");
        return `<span>${esc(`${yyyy}-${mm}-${dd} ${HH}:${MM}`)}</span>`;
    }

    renderEditor(value, propMeta, opts = {}) {
        // input[type=datetime-local] format: "YYYY-MM-DDTHH:MM"
        let dtVal = "";
        if (value) {
            const d = new Date(value);
            if (!isNaN(d.getTime())) {
                const yyyy = d.getFullYear();
                const mm = String(d.getMonth() + 1).padStart(2, "0");
                const dd = String(d.getDate()).padStart(2, "0");
                const HH = String(d.getHours()).padStart(2, "0");
                const MM = String(d.getMinutes()).padStart(2, "0");
                dtVal = `${yyyy}-${mm}-${dd}T${HH}:${MM}`;
            } else {
                // eğer backend zaten bu formatta tutuyorsa (örn "2025-10-29T12:45")
                dtVal = String(value);
            }
        }

        const requiredAttr = propMeta.isRequired ? "required" : "";
        const placeholderAttr = opts.placeholderText
            ? `placeholder="${esc(opts.placeholderText)}"`
            : "";

        return `
            <input
                type="datetime-local"
                class="form-control form-control-sm"
                name="${esc(opts.fieldName)}"
                data-field-type="datetime"
                value="${esc(dtVal)}"
                ${placeholderAttr}
                ${requiredAttr}
            >
        `;
    }
}
export class TimeFieldRenderer {
    renderInline(value) {
        if (!value) return `<span class="text-muted">—</span>`;
        // assume "HH:MM[:SS]" or Date
        if (typeof value === "string") {
            return `<span>${esc(value)}</span>`;
        }
        const d = new Date(value);
        if (isNaN(d.getTime())) return `<span>${esc(String(value))}</span>`;
        const HH = String(d.getHours()).padStart(2, "0");
        const MM = String(d.getMinutes()).padStart(2, "0");
        return `<span>${HH}:${MM}</span>`;
    }

    renderEditor(value, propMeta, opts = {}) {
        let tVal = "";
        if (value) {
            if (typeof value === "string") {
                tVal = value.slice(0, 5); // "HH:MM"
            } else {
                const d = new Date(value);
                if (!isNaN(d.getTime())) {
                    const HH = String(d.getHours()).padStart(2, "0");
                    const MM = String(d.getMinutes()).padStart(2, "0");
                    tVal = `${HH}:${MM}`;
                }
            }
        }

        const requiredAttr = propMeta.isRequired ? "required" : "";
        const placeholderAttr = opts.placeholderText
            ? `placeholder="${esc(opts.placeholderText)}"`
            : "";

        return `
            <input
                type="time"
                class="form-control form-control-sm"
                name="${esc(opts.fieldName)}"
                data-field-type="time"
                value="${esc(tVal)}"
                ${placeholderAttr}
                ${requiredAttr}
            >
        `;
    }
}