(() => {
    const form = document.querySelector("[data-entity-search]");
    if (!form) return;

    const input = form.querySelector('input[name="query"]');
    const status = form.querySelector("[data-search-status]");
    const results = form.querySelector("[data-search-results]");
    let requestController;
    let timer;

    const clearResults = () => {
        results.replaceChildren();
        results.hidden = true;
    };

    const makeResult = (entity) => {
        const item = document.createElement("li");
        const link = document.createElement("a");
        const name = document.createElement("span");
        const meta = document.createElement("span");

        link.href = `/${encodeURIComponent(entity.id)}`;
        name.className = "search-result-name";
        name.textContent = entity.name;
        meta.className = "search-result-meta";
        meta.textContent = entity.type;
        link.append(name, meta);
        item.append(link);
        return item;
    };

    const search = async () => {
        const query = input.value.trim();
        clearResults();
        requestController?.abort();
        requestController = undefined;

        if (query.length < 2) {
            status.textContent = query ? "Saisissez au moins deux caractères." : "";
            return;
        }

        requestController = new AbortController();
        status.textContent = "Recherche en cours…";
        try {
            const response = await fetch(`/Search?query=${encodeURIComponent(query)}`, {
                headers: { Accept: "application/json" },
                signal: requestController.signal
            });
            if (!response.ok) throw new Error("La recherche n’a pas abouti.");

            const entities = await response.json();
            if (input.value.trim() !== query) return;
            if (entities.length === 0) {
                status.textContent = "Aucune entité trouvée.";
                return;
            }

            results.replaceChildren(...entities.map(makeResult));
            results.hidden = false;
            status.textContent = `${entities.length} résultat${entities.length > 1 ? "s" : ""}.`;
        } catch (error) {
            if (error.name === "AbortError") return;
            status.textContent = "La recherche est indisponible. Réessayez dans un instant.";
        }
    };

    input.addEventListener("input", () => {
        window.clearTimeout(timer);
        timer = window.setTimeout(search, 220);
    });

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        window.clearTimeout(timer);
        await search();
        if (input.value.trim().length >= 2) {
            window.location.assign(`/Search?query=${encodeURIComponent(input.value.trim())}`);
        }
    });

    document.addEventListener("click", (event) => {
        if (!form.contains(event.target)) clearResults();
    });
})();
