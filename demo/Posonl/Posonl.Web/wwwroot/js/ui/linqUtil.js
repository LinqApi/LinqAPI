// linqutil.js

/**
 * FormManager handles the creation and update forms.
 * This class encapsulates the logic to build, show, and hide forms.
 */

import { Query, LogicalFilter, Pager } from "../core/models.js";
export class FormManager {
    /**
     * Creates a form for a given set of properties and initial data.
     * @param {HTMLElement} container - The container to place the form.
     * @param {Array} properties - The property metadata.
     * @param {object} initialData - Optional initial data.
     * @returns {HTMLElement} The form element.
     */
    static createForm(container, properties, initialData = {}, options = { mode: "Create" }) {
        const form = document.createElement("div");
        form.className = "card card-body mb-3";

        const inputs = {};
        const allowedTypes = ['string', 'int64', 'boolean', 'datetime', 'int32'];
        properties.forEach(prop => {
            // Skip collection types or Id.
            if (prop.type.includes("ICollection") || prop.name === "id") return;
            const typeLower = prop.type.toLowerCase();
            if (!allowedTypes.some(t => typeLower.includes(t))) return;

            const label = document.createElement("label");
            label.textContent = prop.name;
            const input = document.createElement("input");
            input.type = "text";
            input.className = "form-control mb-2";
            input.value = initialData[prop.name] || "";
            form.appendChild(label);
            form.appendChild(input);
            inputs[prop.name] = input;
        });

        const btnGroup = document.createElement("div");
        btnGroup.className = "d-flex justify-content-end gap-2";
        const saveBtn = document.createElement("button");
        saveBtn.className = "btn btn-success me-2";
        saveBtn.innerHTML = '<i class="bi bi-check-circle"></i>';
        // The save callback should be provided by the caller.
        saveBtn.addEventListener("click", () => {
            // Caller can extract values from inputs.
            const item = {};
            for (const key in inputs) {
                item[key] = inputs[key].value;
            }
            if (typeof options.onSave === "function") {
                options.onSave(item);
            }
            form.classList.remove("show");
            form.classList.add("collapse");
        });
        const cancelBtn = document.createElement("button");
        cancelBtn.className = "btn btn-danger";
        cancelBtn.innerHTML = '<i class="bi bi-x-circle"></i>';
        cancelBtn.addEventListener("click", () => {
            form.classList.remove("show");
            form.classList.add("collapse");
        });
        btnGroup.appendChild(saveBtn);
        btnGroup.appendChild(cancelBtn);
        form.appendChild(btnGroup);

        // Insert form in container.
        container.insertBefore(form, container.firstChild);
        return form;
    }
}

// linqUtil.js (Helpers & DataService)

/* Debounce Helper */
export const debounce = (func, delay = 300) => {
    let timer;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => func(...args), delay);
    };
};

export const defaults = {
    apiPrefix: "/api",
    propertiesRoute: "properties",
    filterPagedRoute: "filterpaged",
    defaultPageSize: 10,
    debounceDuration: 300,
};
/* Common Query Filter Builder */
export const buildFilter = ({ searchProperty, query, filterSuffix = "" }) => {
    const filter = query ? `${searchProperty}.Contains("${query}")` : "";
    return filterSuffix
        ? filter ? `${filterSuffix} AND ${filter}` : filterSuffix
        : filter || "1=1";
};

export const fetchPagedData = async (url, query) => {
    const res = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(query.toPayload()),
    });
    const json = await res.json();
    return {
        items: json.items || json.$values || [],
        totalCount: json.totalRecords || 0, // Use totalCount from response, or default to 0
    };
};


/**
 * Analyzes a property type string and returns an object with its kind and base type.
 * @param {string} typeStr - The property type string (e.g., "ICollection<PosService>" or "String").
 * @returns {Object} An object with 'kind' and 'baseType' properties.
 */
export const analyzePropertyType = (typeStr) => {
    // Regex to detect collection types (e.g., ICollection<PosService>, IList<PosService>, IReadOnlyCollection<PosService>)
    const complexListPattern = /^(ICollection|IList|IReadOnlyCollection)<\s*(.+)\s*>$/i;
    const match = typeStr.match(complexListPattern);
    if (match) {
        return {
            kind: "complexList",
            baseType: match[2].trim()
        };
    }
    // Define the allowed simple types for our application.
    const simpleTypes = ['string', 'int64', 'boolean', 'datetime', 'int32'];
    const lowerType = typeStr.toLowerCase();
    if (simpleTypes.some(t => lowerType.includes(t))) {
        return {
            kind: "simple",
            baseType: typeStr
        };
    }
    // If not a collection and not one of the known simple types, treat it as a complex type.
    return {
        kind: "complex",
        baseType: typeStr
    };
};

export const fetchProperties = async (controller, apiPrefix = "/api") => {
    const url = `${apiPrefix.replace(/\/+$/, "")}/${controller}/properties`;
    return fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error(`fetchProperties request failed: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            // Enhance each property with additional metadata
            const enhanced = data.map(prop => {
                const analysis = analyzePropertyType(prop.type);
                return {
                    ...prop,
                    kind: analysis.kind,
                    baseType: analysis.baseType,
                    displayProperties: prop.displayProperties || []  // New field
                };
            });
            // Filter out items with type "displayproperty"
            const normalProps = enhanced.filter(p =>
                typeof p.type === "string" && p.type.toLowerCase() !== "displayproperty"
            );
            // Optionally, if you need the display property metadata separately, you can store:
            // const displayProps = enhanced.filter(p => 
            //     typeof p.type === "string" && p.type.toLowerCase() === "displayproperty"
            // );
            return normalProps;
        })
        .catch(error => {
            console.error("fetchProperties error:", error);
            throw error;
        });
};
