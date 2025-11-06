// linqUtil.js

import { LinqLocation } from "./linqLocation.js";

/**
 * FormManager handles the creation and update of forms dynamically based on property metadata.
 */
export class FormManager {
    /**
     * Creates a form and returns the element along with a function to initialize its widgets.
     * @param {HTMLElement} container - The parent container (can be null if appended later).
     * @param {Array} properties - The property metadata array from the backend.
     * @param {object} initialData - Optional initial data for updating forms.
     * @param {object} options - Configuration options: { 
     * mode: "Create"|"Update", 
     * onSave: function, 
     * apiPrefix?: string, 
     * controllerSuffix?: string,
     * LinqDataTableClass: class // Dependency Injection for LinqDataTable
     * }
     * @returns {{formElement: HTMLElement, initWidgets: function}}
     */
    static createForm(container, properties, initialData = {}, options = {}) {
        const { mode = "Create", LinqDataTableClass } = options;

        if (!LinqDataTableClass) {
            throw new Error("FormManager requires LinqDataTableClass to be provided in options to handle complex types.");
        }

        const form = document.createElement("form");
        form.className = "row g-3 card card-body mb-3";
        const widgetsToInit = [];

        properties.forEach(prop => {
            const baseTypeLower = (prop.baseType || prop.type || "").toLowerCase();
            const propNameLower = prop.name.toLowerCase();

            // Skip Primary Key fields and Foreign Key fields that have a corresponding complex object
            if (prop.isPrimaryKey) return;
            if (prop.kind === 'simple' && propNameLower.endsWith('id')) {
                const complexEquivalentName = propNameLower.slice(0, -2);
                const complexEquivalent = properties.find(p => p.name.toLowerCase() === complexEquivalentName);
                if (complexEquivalent && (complexEquivalent.kind === 'complex' || complexEquivalent.kind === 'complexList')) {
                    return; // Skip rendering 'venueId' if 'venue' (complex) exists.
                }
            }

            // --- WIDGET RENDERERS ---

            // COMPLEX & COMPLEXLIST
            if (prop.kind === "complex" || prop.kind === "complexList") {
                const isCollection = prop.kind === "complexList";
                const value = initialData[prop.name];
                const selectedIds = isCollection ? (value || []).map(item => item.id) : (value ? [value.id] : []);

                const wrapper = document.createElement("div");
                wrapper.className = "mb-3 col-12";
                const label = document.createElement("label");
                label.className = "form-label fw-bold";
                label.textContent = prop.display?.name || prop.name;
                const widgetContainer = document.createElement("div");
                widgetContainer.id = `form-widget-${prop.name}-${Date.now()}`;
                const hiddenInput = document.createElement("input");
                hiddenInput.type = "hidden";
                hiddenInput.name = prop.name;
                hiddenInput.value = JSON.stringify(selectedIds);
                wrapper.append(label, widgetContainer, hiddenInput);
                form.appendChild(wrapper);

                widgetsToInit.push({
                    containerId: widgetContainer.id, baseType: prop.baseType, isMulti: isCollection,
                    hiddenInputName: prop.name, initialSelection: selectedIds,
                    apiPrefix: options.apiPrefix, controllerSuffix: options.controllerSuffix || ""
                });
                return;
            }

            // ENUM
            if (prop.kind === "enum") {
                const wrapper = document.createElement("div");
                wrapper.className = "mb-3 col-md-6";
                const label = document.createElement("label");
                label.className = "form-label";
                label.htmlFor = prop.name;
                label.textContent = prop.display?.name || prop.name;
                const select = document.createElement("select");
                select.className = "form-select";
                select.name = prop.name;
                select.id = prop.name;
                (prop.display?.values || []).forEach(opt => {
                    const option = document.createElement("option");
                    option.value = opt.value;
                    option.textContent = opt.displayName || opt.name;
                    if (initialData[prop.name] !== undefined && initialData[prop.name] == opt.value) {
                        option.selected = true;
                    }
                    select.appendChild(option);
                });
                wrapper.append(label, select);
                form.appendChild(wrapper);
                return;
            }

            // BOOLEAN
            if (baseTypeLower === "boolean") {
                const wrapper = document.createElement("div");
                wrapper.className = "form-check mb-3 col-12 ms-1";
                const input = document.createElement("input");
                input.type = "checkbox";
                input.className = "form-check-input";
                input.name = prop.name;
                input.id = prop.name;
                if (initialData[prop.name]) input.checked = true;
                const label = document.createElement("label");
                label.className = "form-check-label";
                label.htmlFor = prop.name;
                label.textContent = prop.display?.name || prop.name;
                wrapper.append(input, label);
                form.appendChild(wrapper);
                return;
            }

            // OTHER SIMPLE TYPES (INPUT)
            if (prop.kind === 'simple') {
                const wrapper = document.createElement("div");
                wrapper.className = "mb-3 col-md-6";
                const label = document.createElement("label");
                label.className = "form-label";
                label.htmlFor = prop.name;
                label.textContent = prop.display?.name || prop.name;
                const input = document.createElement("input");
                input.className = "form-control";
                input.name = prop.name;
                input.id = prop.name;

                if (["int16", "int32", "int64", "decimal", "double", "float"].some(t => baseTypeLower.includes(t))) {
                    input.type = "number";
                    input.step = baseTypeLower.includes("int") ? "1" : "any";
                } else if (baseTypeLower.includes("datetime")) {
                    input.type = "datetime-local";
                } else if (baseTypeLower.includes("date")) {
                    input.type = "date";
                } else {
                    input.type = "text";
                }

                const val = initialData[prop.name];
                if (val != null) {
                    if (input.type === "datetime-local") input.value = new Date(new Date(val).getTime() - (new Date().getTimezoneOffset() * 60000)).toISOString().slice(0, 16);
                    else if (input.type === "date") input.value = new Date(val).toISOString().slice(0, 10);
                    else input.value = val;
                }

                wrapper.append(label, input);
                form.appendChild(wrapper);
            }
        });

        // --- BUTTONS & SAVE LOGIC ---
        const btnDiv = document.createElement("div");
        btnDiv.className = "col-12 d-flex justify-content-end gap-2 mt-3";
        const saveBtn = document.createElement("button");
        saveBtn.type = "button";
        saveBtn.className = "btn btn-success";
        saveBtn.textContent = mode === "Create" ? "Oluştur" : "Kaydet";
        saveBtn.addEventListener("click", () => {
            const item = {};
            properties.forEach(prop => {
                if (prop.isPrimaryKey) return;

                const el = form.querySelector(`[name="${prop.name}"]`);
                if (!el) return;

                if (prop.kind === "complex" || prop.kind === "complexList") {
                    const isCollection = prop.kind === "complexList";
                    try {
                        const selectedIds = JSON.parse(el.value || "[]");
                        if (isCollection) {
                            item[prop.name] = selectedIds.map(id => ({ id: id }));
                        } else {
                            item[prop.name] = selectedIds.length > 0 ? { id: selectedIds[0] } : null;
                        }
                    } catch (e) {
                        console.error(`Error parsing value for ${prop.name}:`, el.value);
                        item[prop.name] = isCollection ? [] : null;
                    }
                } else if ((prop.baseType || prop.type).toLowerCase() === "boolean") {
                    item[prop.name] = el.checked;
                } else {
                    item[prop.name] = el.value === "" ? null : el.value;
                }
            });
            options.onSave(item);
        });

        const cancelBtn = document.createElement("button");
        cancelBtn.type = "button";
        cancelBtn.className = "btn btn-secondary";
        cancelBtn.textContent = "İptal";
        cancelBtn.addEventListener("click", () => {
            // Formu içeren en yakın modal'ı bul ve kapat
            const modalElement = form.closest('.modal');
            if (modalElement) {
                const modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) modal.hide();
            }
        });

