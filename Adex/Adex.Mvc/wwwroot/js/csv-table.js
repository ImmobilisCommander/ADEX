document.addEventListener("csv-content-loaded", () => {
    const table = document.querySelector("[data-csv-table]");
    if (!table) {
        return;
    }

    const body = table.tBodies[0];
    const headers = Array.from(table.tHead.rows[0].cells);
    const names = headers.map((header) => header.dataset.csvColumn);
    const collator = new Intl.Collator("fr", { numeric: true, sensitivity: "base" });
    const toNumber = (text) => {
        const value = text.trim().replace(/\s/g, "").replace(",", ".");
        return value !== "" && /^-?\d+(\.\d+)?$/.test(value) ? Number(value) : null;
    };

    const compare = (a, b) => {
        const x = toNumber(a);
        const y = toNumber(b);
        return x !== null && y !== null ? x - y : collator.compare(a, b);
    };

    headers.forEach((header, index) => {
        header.querySelector(".sort-button").addEventListener("click", () => {
            const direction = header.getAttribute("aria-sort") === "ascending" ? "descending" : "ascending";
            const factor = direction === "ascending" ? 1 : -1;

            headers.forEach((other) => other.setAttribute("aria-sort", "none"));
            header.setAttribute("aria-sort", direction);

            Array.from(body.rows)
                .sort((a, b) => factor * compare(a.cells[index].textContent, b.cells[index].textContent))
                .forEach((row) => body.appendChild(row));
        });
    });

    // Column selection, remembered across pages.
    const storageKey = "adex-csv-hidden-columns";
    const toggle = document.querySelector("[data-csv-columns-toggle]");
    const panel = document.querySelector("[data-csv-columns-panel]");
    const checkboxes = Array.from(panel.querySelectorAll("input[data-csv-column]"));
    const loadHidden = () => {
        try {
            return new Set(JSON.parse(localStorage.getItem(storageKey) ?? "[]"));
        } catch {
            return new Set();
        }
    };

    const filterCells = Array.from(table.tHead.querySelectorAll(".csv-filter-row td"));
    const applyColumns = () => {
        const hidden = new Set(checkboxes.filter((box) => !box.checked).map((box) => box.dataset.csvColumn));
        names.forEach((name, index) => {
            const isHidden = hidden.has(name);
            headers[index].hidden = isHidden;
            filterCells[index].hidden = isHidden;
            // A hidden column must not silently filter the file.
            filterCells[index].querySelector("input")?.toggleAttribute("disabled", isHidden);
            Array.from(body.rows).forEach((row) => {
                row.cells[index].hidden = isHidden;
            });
        });

        try {
            localStorage.setItem(storageKey, JSON.stringify([...hidden]));
        } catch {
            // The selection simply is not remembered when storage is disabled.
        }
    };

    const initiallyHidden = loadHidden();
    checkboxes.forEach((box) => {
        box.checked = !initiallyHidden.has(box.dataset.csvColumn);
        box.addEventListener("change", applyColumns);
    });
    applyColumns();

    const setPanelOpen = (open) => {
        panel.hidden = !open;
        toggle.setAttribute("aria-expanded", String(open));
    };

    toggle.addEventListener("click", () => setPanelOpen(panel.hidden));
    document.addEventListener("click", (event) => {
        if (!panel.hidden && !panel.contains(event.target) && !toggle.contains(event.target)) {
            setPanelOpen(false);
        }
    });
    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && !panel.hidden) {
            setPanelOpen(false);
            toggle.focus();
        }
    });
    panel.querySelector("[data-csv-columns-all]").addEventListener("click", () => {
        checkboxes.forEach((box) => (box.checked = true));
        applyColumns();
    });
    panel.querySelector("[data-csv-columns-none]").addEventListener("click", () => {
        checkboxes.forEach((box) => (box.checked = false));
        applyColumns();
    });

    // Record dialog: every column is listed, including those hidden in the table.
    const dialog = document.querySelector("[data-csv-dialog]");
    const record = dialog.querySelector("[data-csv-record]");
    const showRecord = (row) => {
        record.replaceChildren();
        names.forEach((name, index) => {
            const label = document.createElement("dt");
            const value = document.createElement("dd");
            label.textContent = name;
            value.textContent = row.cells[index].textContent;
            record.append(label, value);
        });

        dialog.showModal();
    };

    body.addEventListener("click", (event) => {
        const row = event.target.closest("tr");
        if (row) {
            showRecord(row);
        }
    });
    body.addEventListener("keydown", (event) => {
        if ((event.key === "Enter" || event.key === " ") && event.target.matches("tr")) {
            event.preventDefault();
            showRecord(event.target);
        }
    });
    dialog.querySelector("[data-csv-dialog-close]").addEventListener("click", () => dialog.close());
    dialog.addEventListener("click", (event) => {
        if (event.target === dialog) {
            dialog.close();
        }
    });
}, { once: true });
