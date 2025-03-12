import { createDataTable } from "../datatable/index.js";
import { enableIntellisense } from "../intellisense/index.js";
import { buildQuery, fetchData } from "./linq.js";

export function createLinqTable(container, properties, endpoint) {
    const input = document.createElement("input");
    input.placeholder = "Search...";
    container.appendChild(input);

    enableIntellisense(input, properties);

    let tableContainer = document.createElement("div");
    container.appendChild(tableContainer);

    let query = buildQuery({});

    fetchData(endpoint, query).then(data => {
        createDataTable(tableContainer, properties, data.items);
    });

    input.addEventListener("input", () => {
        query.filter = input.value;
        fetchData(endpoint, query).then(data => {
            createDataTable(tableContainer, properties, data.items);
        });
    });
}
