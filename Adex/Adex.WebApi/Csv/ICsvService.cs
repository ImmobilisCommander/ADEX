using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.WebApi.Csv
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
