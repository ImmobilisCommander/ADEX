using System.Collections.Generic;
using Adex.Contracts.Explorer;

namespace Adex.Mcp.Tools
{
    public sealed record TopEntitiesResult(
        IReadOnlyList<TopEntity> TopContributors,
        IReadOnlyList<TopEntity> TopBeneficiaries
    );
}
