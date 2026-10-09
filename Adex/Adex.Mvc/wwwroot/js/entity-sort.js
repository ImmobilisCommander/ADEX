(() => {
    const sectionSelector = ".entity-links";

    const load = async (section, url, focusSelector) => {
        section.setAttribute("aria-busy", "true");
        try {
            const response = await fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } });
            if (!response.ok) {
                throw new Error(response.statusText);
            }

            const doc = new DOMParser().parseFromString(await response.text(), "text/html");
            const fresh = doc.querySelector(sectionSelector);
            if (!fresh) {
                throw new Error("Section introuvable");
            }

            section.replaceWith(fresh);
            fresh.querySelector(focusSelector)?.focus();
        } catch {
            window.location.href = url;
        }
    };

    document.addEventListener("click", (event) => {
        const link = event.target.closest(`${sectionSelector} a.sort-button, ${sectionSelector} a.pager-link`);
        if (!link || event.ctrlKey || event.metaKey || event.shiftKey || event.button !== 0) {
            return;
        }

        event.preventDefault();
        const isSort = link.classList.contains("sort-button");
        load(link.closest(sectionSelector), link.href, isSort ? "th[aria-sort] a.sort-button" : ".pager-link.is-current");
    });

    // Capture phase, to take over from the generic auto-submit handler in site.js.
    document.addEventListener("change", (event) => {
        const select = event.target.closest(`${sectionSelector} select[data-autosubmit]`);
        if (!select || !select.form) {
            return;
        }

        event.stopPropagation();
        const form = select.form;
        const url = new URL(form.action, window.location.href);
        url.search = new URLSearchParams(new FormData(form)).toString();
        load(form.closest(sectionSelector), url.href, "#page-size");
    }, true);
})();
