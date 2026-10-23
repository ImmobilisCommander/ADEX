(() => {
    const overlay = document.querySelector('[data-page-loading]');
    if (!overlay) {
        return;
    }

    const show = () => {
        overlay.hidden = false;
        document.body.setAttribute('aria-busy', 'true');
    };
    const hide = () => {
        overlay.hidden = true;
        document.body.removeAttribute('aria-busy');
    };

    // API publique pour les chargements AJAX (compteur : plusieurs requêtes peuvent se chevaucher).
    let pending = 0;
    window.pageLoading = {
        begin() {
            pending++;
            show();
        },
        end() {
            pending = Math.max(0, pending - 1);
            if (pending === 0) {
                hide();
            }
        }
    };

    // Écouteur sur window (phase de bubbling) pour passer après les gestionnaires AJAX
    // qui appellent preventDefault ; la navigation native n'est alors pas annoncée.
    window.addEventListener('click', (event) => {
        if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
            return;
        }

        const link = event.target.closest('a[href]');
        if (!link || (link.target && link.target !== '_self') || link.hasAttribute('download')) {
            return;
        }

        const url = new URL(link.href, window.location.href);
        const sameOrigin = url.origin === window.location.origin;
        const samePageAnchor = url.pathname === window.location.pathname && url.search === window.location.search;
        if (!sameOrigin || !/^https?:$/.test(url.protocol) || samePageAnchor) {
            return;
        }

        show();
    });

    window.addEventListener('submit', (event) => {
        // Un formulaire method="dialog" ferme seulement une modale : aucune navigation.
        const form = event.target;
        const closesDialog = form.closest('dialog') || event.submitter?.formMethod === 'dialog'
            || form.method === 'dialog' || form.getAttribute('method') === 'dialog';
        if (!event.defaultPrevented && !closesDialog) {
            show();
        }
    });

    // Filet de sécurité : la fermeture d'une modale ne déclenche jamais de navigation.
    document.addEventListener('close', (event) => {
        if (event.target instanceof HTMLDialogElement && pending === 0) {
            hide();
        }
    }, true);

    // Retour arrière : la page restaurée du cache ne doit pas garder le calque.
    window.addEventListener('pageshow', hide);
})();
