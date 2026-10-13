(() => {
    const section = document.querySelector("[data-planetary-section]");
    if (!section) return;

    const helper = window.AdexCounterpartyLimit;
    const container = section.querySelector("[data-planetary-container]");
    const slider = section.querySelector("[data-counterparty-slider]");
    const activeBar = helper.activeBar(section);

    const centerViewport = () => {
        const viewport = section.querySelector("[data-planetary-viewport]");
        if (viewport && viewport.scrollWidth > viewport.clientWidth) {
            viewport.scrollLeft = (viewport.scrollWidth - viewport.clientWidth) / 2;
        }
    };

    centerViewport();

    const showLink = (event) => {
        const link = event.target.closest(".planetary-link");
        if (link) {
            activeBar.show(
                link.getAttribute("data-counterparty-name"),
                link.getAttribute("data-counterparty-amount")
            );
        }
    };

    section.addEventListener("mouseover", showLink);
    section.addEventListener("focusin", showLink);
    section.addEventListener("mouseout", (event) => {
        const link = event.target.closest(".planetary-link");
        if (link && !link.contains(event.relatedTarget)) {
            activeBar.reset();
        }
    });
    section.addEventListener("focusout", activeBar.reset);

    if (slider && container) {
        const endpointUrl = slider.getAttribute("data-counterparty-url");

        helper.bindSlider(section, async (limit, signal) => {
            const response = await fetch(helper.buildUrl(endpointUrl, limit), {
                signal,
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });
            if (!response.ok) throw new Error("Network error");

            container.innerHTML = await response.text();
            activeBar.reset();
            centerViewport();
            helper.setScope(
                section,
                container.querySelector("[data-planetary-scope]")?.getAttribute("data-planetary-scope")
            );
        });
    }
})();
