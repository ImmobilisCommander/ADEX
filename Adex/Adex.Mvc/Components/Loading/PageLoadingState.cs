using System;
using System.Threading;

namespace Adex.Mvc.Components.Loading
{
    /// <summary>
    /// État du calque de chargement d'un circuit Blazor. Un compteur permet à plusieurs
    /// opérations simultanées de partager le même calque : il disparaît quand la dernière se termine.
    /// </summary>
    public sealed class PageLoadingState
    {
        private int pending;

        public bool IsLoading => Volatile.Read(ref pending) > 0;

        public event Action Changed;

        public void Toggle(bool show)
        {
            if (show)
            {
                Interlocked.Increment(ref pending);
            }
            else
            {
                // Ne jamais descendre sous zéro, même si l'appel de fin est dupliqué.
                int current;
                do
                {
                    current = Volatile.Read(ref pending);
                }
                while (current > 0 && Interlocked.CompareExchange(ref pending, current - 1, current) != current);
            }

            Changed?.Invoke();
        }
    }
}
