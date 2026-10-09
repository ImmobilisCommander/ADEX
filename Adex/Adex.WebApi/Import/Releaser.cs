using System;
using System.Threading;

namespace Adex.WebApi.Import
{
    internal sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose() => semaphore.Release();
    }
}
