export function enableIntellisense(input, suggestions) {
    let dropdown = document.createElement("div");
    dropdown.classList.add("autocomplete-dropdown");
    document.body.appendChild(dropdown);

    input.addEventListener("input", () => {
        let value = input.value.toLowerCase();
        dropdown.innerHTML = "";
        let matches = suggestions.filter(s => s.toLowerCase().includes(value));

        matches.forEach(match => {
            let item = document.createElement("div");
            item.classList.add("autocomplete-item");
            item.textContent = match;
            item.addEventListener("click", () => {
                input.value = match;
                dropdown.innerHTML = "";
            });
            dropdown.appendChild(item);
        });

        let rect = input.getBoundingClientRect();
        dropdown.style.top = `${rect.bottom}px`;
        dropdown.style.left = `${rect.left}px`;
        dropdown.style.width = `${rect.width}px`;
    });

    document.addEventListener("click", e => {
        if (!dropdown.contains(e.target) && e.target !== input) {
            dropdown.innerHTML = "";
        }
    });
}
