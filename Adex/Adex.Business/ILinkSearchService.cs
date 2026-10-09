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
}
