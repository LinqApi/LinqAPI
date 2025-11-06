// core/FieldRegistry.js
import {
  TimeFieldRenderer,
  DateTimeFieldRenderer,
  DateFieldRenderer,
  LocationFieldRendererBasic,
  UploadFieldRenderer,
  EnumFieldRenderer,
  BooleanFieldRenderer,
  FallbackFieldRenderer
} from "./render/DefaultRenderers.js";

// basit escape
function esc(v) {
  return String(v ?? "")
    .replace(/&/g,"&amp;")
    .replace(/</g,"&lt;")
    .replace(/>/g,"&gt;")
    .replace(/"/g,"&quot;")
    .replace(/'/g,"&#039;");
}

function normalizeBaseType(raw) {
  let t = String(raw || "").toLowerCase().trim();
  const m = t.match(/^nullable<\s*([^>]+)\s*>$/);
  if (m) t = m[1].trim();
  t = t.replace(/^system\./, "");
  return t;
}

export class FieldRegistry {
  constructor() {
    this._customTypeMap = new Map();      // "location" -> rendererObj
    this._kindHandlers  = new Map();      // "complex"  -> rendererObj
    this._registeredRenderers = new Set();// afterTableRender/postProcess fan-out

    // default singletons (instance'a bağlı!)
    this.booleanRenderer       = new BooleanFieldRenderer();
    this.enumRenderer          = new EnumFieldRenderer();
    this.uploadRenderer        = new UploadFieldRenderer();
    this.fallbackRenderer      = new FallbackFieldRenderer();
    this.basicLocationRenderer = new LocationFieldRendererBasic();
    this.dateRenderer          = new DateFieldRenderer();
    this.dateTimeRenderer      = new DateTimeFieldRenderer();
    this.timeRenderer          = new TimeFieldRenderer();
  }

  // ---------- Public API (instance) ----------
  registerCustomType(typeName, rendererObj) {
    const key = String(typeName || "").toLowerCase();
    this._customTypeMap.set(key, rendererObj);
    this._registeredRenderers.add(rendererObj);
  }
  unregisterCustomType(typeName) {
    const key = String(typeName || "").toLowerCase();
    const r = this._customTypeMap.get(key);
    if (r) this._registeredRenderers.delete(r);
    this._customTypeMap.delete(key);
  }

  registerKindHandler(kindName, rendererObj) {
    const key = String(kindName || "").toLowerCase();
    this._kindHandlers.set(key, rendererObj);
    this._registeredRenderers.add(rendererObj);
  }
  unregisterKindHandler(kindName) {
    const key = String(kindName || "").toLowerCase();
    const r = this._kindHandlers.get(key);
    if (r) this._registeredRenderers.delete(r);
    this._kindHandlers.delete(key);
  }

  getRenderer(propMeta) { return this._pickRenderer(propMeta); }

  renderInline(value, propMeta, opts={}) {
    const r = this._pickRenderer(propMeta);
    return r.renderInline ? r.renderInline(value, propMeta, opts) : esc(value);
  }

  renderEditor(value, propMeta, opts={}) {
    const r = this._pickRenderer(propMeta);
    if (r.renderEditor) return r.renderEditor(value, propMeta, opts);
    const safeVal = value != null ? String(value) : "";
    return `
      <input type="text" class="form-control form-control-sm"
             name="${esc(opts.fieldName)}"
             data-field-type="text" value="${esc(safeVal)}">
    `;
  }

  postProcessDto(dtoRaw, props) {
    let dto = { ...dtoRaw };
    this._registeredRenderers.forEach(r => {
      if (typeof r.postProcessDto === "function") {
        dto = r.postProcessDto(dto, props);
      }
    });
    return dto;
  }

  shouldShowColumn(propMeta) {
    const r = this._pickRenderer(propMeta);
    if (typeof r.shouldShowColumn === "function") {
      return r.shouldShowColumn(propMeta);
    }
    const kind = (propMeta?.kind || "").toLowerCase();
    if (kind === "complex" || kind === "complexlist") return false; // default gizli
    return true;
  }

  afterTableRender(rootEl, controller) {
    this._registeredRenderers.forEach(r => {
      if (typeof r.afterTableRender === "function") {
        r.afterTableRender(rootEl, controller);
      }
    });
  }

  // ---------- Private ----------
  _pickRenderer(propMeta) {
    const kind = (propMeta?.kind || "").toLowerCase();
    const baseTypeLower = normalizeBaseType(propMeta?.baseType || propMeta?.type || "");

    // 1) custom type
    if (this._customTypeMap.has(baseTypeLower)) {
      return this._customTypeMap.get(baseTypeLower);
    }
    // 2) datetime family
    if (baseTypeLower === "datetime" || baseTypeLower === "datetimeoffset" || baseTypeLower === "timestamp") {
      return this.dateTimeRenderer;
    }
    if (baseTypeLower === "date") return this.dateRenderer;
    if (baseTypeLower === "time") return this.timeRenderer;

    // 3) enum kind
    if (kind === "enum") return this.enumRenderer;

    // 4) complex/complexlist kind handler
    if ((kind === "complex" || kind === "complexlist") && this._kindHandlers.has(kind)) {
      return this._kindHandlers.get(kind);
    }

    // 5) boolean
    if (baseTypeLower === "bool" || baseTypeLower === "boolean") return this.booleanRenderer;

    // 6) explicit enum via display.values
    if (propMeta?.display?.values && Array.isArray(propMeta.display.values)) {
      return this.enumRenderer;
    }

    // 7) heuristics
    if (baseTypeLower.includes("media") || baseTypeLower.includes("file") || baseTypeLower.includes("upload")) {
      return this.uploadRenderer;
    }
    if (baseTypeLower.includes("location") || baseTypeLower.includes("geo")) {
      return this.basicLocationRenderer;
    }

    // 8) fallback
    return this.fallbackRenderer;
  }
}

// Factory + default (geri uyum için)
export function createFieldRegistry() { return new FieldRegistry(); }
export const defaultRegistry = createFieldRegistry();