        btnDiv.append(saveBtn, cancelBtn);
        form.appendChild(btnDiv);

        return {
            formElement: form,
            initWidgets: () => {
                widgetsToInit.forEach(w => {
                    const widgetHost = document.getElementById(w.containerId);
                    const hiddenInput = form.querySelector(`input[name="${w.hiddenInputName}"]`);
                    if (!widgetHost || !hiddenInput) return;

                    new LinqDataTableClass({
                        container: `#${w.containerId}`,
                        controller: w.baseType,
                        apiPrefix: w.apiPrefix,
                        controllerSuffix: w.controllerSuffix,
                        selectionMode: w.isMulti ? 'multiple' : 'single',
                        initialSelection: w.initialSelection,
                        showColumnSelector: false,
                        allowCreate: false,
                        allowUpdate: false,
                        allowDelete: false,
                        pager: { pageNumber: 1, pageSize: 5 }, // Widget'lar için daha küçük bir sayfalama
                        onSelectionChange: (selectedIds) => {
                            hiddenInput.value = JSON.stringify(selectedIds);
                        }
                    });
                });
            }
        };
    }
}

// --- Diğer Yardımcı Fonksiyonlar ---

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

export const fetchPagedData = async (url, query) => {
    const res = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(query),
    });
    if (!res.ok) {
        console.error("Fetch paged data failed:", res.status, await res.text());
        return { items: [], totalCount: 0 };
    }
    const json = await res.json();
    return {
        items: json.items || json.$values || [],
        totalCount: json.totalRecords || json.totalCount || 0,
    };
};

export const fetchProperties = async (controller, apiPrefix = "/api") => {
    if (!controller) {
        console.warn("fetchProperties called with no controller. Returning empty array.");
        return [];
    }
    const url = `${apiPrefix.replace(/\/+$/, "")}/${controller}/${defaults.propertiesRoute}`;
    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error(`Could not fetch properties for controller '${controller}' from '${url}':`, error);
        return [];
    }
};