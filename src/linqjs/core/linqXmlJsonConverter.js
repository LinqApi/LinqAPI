export class LinqXmlJsonConverter {
    constructor({ attributePrefix = '_', stripWhitespace = true } = {}) {
        this.attributePrefix = attributePrefix;
        this.stripWhitespace = stripWhitespace;
    }

    // 1) Raw XML string'i DOM Document'e çevir
    _parseXml(xmlStr) {
        const parser = new DOMParser();
        const doc = parser.parseFromString(xmlStr, 'application/xml');
        // parse error handling
        if (doc.querySelector('parsererror')) {
            throw new Error('Invalid XML');
        }
        return doc;
    }

    // 2) DOM tree'ini JS objesine dönüştür
    _nodeToObject(node) {
        // Text node
        if (node.nodeType === Node.TEXT_NODE) {
            const txt = node.textContent;
            return this.stripWhitespace ? txt.trim() : txt;
        }

        const obj = {};
        // Attributes
        if (node.attributes) {
            for (let { name, value } of node.attributes) {
                obj[this.attributePrefix + name] = value;
            }
        }
        // Children
        for (let child of node.childNodes) {
            const key = child.nodeName;
            const val = this._nodeToObject(child);
            if (val === '' || val == null) continue;
            if (obj[key] !== undefined) {
                // array form
                if (!Array.isArray(obj[key])) obj[key] = [obj[key]];
                obj[key].push(val);
            } else {
                obj[key] = val;
            }
        }
        return obj;
    }

    // Public: XML string → JS object
    xml2json(xmlStr) {
        const doc = this._parseXml(xmlStr);
        // kök elementi al
        const root = doc.documentElement;
        return { [root.nodeName]: this._nodeToObject(root) };
    }

    // Public: JS object → XML string (basit)
    json2xml(obj) {
        const build = (o, tag) => {
            if (typeof o !== 'object') {
                return `<${tag}>${o}</${tag}>`;
            }
            const attrs = Object.entries(o)
                .filter(([k]) => k.startsWith(this.attributePrefix))
                .map(([k, v]) => `${k.slice(this.attributePrefix.length)}="${v}"`)
                .join(' ');
            const children = Object.entries(o)
                .filter(([k]) => !k.startsWith(this.attributePrefix))
                .map(([k, v]) => {
                    if (Array.isArray(v)) {
                        return v.map(item => build(item, k)).join('');
                    }
                    return build(v, k);
                })
                .join('');
            return `<${tag}${attrs ? ' ' + attrs : ''}>${children}</${tag}>`;
        };

        const [rootTag] = Object.keys(obj);
        return build(obj[rootTag], rootTag);
    }
}

export default new LinqXmlJsonConverter();
