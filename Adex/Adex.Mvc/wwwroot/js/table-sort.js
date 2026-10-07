(() => {
    const collator = new Intl.Collator("fr", { numeric: true, sensitivity: "base" });

    document.querySelectorAll("table[data-sortable]").forEach((table) => {
        const tbody = table.tBodies[0];
        const headers = Array.from(table.tHead.rows[0].cells);

        const cellValue = (row, index) => {
            const cell = row.cells[index];
            return cell.dataset.sort ?? cell.textContent.trim();
        };

        const sortBy = (header, index) => {
            const type = header.dataset.sortType || "text";
            const direction = header.getAttribute("aria-sort") === "ascending" ? "descending" : "ascending";
            const factor = direction === "ascending" ? 1 : -1;

            headers.forEach((other) => other.removeAttribute("aria-sort"));
            header.setAttribute("aria-sort", direction);

            const rows = Array.from(tbody.rows);
            rows.sort((a, b) => {
                const left = cellValue(a, index);
                const right = cellValue(b, index);
                const result = type === "number"
                    ? (Number(left) || 0) - (Number(right) || 0)
                    : collator.compare(left, right);
                return result * factor;
            });
            tbody.append(...rows);
        };

        headers.forEach((header, index) => {
            if (!header.hasAttribute("data-sort-type")) return;

            const label = header.textContent.trim();
            const button = document.createElement("button");
            button.type = "button";
            button.className = "sort-button";
            button.innerHTML = `<span></span><i aria-hidden="true"></i>`;
            button.firstChild.textContent = label;
            button.title = `Trier par ${label.toLowerCase()}`;
            button.addEventListener("click", () => sortBy(header, index));
            header.replaceChildren(button);
        });
    });
})();
