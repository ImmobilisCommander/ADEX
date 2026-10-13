(() => {
    const section = document.querySelector(".planetary-section");
    if (!section) return;

    const slider = section.querySelector("[data-planetary-slider]");
    const form = section.querySelector("[data-planetary-form]");
    const valueDisplay = section.querySelector("[data-planetary-limit-value]");
    const countDisplay = section.querySelector("[data-planetary-count-display]");
    const container = section.querySelector("[data-planetary-container]");
    const activeBar = section.querySelector("[data-planetary-active-bar]");
    const defaultHint = section.querySelector("[data-planetary-active-default]");
    const nameEl = section.querySelector("[data-planetary-active-name]");
    const amountEl = section.querySelector("[data-planetary-active-amount]");

    const centerViewport = () => {
        const viewport = section.querySelector("[data-planetary-viewport]");
        if (viewport && viewport.scrollWidth > viewport.clientWidth) {
            viewport.scrollLeft = (viewport.scrollWidth - viewport.clientWidth) / 2;
        }
    };

    centerViewport();

    const resetActiveBar = () => {
        if (defaultHint && nameEl && amountEl) {
            defaultHint.style.display = "";
            nameEl.textContent = "";
            nameEl.style.display = "none";
            amountEl.textContent = "";
            amountEl.style.display = "none";
        }
    };

    // Event delegation on section for satellite hover and focus
    section.addEventListener("mouseover", (e) => {
        const link = e.target.closest(".planetary-link");
        if (link && defaultHint && nameEl && amountEl) {
            const name = link.getAttribute("data-counterparty-name");
            const amount = link.getAttribute("data-counterparty-amount");
            defaultHint.style.display = "none";
            nameEl.textContent = name || "";
            nameEl.style.display = "inline";
            amountEl.textContent = amount ? ` (${amount})` : "";
            amountEl.style.display = "inline";
        }
    });

    section.addEventListener("mouseout", (e) => {
        const link = e.target.closest(".planetary-link");
        if (link && !link.contains(e.relatedTarget)) {
            resetActiveBar();
        }
    });

    section.addEventListener("focusin", (e) => {
        const link = e.target.closest(".planetary-link");
        if (link && defaultHint && nameEl && amountEl) {
            const name = link.getAttribute("data-counterparty-name");
            const amount = link.getAttribute("data-counterparty-amount");
            defaultHint.style.display = "none";
            nameEl.textContent = name || "";
            nameEl.style.display = "inline";
            amountEl.textContent = amount ? ` (${amount})` : "";
            amountEl.style.display = "inline";
        }
    });

    section.addEventListener("focusout", () => {
        resetActiveBar();
    });

    if (slider && container) {
        let abortController = null;
        let debounceTimer = null;
        const endpointUrl = slider.getAttribute("data-planetary-url");

        const updateLinksLimit = (limitValue) => {
            document.querySelectorAll("a[href*='sort='], a[href*='page='], a.pagination-link, a.sort-button").forEach((link) => {
                try {
                    const u = new URL(link.href, window.location.origin);
                    u.searchParams.set("limit", limitValue);
                    link.href = u.pathname + u.search + u.hash;
                } catch { }
            });
        };

        const updateBrowserUrl = (limitValue) => {
            try {
                const u = new URL(window.location.href);
                u.searchParams.set("limit", limitValue);
                window.history.replaceState(null, "", u.pathname + u.search + u.hash);
                updateLinksLimit(limitValue);
            } catch { }
        };

        const fetchPlanetary = (limitValue) => {
            if (!endpointUrl) return;

            if (abortController) {
                abortController.abort();
            }
            abortController = new AbortController();

            container.classList.add("is-loading");

            const url = new URL(endpointUrl, window.location.origin);
            url.searchParams.set("limit", limitValue);
            const currentParams = new URLSearchParams(window.location.search);
            const fromQuery = currentParams.get("fromQuery");
            if (fromQuery) {
                url.searchParams.set("fromQuery", fromQuery);
            }

            fetch(url.toString(), {
                signal: abortController.signal,
                headers: { "X-Requested-With": "XMLHttpRequest" }
            })
                .then((response) => {
                    if (!response.ok) throw new Error("Network error");
                    return response.text();
                })
                .then((html) => {
                    container.innerHTML = html;
                    container.classList.remove("is-loading");
                    resetActiveBar();
                    centerViewport();

                    const newScope = container.querySelector("[data-planetary-scope]")?.getAttribute("data-planetary-scope");
                    if (newScope && countDisplay) {
                        countDisplay.textContent = newScope;
                    }

                    updateBrowserUrl(limitValue);
                })
                .catch((err) => {
                    if (err.name !== "AbortError") {
                        container.classList.remove("is-loading");
                    }
                });
        };

        slider.addEventListener("input", () => {
            const val = slider.value;
            if (valueDisplay) {
                valueDisplay.textContent = val;
            }
            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }
            debounceTimer = setTimeout(() => {
                fetchPlanetary(val);
            }, 120);
        });

        slider.addEventListener("change", () => {
            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }
            fetchPlanetary(slider.value);
        });

        if (form) {
            form.addEventListener("submit", (e) => {
                e.preventDefault();
                if (debounceTimer) {
                    clearTimeout(debounceTimer);
                }
                fetchPlanetary(slider.value);
            });
        }
    }
})();

