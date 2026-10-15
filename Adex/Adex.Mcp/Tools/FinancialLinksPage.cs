using System;
using System.Collections.Generic;
using Adex.Contracts.Explorer;

namespace Adex.Mcp.Tools
{
    public sealed record FinancialLinksPage(
        Guid EntityId,
        string EntityName,
        int TotalCount,
        int Page,
        int PageCount,
        IReadOnlyList<FinancialLinkDetails> Links
    );
}
