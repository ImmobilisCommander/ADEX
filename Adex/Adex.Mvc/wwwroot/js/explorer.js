(() => {
    const dashboard = document.querySelector("[data-dashboard-content]");
    if (dashboard) {
        const url = dashboard.dataset.dashboardUrl;
        let timer;
        let loading = false;
        let countUpFrame;
        const counters = [...dashboard.querySelectorAll("[data-countup-target]")];
        const fullNumber = new Intl.NumberFormat("fr-FR", { maximumFractionDigits: 0 });
        const countUpTau = 2000;
        const countUpStart = performance.now();
        const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

        const renderCounters = factor => {
            for (const counter of counters) {
                const value = Math.round(Number(counter.dataset.countupTarget) * factor);
                counter.textContent = `${fullNumber.format(value)}${counter.dataset.countupSuffix ?? ""}`;
            }
        };

        // Saturation exponentielle : v(t) = V * (1 - e^(-t/tau)), rapide puis de plus en plus lente.
        const animateCounters = now => {
            const elapsed = now - countUpStart;
            renderCounters(1 - Math.exp(-elapsed / countUpTau));
            countUpFrame = window.requestAnimationFrame(animateCounters);
        };

        if (reducedMotion) {
            renderCounters(1);
        } else {
            countUpFrame = window.requestAnimationFrame(animateCounters);
        }
        const chartObserver = "IntersectionObserver" in window
            ? new IntersectionObserver(entries => {
                for (const entry of entries) {
                    if (!entry.isIntersecting) continue;
                    entry.target.classList.add("is-visible");
                    chartObserver.unobserve(entry.target);
                }
            }, { rootMargin: "48px" })
            : null;

        const observeCharts = root => {
            root.querySelectorAll(".dashboard-loading-panel, [data-chart-motion]").forEach(chart => {
                if (chartObserver) {
                    chartObserver.observe(chart);
                } else {
                    chart.classList.add("is-visible");
                }
            });
        };

        const loadDashboard = async () => {
            if (document.hidden || loading || dashboard.dataset.loaded === "true") return;

            loading = true;
            try {
                const response = await fetch(url, {
                    headers: { Accept: "text/html" },
                    cache: "no-store"
                });
                if (response.status === 202) {
                    loading = false;
                    timer = window.setTimeout(loadDashboard, 1200);
                    return;
                }
                if (!response.ok) throw new Error("Le tableau de bord est indisponible.");

                window.cancelAnimationFrame(countUpFrame);
                dashboard.querySelectorAll(".dashboard-loading-panel").forEach(panel => chartObserver?.unobserve(panel));
                dashboard.innerHTML = await response.text();
                dashboard.classList.add("is-loaded");
                dashboard.dataset.loaded = "true";
                dashboard.setAttribute("aria-busy", "false");
                observeCharts(dashboard);
                loading = false;
            } catch {
                window.cancelAnimationFrame(countUpFrame);
                dashboard.innerHTML = '<p class="error-state" role="alert">Le tableau de bord est indisponible. Rechargez la page pour réessayer.</p>';
                dashboard.setAttribute("aria-busy", "false");
                loading = false;
            }
        };

        document.addEventListener("visibilitychange", () => {
            window.clearTimeout(timer);
            dashboard.classList.toggle("document-hidden", document.hidden);
            if (!document.hidden && dashboard.dataset.loaded !== "true") {
                loadDashboard();
            }
        });

        dashboard.classList.toggle("document-hidden", document.hidden);
        observeCharts(dashboard);
        loadDashboard();
    }

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
        meta.textContent = entity.detail ? `${entity.type} · ${entity.detail}` : entity.type;
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
