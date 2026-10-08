(() => {
    const themeKey = "adex-theme";
    const controls = document.querySelectorAll('input[name="theme"]');
    const status = document.querySelector(".settings-status");
    const savedTheme = document.documentElement.dataset.theme === "dark" ? "dark" : "light";

    document.querySelector(`input[name="theme"][value="${savedTheme}"]`).checked = true;

    controls.forEach((control) => {
        control.addEventListener("change", () => {
            if (!control.checked) {
                return;
            }

            document.documentElement.dataset.theme = control.value;
            try {
                localStorage.setItem(themeKey, control.value);
                status.textContent = `Thème ${control.value === "dark" ? "sombre" : "clair"} appliqué.`;
            } catch {
                status.textContent = "Le thème est appliqué, mais le navigateur ne permet pas de mémoriser ce choix.";
            }
        });
    });
})();
