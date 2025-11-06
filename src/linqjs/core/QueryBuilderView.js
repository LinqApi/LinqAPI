// core/QueryBuilderView.js
//
// Bu view tamamen opsiyonel. Controller.queryBuilderView olarak enjekte edilir.
// Görevi: filter / includes / groupBy / select değerlerini toplamak
// ve controller.applyQueryFromBuilder(...) çağırmak.
//
// Not: bu basit MVP; UI'yı istediğin kadar güzelleştirebilirsin.

export class QueryBuilderView {
    constructor({ controller }) {
        this.controller = controller;
        this._modalEl = null;
    }

    show({ properties, current }) {
        // properties = controller.properties (entity şema)
        // current = { filter, includes, groupBy, select }

        // includes için candidate list: complex / complexList alanlar
        const includeCandidates = (properties || [])
            .filter(p => p.kind === "complex" || p.kind === "complexList")
            .map(p => p.name)
            .filter(Boolean);

        // groupby/select için candidate list: simple / enum alanlar
        const simpleCandidates = (properties || [])
            .filter(p => p.kind === "simple" || p.kind === "enum")
            .map(p => p.name)
            .filter(Boolean);

        // mevcut değerleri stringe çevir
        const filterVal = current?.filter ?? this.controller.query.filter ?? "1=1";
        const groupByVal = current?.groupBy ?? this.controller.query.groupBy ?? "";
        const selectVal = current?.select ?? this.controller.query.select ?? "";

        const sortColVal = this.controller.sortColumn ?? "";
        const sortDescVal = !!this.controller.sortDesc;

        // includes şu an array, seçili olanları tutalım
        const selectedIncludes = new Set(
            Array.isArray(current?.includes)
                ? current.includes.map(i => i.propertyName || i.name || i.prop || i)
                : []
        );

        // includes çok kaba haliyle sadece propertyName listesi olarak gösterilecek
        // (gerçek include objesini sonra build edeceğiz)
        const includesChecklistHtml = includeCandidates.length
            ? includeCandidates.map(name => {
                const checked = selectedIncludes.has(name) ? "checked" : "";
                return `
                    <div class="form-check form-check-sm">
                        <input class="form-check-input" type="checkbox"
                               data-qb-include="${name}" ${checked}>
                        <label class="form-check-label small">${name}</label>
                    </div>
                `;
            }).join("")
            : `<div class="text-muted small">No complex relations.</div>`;

        const groupByOptionsHtml = simpleCandidates.map(name => {
            const sel = groupByVal.split(",").map(s => s.trim()).includes(name) ? "selected" : "";
            return `<option value="${name}" ${sel}>${name}</option>`;
        }).join("");

        const selectOptionsHtml = simpleCandidates.map(name => {
            const sel = selectVal.split(",").map(s => s.trim()).includes(name) ? "selected" : "";
            return `<option value="${name}" ${sel}>${name}</option>`;
        }).join("");

        // modal HTML (Bootstrap 5 compatible, lightweight)
        const html = `
        <div class="modal show"
             tabindex="-1"
             style="display:block; z-index:1055;"
             data-qb-modal>
            <div class="modal-dialog modal-lg modal-dialog-scrollable">
                <div class="modal-content">

                    <div class="modal-header">
                        <h5 class="modal-title">Query Builder</h5>
                        <button type="button" class="btn-close" data-qb-close></button>
                    </div>

                    <div class="modal-body">
                        <div class="row g-3">
                                                    <!-- ORDER BY -->
                            <div class="col-12 col-md-8">
                                <label class="form-label small fw-semibold">Order By</label>
                                <select class="form-select form-select-sm" data-qb-orderby>
                                  <option value="">(none)</option>
                                  ${simpleCandidates.map(n => `<option value="${n}" ${n === sortColVal ? 'selected' : ''}>${n}</option>`).join("")}
                                </select>
                            </div>
                            <div class="col-12 col-md-4">
                                <label class="form-label small fw-semibold">Descending</label>
                                <div class="form-check">
                                  <input class="form-check-input" type="checkbox" data-qb-desc ${sortDescVal ? 'checked' : ''}>
                                  <label class="form-check-label small">DESC</label>
                                </div>
                            </div>

                            <!-- FILTER -->
                            <div class="col-12">
                                <label class="form-label small fw-semibold">
                                    Filter (WHERE)
                                </label>
                                <input type="text"
                                       class="form-control form-control-sm"
                                       data-qb-filter
                                       value="${this._escape(filterVal)}"
                                       placeholder="e.g. firstName contains 'ali' AND privacyLevel = 0">
                                <div class="form-text small text-muted">
                                    Raw WHERE expression string sent as Query.filter
                                </div>
                            </div>

                            <!-- GROUP BY -->
                            <div class="col-12 col-md-6">
                                <label class="form-label small fw-semibold">
                                    Group By
                                </label>
                                <select class="form-select form-select-sm"
                                        data-qb-groupby
                                        multiple
                                        size="5">
                                    ${groupByOptionsHtml}
                                </select>
                                <div class="form-text small text-muted">
                                    Multiple allowed. We'll join with comma.
                                </div>
                            </div>

                            <!-- SELECT -->
                            <div class="col-12 col-md-6">
                                <label class="form-label small fw-semibold">
                                    Select
                                </label>
                                <select class="form-select form-select-sm"
                                        data-qb-select
                                        multiple
                                        size="5">
                                    ${selectOptionsHtml}
                                </select>
                                <div class="form-text small text-muted">
                                    You can choose columns to project. (Custom agg like COUNT(...) must be typed manually later if needed.)
                                </div>
                            </div>

                            <!-- INCLUDES -->
                            <div class="col-12">
                                <label class="form-label small fw-semibold">
                                    Includes (relations)
                                </label>
                                <div class="border rounded p-2" style="max-height:150px; overflow:auto;">
                                    ${includesChecklistHtml}
                                </div>
                                <div class="form-text small text-muted">
                                    We'll include these complex/complexList navigation props.
                                </div>
                            </div>

                            <!-- RAW OVERRIDES -->
                            <div class="col-12">
                                <label class="form-label small fw-semibold">
                                    Custom SELECT (raw)
                                </label>
                                <input type="text"
                                       class="form-control form-control-sm"
                                       data-qb-select-raw
                                       value="${this._escape(selectVal)}"
                                       placeholder="id, firstName, COUNT(friendRequestsSent) as friendCount">
                                <div class="form-text small text-muted">
                                    If filled, this overrides the multi-select above.
                                </div>
                            </div>

                            <div class="col-12">
                                <label class="form-label small fw-semibold">
                                    Custom GROUP BY (raw)
                                </label>
                                <input type="text"
                                       class="form-control form-control-sm"
                                       data-qb-groupby-raw
                                       value="${this._escape(groupByVal)}"
                                       placeholder="privacyLevel, notificationSetting">
                                <div class="form-text small text-muted">
                                    If filled, this overrides the group-by multiselect above.
                                </div>
                            </div>

                        </div>
                    </div>

                    <div class="modal-footer d-flex justify-content-between">
                        <button class="btn btn-sm btn-outline-secondary" data-qb-close>Cancel</button>
                        <button class="btn btn-sm btn-primary" data-qb-apply>Apply</button>
                    </div>

                </div>
            </div>
        </div>
        `;

        this._teardown(); // eğer eski modal açık kaldıysa temizle
        const wrapper = document.createElement("div");
        wrapper.innerHTML = html;
        this._modalEl = wrapper.firstElementChild;
        document.body.appendChild(this._modalEl);

        // cancel / close
        this._modalEl.querySelectorAll("[data-qb-close]").forEach(btn => {
            btn.addEventListener("click", () => this._teardown());
        });

        // apply
        const applyBtn = this._modalEl.querySelector("[data-qb-apply]");
        applyBtn.addEventListener("click", () => {
            const filterInput = this._modalEl.querySelector("[data-qb-filter]");
            const gbSelect = this._modalEl.querySelector("[data-qb-groupby]");
            const selSelect = this._modalEl.querySelector("[data-qb-select]");
            const gbRawInput = this._modalEl.querySelector("[data-qb-groupby-raw]");
            const selRawInput = this._modalEl.querySelector("[data-qb-select-raw]");

            // groupBy
            let groupByStr = gbRawInput.value.trim();
            if (!groupByStr) {
                const gbVals = Array.from(gbSelect.selectedOptions).map(o => o.value);
                groupByStr = gbVals.join(", ");
            }

            // select
            let selectStr = selRawInput.value.trim();
            if (!selectStr) {
                const selVals = Array.from(selSelect.selectedOptions).map(o => o.value);
                selectStr = selVals.join(", ");
            }

            // includes -> sadece propertyName listesiyle build edelim
            const includeProps = [];
            this._modalEl.querySelectorAll("[data-qb-include]").forEach(cb => {
                if (cb.checked) {
                    includeProps.push(cb.getAttribute("data-qb-include"));
                }
            });
            const orderBySel = this._modalEl.querySelector("[data-qb-orderby]");
            const descChk = this._modalEl.querySelector("[data-qb-desc]");
            // Bunları controller'a pushla
            this.controller.applyQueryFromBuilder({
                filter: filterInput.value.trim() || "1=1",
                groupBy: groupByStr,
                select: selectStr,
                includes: includeProps.map(name => ({
                    propertyName: name,
                    pager: { pageNumber: 1, pageSize: 10 },
                    thenIncludes: []
                })),
                sortColumn: (orderBySel?.value || ""),
                sortDesc: !!(descChk?.checked)
            });

            this._teardown();
        });
    }

    _teardown() {
        if (this._modalEl) {
            this._modalEl.remove();
            this._modalEl = null;
        }
    }

    _escape(v) {
        return String(v ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }
}
