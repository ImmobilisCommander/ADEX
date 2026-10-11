using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adex.Legacy.MetaModel.Graph;

namespace Adex.Legacy.MetaModel.Import
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
