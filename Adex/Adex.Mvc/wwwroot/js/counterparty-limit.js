window.AdexCounterpartyLimit = (() => {
    const activeBar = (section) => {
        const hint = section.querySelector("[data-counterparty-active-default]");
        const nameEl = section.querySelector("[data-counterparty-active-name]");
        const amountEl = section.querySelector("[data-counterparty-active-amount]");

        const reset = () => {
            if (!hint || !nameEl || !amountEl) return;
            hint.style.display = "";
            nameEl.textContent = "";
            nameEl.style.display = "none";
            amountEl.textContent = "";
            amountEl.style.display = "none";
        };

        const show = (name, amount) => {
            if (!hint || !nameEl || !amountEl) return;
            hint.style.display = "none";
            nameEl.textContent = name || "";
            nameEl.style.display = "inline";
            amountEl.textContent = amount ? ` (${amount})` : "";
            amountEl.style.display = "inline";
        };

        return { show, reset };
    };

    const buildUrl = (endpointUrl, limit) => {
        const url = new URL(endpointUrl, window.location.origin);
        url.searchParams.set("limit", limit);
        const fromQuery = new URLSearchParams(window.location.search).get("fromQuery");
        if (fromQuery) {
            url.searchParams.set("fromQuery", fromQuery);
        }
        return url.toString();
    };

    const updateLinksLimit = (limit) => {
        document.querySelectorAll("a[href*='sort='], a[href*='page='], a.pagination-link, a.sort-button").forEach((link) => {
            try {
                const url = new URL(link.href, window.location.origin);
                url.searchParams.set("limit", limit);
                link.href = url.pathname + url.search + url.hash;
            } catch { }
        });
    };

    const updateBrowserUrl = (limit) => {
        try {
            const url = new URL(window.location.href);
            url.searchParams.set("limit", limit);
            window.history.replaceState(null, "", url.pathname + url.search + url.hash);
            updateLinksLimit(limit);
        } catch { }
    };

    // load(limit, signal) fetches and renders the visualization; it is aborted when a newer value arrives.
    const bindSlider = (section, load) => {
        const slider = section.querySelector("[data-counterparty-slider]");
        const form = section.querySelector("[data-counterparty-form]");
        const valueDisplay = section.querySelector("[data-counterparty-limit-value]");
        const container = section.querySelector("[data-counterparty-container]");
        if (!slider || !container) return;

        let abortController = null;
        let debounceTimer = null;

        const refresh = (limit) => {
            if (abortController) {
                abortController.abort();
            }
            abortController = new AbortController();
            container.classList.add("is-loading");

            load(limit, abortController.signal)
                .then(() => {
                    container.classList.remove("is-loading");
                    updateBrowserUrl(limit);
                })
                .catch((error) => {
                    if (error.name !== "AbortError") {
                        container.classList.remove("is-loading");
                    }
                });
        };

        const cancelPending = () => {
            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }
        };

        slider.addEventListener("input", () => {
            const value = slider.value;
            if (valueDisplay) {
                valueDisplay.textContent = value;
            }
            cancelPending();
            debounceTimer = setTimeout(() => refresh(value), 120);
        });

        slider.addEventListener("change", () => {
            cancelPending();
            refresh(slider.value);
        });

        if (form) {
            form.addEventListener("submit", (event) => {
                event.preventDefault();
                cancelPending();
                refresh(slider.value);
            });
        }
    };

    const setScope = (section, scope) => {
        const countDisplay = section.querySelector("[data-counterparty-count-display]");
        if (scope && countDisplay) {
            countDisplay.textContent = scope;
        }
    };

    return { activeBar, bindSlider, buildUrl, setScope };
})();
