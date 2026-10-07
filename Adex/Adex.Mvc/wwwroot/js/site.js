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
