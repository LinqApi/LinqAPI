export function enableDragDrop(table) {
    let headers = table.querySelectorAll("th");
    headers.forEach(th => {
        th.draggable = true;
        th.addEventListener("dragstart", e => {
            e.dataTransfer.setData("text/plain", th.getAttribute("data-column"));
        });

        th.addEventListener("drop", e => {
            e.preventDefault();
            let draggedColumn = e.dataTransfer.getData("text/plain");
            let targetColumn = th.getAttribute("data-column");

            if (draggedColumn !== targetColumn) {
                console.log(`Grouped by: ${draggedColumn}`);
                console.log(`Ordered by: ${targetColumn}`);
            }
        });

        th.addEventListener("dragover", e => e.preventDefault());
    });
}
