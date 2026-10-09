(() => {
    const host = document.querySelector("[data-csv-content]");
    if (!host) {
        return;
    }

    // Leaving the page aborts the request, which cancels the file read on the server.
    const controller = new AbortController();
    window.addEventListener("pagehide", () => controller.abort());

    fetch(host.dataset.contentUrl, {
        headers: { Accept: "text/html" },
        cache: "no-store",
        signal: controller.signal,
    })
        .then((response) => {
            if (!response.ok) {
                throw new Error("CSV content unavailable");
            }

            return response.text();
        })
        .then((html) => {
            host.innerHTML = html;
            host.setAttribute("aria-busy", "false");
            document.dispatchEvent(new Event("csv-content-loaded"));
        })
        .catch((error) => {
            if (error.name === "AbortError") {
                return;
            }

            host.innerHTML = '<p class="error-state" role="alert">Les données ne sont pas accessibles. Rechargez la page pour réessayer.</p>';
            host.setAttribute("aria-busy", "false");
        });
})();
