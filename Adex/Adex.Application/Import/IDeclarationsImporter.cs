using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.Application.Import
{
    public interface IDeclarationsImporter
    {
        Task<DeclarationsImportResult> ImportAsync(
            string path,
            IProgress<string> progress,
            CancellationToken cancellationToken
        );
    }
}
