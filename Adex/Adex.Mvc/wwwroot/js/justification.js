(() => {
    const dialog = document.getElementById("justification-dialog");
    if (!dialog || typeof dialog.showModal !== "function") {
        return;
    }

    const body = dialog.querySelector(".justification-body");
    let opener = null;

    const open = (row) => {
        const template = row.querySelector("template");
        if (!template) {
            return;
        }

        body.replaceChildren(template.content.cloneNode(true));
        opener = row;
        dialog.showModal();
    };

    document.addEventListener("click", (event) => {
        // A click on the backdrop targets the dialog element itself.
        if (event.target === dialog) {
            dialog.close();
            return;
        }

        const row = event.target.closest("[data-justification-row]");
        if (row && !event.target.closest("a, button")) {
            open(row);
        }
    });

    document.addEventListener("keydown", (event) => {
        if ((event.key === "Enter" || event.key === " ") && event.target.matches("[data-justification-row]")) {
            event.preventDefault();
            open(event.target);
        }
    });

    dialog.addEventListener("close", () => opener?.focus());
})();
