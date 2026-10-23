using System;
using System.Threading;
using System.Threading.Tasks;
using Adex.Mvc.Components.Loading;
using Microsoft.AspNetCore.Components;

namespace Adex.Mvc.Components
{
    /// <summary>
    /// Base des composants qui appellent l'API : fournit <see cref="ToggleLoading"/> pour le calque
    /// de chargement et un jeton d'annulation lié à la durée de vie du composant.
    /// </summary>
    public abstract class AdexComponentBase : ComponentBase, IDisposable
    {
        private readonly CancellationTokenSource disposed = new();

        [Inject]
        private PageLoadingState LoadingState { get; set; }

        protected CancellationToken ComponentToken => disposed.Token;

        /// <summary>Affiche (<c>true</c>) ou masque (<c>false</c>) le calque de chargement.</summary>
        protected void ToggleLoading(bool show) => LoadingState.Toggle(show);

        /// <summary>
        /// Encadre une opération longue (appel d'API) par <see cref="ToggleLoading"/> :
        /// le calque est toujours masqué à la fin, même en cas d'erreur ou d'annulation.
        /// </summary>
        protected async Task RunWithLoadingAsync(Func<CancellationToken, Task> operation)
        {
            ToggleLoading(true);
            try
            {
                await operation(ComponentToken);
            }
            catch (OperationCanceledException) when (ComponentToken.IsCancellationRequested)
            {
                // Le composant a été supprimé pendant l'appel : rien à afficher.
            }
            finally
            {
                ToggleLoading(false);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                disposed.Cancel();
                disposed.Dispose();
            }
        }
    }
}
