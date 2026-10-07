using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adex.Common;

namespace Adex.Business
{
    public interface ILinkSearchService
    {
        Task<GraphDataSet> LinksToJsonAsync(
            string text,
            int take,
            CancellationToken cancellationToken
        );
    }

    public interface IMetadataLookupService
    {
        Task<Dictionary<string, string>> SearchAsync(
            string text,
            CancellationToken cancellationToken
        );

        Task<Dictionary<string, string>> GetBeneficiaryAsync(
            string reference,
            CancellationToken cancellationToken
        );
    }
}
