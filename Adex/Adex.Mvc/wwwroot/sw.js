// Service worker minimal : rend l'application installable et affiche une page hors ligne explicite.
// Seuls la page hors ligne et les icônes sont mis en cache ; les pages, les API et les données
// (potentiellement personnelles ou dynamiques) ne sont jamais conservées.
const CACHE_NAME = "adex-shell-v1";
const PRECACHED = ["/offline.html", "/icons/icon-192.png", "/icons/icon-512.png"];

self.addEventListener("install", (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => cache.addAll(PRECACHED)).then(() => self.skipWaiting())
    );
});

self.addEventListener("activate", (event) => {
    event.waitUntil(
        caches
            .keys()
            .then((keys) => Promise.all(keys.filter((key) => key !== CACHE_NAME).map((key) => caches.delete(key))))
            .then(() => self.clients.claim())
    );
});

self.addEventListener("fetch", (event) => {
    const request = event.request;
    if (request.method !== "GET" || request.mode !== "navigate") {
        return;
    }

    // Réseau d'abord, sans cache : le repli hors ligne n'intervient qu'en cas d'échec réseau.
    event.respondWith(
        fetch(request).catch(async () => (await caches.match("/offline.html")) || Response.error())
    );
});
