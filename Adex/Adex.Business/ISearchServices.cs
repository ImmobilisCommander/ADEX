using System.Collections.Generic;
using Adex.Common;

namespace Adex.Business
{
    public interface ILinkSearchService
    {
        GraphDataSet LinksToJson(string text, int? take);
    }

    public interface IMetadataLookupService
    {
        Dictionary<string, string> Search(string text);

        Dictionary<string, string> GetBeneficiary(string reference);
    }
}
