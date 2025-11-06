// boot/relation-plugin.js
import { FieldRegistry, RelationFieldRenderer } from "../core/FieldRegistry.js";
import "../boot/fieldregistry-hub.js"; // hook zincirleyici (1 kez yüklenmeli)
// NOT: RelationGridPicker'ı burada import etmene gerek yok; renderer içinde import ediyorsan yeter.

const relation = new RelationFieldRenderer();

/* 1) renderEditor yönlendirmesi (sadece editor kısmını wrap’liyoruz) */
const _origRenderEditor = FieldRegistry.renderEditor.bind(FieldRegistry);
FieldRegistry.renderEditor = (value, propMeta, opts = {}) => {
    const kind = (propMeta?.kind || propMeta?.Kind || "").toLowerCase();
    if (kind === "complex" || kind === "complexlist") {
        return relation.renderEditor(value, propMeta, {
            ...opts,
            fieldName: opts.fieldName || propMeta.name,
            multiple: kind === "complexlist",
        });
    }
    return _origRenderEditor(value, propMeta, opts);
};

