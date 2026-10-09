// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
    const themeKey = "adex-theme";
    const root = document.documentElement;
    const getSavedTheme = () => {
        try {
            return localStorage.getItem(themeKey) === "dark" ? "dark" : "light";
        } catch {
            return "light";
        }
    };

    root.dataset.theme = getSavedTheme();
})();

(() => {
    const menu = document.querySelector('[data-nav-menu]');
    if (!menu) {
        return;
    }

    const toggle = menu.querySelector('[data-nav-menu-toggle]');
    const list = menu.querySelector('.nav-menu-list');
    const setOpen = (open) => {
        list.hidden = !open;
        toggle.setAttribute('aria-expanded', String(open));
    };

    toggle.addEventListener('click', () => setOpen(list.hidden));
    document.addEventListener('click', (event) => {
        if (!menu.contains(event.target)) {
            setOpen(false);
        }
    });
    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape' && !list.hidden) {
            setOpen(false);
            toggle.focus();
        }
    });
})();

document.addEventListener('change', (event) => {
    const select = event.target.closest('select[data-autosubmit]');
    if (select && select.form) {
        select.form.submit();
    }
});
