using System.Collections.Generic;

namespace Adex.Mvc.Views.Home
{
    public sealed record RankingModel(
        string Id,
        string Title,
        IReadOnlyList<Adex.Mvc.Models.TopEntityViewModel> Entities
    );
}
