using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adex.Contracts.Csv;

namespace Adex.Application.Csv
{
    public interface ICsvService
    {
        Task<CsvPage> GetPageAsync(
            int page,
            Dictionary<string, string> filters,
            CancellationToken cancellationToken
        );
    }
}
