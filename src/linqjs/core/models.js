// core/models.js
//
// Backend'in beklediği payload şemasına uygun sade modeller.
// Burada özellikle "filter" string olacak.
// includes formatı backend uyumlu tutuluyor.
//
// Not: Eğer projede zaten models.js varsa, sadece aşağıdaki
// Query ve Pager sınıflarını bu mantığa göre güncellemen yeterli.

export class Pager {
    constructor({ pageNumber = 1, pageSize = 20 } = {}) {
        // IMPORTANT:
        // Eğer backend ilk sayfayı 0 bekliyorsa,
        // DataTableController payload üretirken (pageNumber - 1) gönderecek.
        this.pageNumber = pageNumber;
        this.pageSize = pageSize;
    }
}

export class Query {
    constructor(controllerName) {
        this.controllerName = controllerName || "";

        // backend şu formatı bekliyor:
        // {
        //   "filter": "string",
        //   "pager": { "pageNumber": X, "pageSize": Y },
        //   "orderBy": "string",
        //   "desc": true,
        //   "groupBy": "string",
        //   "select": "string",
        //   "includes": [...]
        // }

        // default values:
        this.filter = "1=1";     // string olmalı
        this.orderBy = "";       // örn "id"
        this.desc = false;       // boolean
        this.groupBy = "";       // opsiyonel
        this.select = "";        // opsiyonel

        // includes örnek formatı:
        // [
        //   {
        //     propertyName: "Orders",
        //     pager: { pageNumber: 1, pageSize: 10 },
        //     thenIncludes: [{ childIncludes: ["Product"] }]
        //   }
        // ]
        this.includes = [];
    }

    setFilter(filterString) {
        // dışarıdan QueryBuilder bu methodla string filter set eder
        this.filter = filterString || "1=1";
    }

    setOrderBy(columnName, desc = false) {
        this.orderBy = columnName || "";
        this.desc = !!desc;
    }

    setGroupBy(groupByExpr) {
        this.groupBy = groupByExpr || "";
    }

    setSelect(selectExpr) {
        this.select = selectExpr || "";
    }

    setIncludes(includesArr) {
        // beklenen formatta bir array geldiğini varsayıyoruz
        this.includes = Array.isArray(includesArr) ? includesArr : [];
    }

    // Bu method artık doğrudan backend şemasını dönebilir.
    // Controller isterse bunu kullanabilir.
toPayload(pagerState, sortColumnOverride, sortDescOverride) {
    // override > current state fallback
    const orderBy = (sortColumnOverride != null)
        ? sortColumnOverride
        : this.orderBy;

    const desc = (sortColumnOverride != null)
        ? !!sortDescOverride
        : !!this.desc;

    // başlangıç payload (minimum zorunlu alanlar)
    const payload = {
        filter: this.filter || "1=1",
        pager: {
            pageNumber: pagerState.pageNumber,
            pageSize: pagerState.pageSize
        }
    };

    // orderBy varsa ekle
    if (orderBy && orderBy.trim() !== "") {
        payload.orderBy = orderBy;
        payload.desc = !!desc;
    }

    // groupBy varsa ekle
    if (this.groupBy && this.groupBy.trim() !== "") {
        payload.groupBy = this.groupBy;
    }

    // select varsa ekle
    if (this.select && this.select.trim() !== "") {
        payload.select = this.select;
    }

    // includes varsa ekle
    const normInc = normalizeIncludes(this.includes);
    if (Array.isArray(normInc) && normInc.length > 0) {
        payload.includes = normInc;
    }

    return payload;
}

}

// internal helper to normalize includes shape
function normalizeIncludes(rawIncludes) {
    if (!Array.isArray(rawIncludes)) return [];

    return rawIncludes.map(inc => {
        const propertyName = inc.propertyName || inc.name || inc.prop || "";

        // child pager (varsayılan 1/10)
        const childPager = inc.pager || {};
        const childPageNumber = (typeof childPager.pageNumber === "number")
            ? childPager.pageNumber
            : 1;
        const childPageSize = (typeof childPager.pageSize === "number")
            ? childPager.pageSize
            : 10;

        // thenIncludes formatı
        let thenIncludes = [];
        if (Array.isArray(inc.thenIncludes)) {
            thenIncludes = inc.thenIncludes.map(t => {
                if (Array.isArray(t.childIncludes)) {
                    return { childIncludes: t.childIncludes.slice() };
                }
                if (Array.isArray(t)) {
                    return { childIncludes: t.slice() };
                }
                return { childIncludes: [] };
            });
        } else if (Array.isArray(inc.childIncludes)) {
            // eski style: ["Roles","Permissions"]
            thenIncludes = [
                { childIncludes: inc.childIncludes.slice() }
            ];
        }

        return {
            propertyName,
            pager: {
                pageNumber: childPageNumber,
                pageSize: childPageSize
            },
            thenIncludes
        };
    });
}
