export function buildQuery({ filter, groupBy, orderBy, select, pageSize = 25, pageNumber = 1 }) {
    return {
        filter: filter || "true",
        groupBy: groupBy || "",
        orderBy: orderBy || "id",
        select: select || "*",
        pager: { pageNumber, pageSize }
    };
}

export async function fetchData(endpoint, query) {
    try {
        const response = await fetch(endpoint, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(query)
        });

        return await response.json();
    } catch (error) {
        console.error("Fetch error:", error);
        return { items: [], totalRecords: 0 };
    }
}
