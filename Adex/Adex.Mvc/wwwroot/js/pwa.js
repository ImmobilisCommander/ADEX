(() => {
    if ("serviceWorker" in navigator) {
        window.addEventListener("load", () => {
            navigator.serviceWorker.register("/sw.js", { scope: "/" }).catch(() => {
                // L'application reste utilisable sans service worker ; seule l'installation est indisponible.
            });
        });
    }

    const installItems = document.querySelectorAll("[data-pwa-install]");
    const installButtons = document.querySelectorAll("[data-pwa-install-button]");
    const iosHelp = document.querySelectorAll("[data-pwa-ios-help]");
    const installedNotes = document.querySelectorAll("[data-pwa-installed]");
    const standalone =
        window.matchMedia("(display-mode: standalone)").matches || navigator.standalone === true;
    const isIos =
        /iphone|ipad|ipod/i.test(navigator.userAgent) ||
        (navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1);
    let deferredPrompt = null;

    const setHidden = (elements, hidden) => elements.forEach((element) => (element.hidden = hidden));

    if (standalone) {
        setHidden(installedNotes, false);
        return;
    }

    setHidden(iosHelp, !isIos);

    window.addEventListener("beforeinstallprompt", (event) => {
        event.preventDefault();
        deferredPrompt = event;
        setHidden(installItems, false);
    });

    installButtons.forEach((button) => {
        button.addEventListener("click", async () => {
            if (!deferredPrompt) {
                return;
            }

            const prompt = deferredPrompt;
            deferredPrompt = null;
            setHidden(installItems, true);
            await prompt.prompt();
        });
    });

    window.addEventListener("appinstalled", () => {
        deferredPrompt = null;
        setHidden(installItems, true);
        setHidden(iosHelp, true);
        setHidden(installedNotes, false);
    });
})();
