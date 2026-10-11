using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Adex.Mvc.Home
{
    public sealed record RankingModel(
        string Id,
        string Title,
        IReadOnlyList<Adex.Mvc.Home.TopEntityViewModel> Entities
    )
    {
        public IReadOnlyList<(Adex.Mvc.Home.TopEntityViewModel Entity, string Rank, int Width, string Amount)> Items
        {
            get
            {
                var topAmount = Entities.Count == 0 ? 0m : Entities.Max(entity => entity.Amount);
                return Entities
                    .Select(
                        (entity, index) =>
                            (
                                Entity: entity,
                                Rank: (index + 1).ToString("D2", CultureInfo.GetCultureInfo("fr-FR")),
                                Width: topAmount == 0
                                    ? 0
                                    : (int)Math.Round((double)(entity.Amount * 100m / topAmount)),
                                Amount: entity.Amount.ToString("C0", CultureInfo.GetCultureInfo("fr-FR"))
                            )
                    )
                    .ToList();
            }
        }
    }
}
