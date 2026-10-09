using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adex.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Adex.WebApi.Explorer
{
    public sealed class DataExplorerService
    {
        public const string DashboardCacheKey = "adex-dashboard";

        private const int SearchResultLimit = 20;
        private const int SearchMinimumLength = 3;
        public static readonly string[] SortKeys = { "date", "amount", "type", "kind", "direction", "name" };
        public const int DefaultPageSize = 20;
        public static readonly int[] PageSizes = { 10, 20, 50 };

        private static readonly SemaphoreSlim DashboardLock = new(1, 1);

        private readonly IDbContextFactory<AdexContext> _contextFactory;
        private readonly IMemoryCache _cache;

        public DataExplorerService(IDbContextFactory<AdexContext> contextFactory, IMemoryCache cache)
        {
            _contextFactory = contextFactory;
            _cache = cache;
        }

        public async Task<DashboardModel> GetDashboardAsync(CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(DashboardCacheKey, out DashboardModel cached))
            {
                return cached;
            }

            await DashboardLock.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue(DashboardCacheKey, out cached))
                {
                    return cached;
                }

                var dashboard = await BuildDashboardAsync(cancellationToken);
                _cache.Set(DashboardCacheKey, dashboard, TimeSpan.FromHours(6));
                return dashboard;
            }
            finally
            {
                DashboardLock.Release();
            }
        }

        private async Task<DashboardModel> BuildDashboardAsync(CancellationToken cancellationToken)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);
            // Agrégat sur toute la table des liens, mis en cache ensuite : le délai par défaut (30 s) est trop court.
            db.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

            var companiesCount = await db.Companies.CountAsync(cancellationToken);
            var personsCount = await db.Persons.CountAsync(cancellationToken);

            var linkTypes = (await db.FinancialLinkTypeTotals.ToListAsync(cancellationToken))
                .Select(total => new FinancialLinkTypeSummary
                {
                    Type = total.Type,
                    Count = (int)total.Count,
                    Amount = total.Amount,
                })
                .OrderByDescending(summary => summary.Amount)
                .ToList();

            var topContributors = await (
                from total in db.EntityTotals
                join company in db.Companies on total.EntityId equals company.Id
                where total.OutgoingAmount > 0
                orderby total.OutgoingAmount descending
                select new TopEntity
                {
                    Id = company.Id,
                    Name = company.Designation ?? "(sans nom)",
                    Type = "Contributeur",
                    Amount = total.OutgoingAmount,
                }
            )
                .Take(10)
                .ToListAsync(cancellationToken);

            var beneficiaryRows = await (
                from total in db.EntityTotals
                join person in db.Persons on total.EntityId equals person.Id
                where total.IncomingAmount > 0
                orderby total.IncomingAmount descending
                select new
                {
                    person.Id,
                    person.FirstName,
                    person.LastName,
                    Amount = total.IncomingAmount,
                }
            )
                .Take(10)
                .ToListAsync(cancellationToken);
            var topBeneficiaries = beneficiaryRows
                .Select(person => new TopEntity
                {
                    Id = person.Id,
                    Name = JoinName(person.FirstName, person.LastName),
                    Type = "Bénéficiaire",
                    Amount = person.Amount,
                })
                .ToList();

            var monthlyDeclarations = await GetMonthlyDeclarationsAsync(db, cancellationToken);
            var concentration = new List<ConcentrationCurve>
            {
                BuildConcentration(
                    "Bénéficiaires (montants reçus)",
                    await (
                        from total in db.EntityTotals
                        join person in db.Persons on total.EntityId equals person.Id
                        where total.IncomingAmount > 0
                        select total.IncomingAmount
                    ).ToListAsync(cancellationToken)
                ),
                BuildConcentration(
                    "Entreprises (montants versés)",
                    await (
                        from total in db.EntityTotals
                        join company in db.Companies on total.EntityId equals company.Id
                        where total.OutgoingAmount > 0
                        select total.OutgoingAmount
                    ).ToListAsync(cancellationToken)
                ),
            };
            var amountHistogram = await GetAmountHistogramAsync(db, cancellationToken);

            return new DashboardModel
            {
                MonthlyDeclarations = monthlyDeclarations,
                Concentration = concentration,
                AmountHistogram = amountHistogram,
                EntityCount = companiesCount + personsCount,
                FinancialLinkCount = linkTypes.Sum(summary => summary.Count),
                TotalAmount = linkTypes.Sum(summary => summary.Amount),
                EntityTypes = new List<EntityTypeCount>
                {
                    new() { Type = "Entreprise", Count = companiesCount },
                    new() { Type = "Bénéficiaire", Count = personsCount },
                },
                FinancialLinkTypes = linkTypes,
                TopContributors = topContributors,
                TopBeneficiaries = topBeneficiaries,
            };
        }

        // La source contient des dates aberrantes (année 0001, 1900 par défaut, etc.) : on les exclut du graphique.
        private static readonly DateTime MonthlyChartStart = new(2012, 1, 1);

        private static async Task<List<MonthlyDeclarationCount>> GetMonthlyDeclarationsAsync(
            AdexContext db,
            CancellationToken cancellationToken
        )
        {
            var now = DateTime.UtcNow;
            var end = new DateTime(now.Year, now.Month, 1).AddMonths(1);

            var rows = await db.Database
                .SqlQuery<MonthRow>(
                    $"""
                    SELECT date_part('year', "Date")::int AS "Year", date_part('month', "Date")::int AS "Month", count(*)::int AS "Count"
                    FROM public."Links"
                    WHERE "Date" >= {MonthlyChartStart.ToString("yyyy-MM-dd")}::timestamp AND "Date" < {end.ToString("yyyy-MM-dd")}::timestamp
                    GROUP BY 1, 2
                    """
                )
                .ToListAsync(cancellationToken);
            if (rows.Count == 0)
            {
                return new List<MonthlyDeclarationCount>();
            }

            var counts = rows.ToDictionary(row => (row.Year, row.Month), row => row.Count);
            var first = rows.Min(row => new DateTime(row.Year, row.Month, 1));
            var last = rows.Max(row => new DateTime(row.Year, row.Month, 1));
            var months = new List<MonthlyDeclarationCount>();
            for (var month = first; month <= last; month = month.AddMonths(1))
            {
                counts.TryGetValue((month.Year, month.Month), out var count);
                months.Add(new MonthlyDeclarationCount { Year = month.Year, Month = month.Month, Count = count });
            }

            return months;
        }

        // Agrégats globaux en SQL direct : l'héritage TPT ferait joindre Entities, Links et FinancialLinks sur des millions de lignes.
        private static readonly double[] ConcentrationPercents =
            { 0.1, 0.25, 0.5, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100 };

        // Courbe de Pareto : part cumulée du montant détenue par les x % les plus importants.
        private static ConcentrationCurve BuildConcentration(string label, List<decimal> amounts)
        {
            var curve = new ConcentrationCurve { Label = label, Count = amounts.Count };
            if (amounts.Count == 0)
            {
                return curve;
            }

            amounts.Sort((left, right) => right.CompareTo(left));
            var cumulative = new decimal[amounts.Count];
            decimal running = 0;
            for (var i = 0; i < amounts.Count; i++)
            {
                running += amounts[i];
                cumulative[i] = running;
            }

            curve.Total = running;
            double ShareOfTop(int count) =>
                (double)(cumulative[Math.Clamp(count, 1, amounts.Count) - 1] / running) * 100d;

            foreach (var percent in ConcentrationPercents)
            {
                var count = (int)Math.Ceiling(amounts.Count * percent / 100d);
                curve.Points.Add(new ConcentrationPoint
                {
                    PopulationPercent = percent,
                    AmountPercent = ShareOfTop(count),
                });
            }

            curve.TopOnePercentShare = ShareOfTop((int)Math.Ceiling(amounts.Count / 100d));
            curve.TopTenShare = ShareOfTop(10);
            return curve;
        }

        // Tranches d'une demi-décade (1, 3,16, 10, 31,6, 100...) ; la tranche 0 regroupe les montants nuls ou négatifs.
        private static async Task<List<AmountHistogramBin>> GetAmountHistogramAsync(
            AdexContext db,
            CancellationToken cancellationToken
        )
        {
            var rows = await db.Database
                .SqlQuery<HistogramRow>(
                    $"""
                    SELECT CASE WHEN "Amount" <= 0 THEN -1000 ELSE floor(log10("Amount"::double precision) * 2)::int END AS "Key",
                           count(*)::int AS "Count"
                    FROM public."FinancialLinks"
                    GROUP BY 1
                    """
                )
                .ToListAsync(cancellationToken);
            if (rows.Count == 0)
            {
                return new List<AmountHistogramBin>();
            }

            var positive = rows.Where(row => row.Key > -1000).ToList();
            var bins = new List<AmountHistogramBin>();
            var nonPositive = rows.FirstOrDefault(row => row.Key <= -1000);
            if (nonPositive is not null)
            {
                bins.Add(new AmountHistogramBin { Count = nonPositive.Count });
            }

            if (positive.Count == 0)
            {
                return bins;
            }

            var counts = positive.ToDictionary(row => row.Key, row => row.Count);
            for (var key = counts.Keys.Min(); key <= counts.Keys.Max(); key++)
            {
                counts.TryGetValue(key, out var count);
                bins.Add(new AmountHistogramBin
                {
                    LowerBound = HalfDecade(key),
                    UpperBound = HalfDecade(key + 1),
                    Count = count,
                });
            }

            return bins;
        }

        private static decimal HalfDecade(int key)
        {
            return Math.Round((decimal)Math.Pow(10d, key / 2d), 2);
        }

        private static async Task<(List<EntityYearActivity> Activity, List<EntityTypeBreakdown> Breakdown)> GetEntityActivityAsync(
            AdexContext db,
            Guid entityId,
            CancellationToken cancellationToken
        )
        {
            // Une requête par sens pour utiliser les index (entité, date) plutôt qu'un OR sur les deux colonnes.
            var outgoing = await db.FinancialLinks
                .Where(link => link.From_Id == entityId)
                .GroupBy(link => new { link.DeclarationType, link.Date.Year })
                .Select(group => new { group.Key.DeclarationType, group.Key.Year, Count = group.Count(), Amount = group.Sum(link => link.Amount) })
                .ToListAsync(cancellationToken);
            var incoming = await db.FinancialLinks
                .Where(link => link.To_Id == entityId)
                .GroupBy(link => new { link.DeclarationType, link.Date.Year })
                .Select(group => new { group.Key.DeclarationType, group.Key.Year, Count = group.Count(), Amount = group.Sum(link => link.Amount) })
                .ToListAsync(cancellationToken);
            var rows = outgoing.Concat(incoming).ToList();

            var breakdown = rows
                .GroupBy(row => row.DeclarationType ?? "Non renseigné")
                .Select(group => new EntityTypeBreakdown
                {
                    Type = group.Key,
                    Count = group.Sum(row => row.Count),
                    Amount = group.Sum(row => row.Amount),
                })
                .OrderByDescending(item => item.Count)
                .ToList();

            var byYear = rows
                .Where(row => row.Year >= MonthlyChartStart.Year && row.Year <= DateTime.UtcNow.Year)
                .GroupBy(row => row.Year)
                .ToDictionary(
                    group => group.Key,
                    group => new EntityYearActivity
                    {
                        Year = group.Key,
                        Count = group.Sum(row => row.Count),
                        Amount = group.Sum(row => row.Amount),
                    });
            var activity = new List<EntityYearActivity>();
            if (byYear.Count > 0)
            {
                for (var year = byYear.Keys.Min(); year <= byYear.Keys.Max(); year++)
                {
                    activity.Add(byYear.TryGetValue(year, out var item) ? item : new EntityYearActivity { Year = year });
                }
            }

            return (activity, breakdown);
        }

        public async Task<List<EntitySearchResult>> SearchEntitiesAsync(
            string query,
            CancellationToken cancellationToken
        )
        {
            query = query?.Trim();
            if (string.IsNullOrEmpty(query))
            {
                return new List<EntitySearchResult>();
            }

            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var results = new List<EntitySearchResult>();

            var queryId = Guid.TryParse(query, out var parsedId) ? parsedId : Guid.Empty;
            var byReference = await db.Companies
                .Where(company => company.Id == queryId)
                .Select(company => new EntitySearchResult
                {
                    Id = company.Id,
                    Name = company.Designation ?? "(sans nom)",
                    Type = "Entreprise",
                })
                .ToListAsync(cancellationToken);
            results.AddRange(byReference);

            var personByReference = await db.Persons
                .Where(person => person.Id == queryId)
                .Select(person => new
                {
                    Id = person.Id,
                    person.FirstName,
                    person.LastName,
                })
                .ToListAsync(cancellationToken);
            results.AddRange(
                personByReference.Select(person => new EntitySearchResult
                {
                    Id = person.Id,
                    Name = JoinName(person.FirstName, person.LastName),
                    Type = "Bénéficiaire",
                })
            );

            // Name searches rely on trigram indexes, which need at least three characters.
            if (query.Length >= SearchMinimumLength)
            {
                var pattern = $"%{EscapeLikePattern(query)}%";
                const string escapeCharacter = "\\";

                var companies = await db.Companies
                    .Where(company =>
                        company.Designation != null
                        && EF.Functions.ILike(company.Designation, pattern, escapeCharacter)
                    )
                    .OrderBy(company => company.Designation)
                    .Take(SearchResultLimit)
                    .Select(company => new EntitySearchResult
                    {
                        Id = company.Id,
                        Name = company.Designation,
                        Type = "Entreprise",
                    })
                    .ToListAsync(cancellationToken);
                results.AddRange(companies);

                var persons = await db.Persons
                    .Where(person =>
                        (
                            person.FirstName != null
                            && EF.Functions.ILike(person.FirstName, pattern, escapeCharacter)
                        )
                        || (
                            person.LastName != null
                            && EF.Functions.ILike(person.LastName, pattern, escapeCharacter)
                        )
                    )
                    .OrderBy(person => person.LastName)
                    .ThenBy(person => person.FirstName)
                    .Take(SearchResultLimit)
                    .Select(person => new
                    {
                        Id = person.Id,
                        person.FirstName,
                        person.LastName,
                    })
                    .ToListAsync(cancellationToken);
                results.AddRange(
                    persons.Select(person => new EntitySearchResult
                    {
                        Id = person.Id,
                        Name = JoinName(person.FirstName, person.LastName),
                        Type = "Bénéficiaire",
                    })
                );
            }

            return results
                .GroupBy(result => result.Id)
                .Select(group => group.First())
                .OrderBy(result => result.Name)
                .ThenBy(result => result.Id)
                .Take(SearchResultLimit)
                .ToList();
        }

        public async Task<EntityDetails> GetEntityAsync(
            Guid id,
            int page,
            int pageSize,
            string sort,
            bool descending,
            CancellationToken cancellationToken
        )
        {
            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var company = await db.Companies
                .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
            var person = company is null
                ? await db.Persons
                    .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken)
                : null;
            var otherEntity = company is null && person is null
                ? await db.Entities
                    .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken)
                : null;
            if (company is null && person is null && otherEntity is null)
            {
                return null;
            }

            var entity = (Entity)company ?? person ?? otherEntity;
            var entityId = entity.Id;
            var attributeData = await db.EntityAttributes
                .Where(attribute => attribute.EntityId == entityId)
                .Select(attribute => attribute.Data)
                .SingleOrDefaultAsync(cancellationToken);
            var attributes = (attributeData ?? new Dictionary<string, string>())
                .OrderBy(pair => pair.Key, StringComparer.CurrentCulture)
                .Select(pair => new EntityAttributeDetails { Name = pair.Key, Value = pair.Value })
                .ToList();
            var name = company?.Designation
                ?? (person is null
                    ? FindAttributeName(attributes)
                    : JoinName(person.FirstName, person.LastName));

            var totals = await db.EntityTotals
                .Where(total => total.EntityId == entityId)
                .SingleOrDefaultAsync(cancellationToken);

            pageSize = Array.IndexOf(PageSizes, pageSize) >= 0 ? pageSize : DefaultPageSize;
            sort = SortKeys.Contains(sort) ? sort : "date";
            var linkCount = totals?.LinkCount ?? 0;
            var pageCount = Math.Max(1, (linkCount + pageSize - 1) / pageSize);
            page = Math.Clamp(page, 1, pageCount);
            var skip = (page - 1) * pageSize;
            var links = await LoadLinksPageAsync(db, entityId, sort, descending, skip, pageSize, cancellationToken);

            var linkIds = links.Select(link => link.Id).ToList();
            var justificationRows = await db.EntityAttributes
                .Where(attribute => linkIds.Contains(attribute.EntityId))
                .Select(attribute => new { attribute.EntityId, attribute.Data })
                .ToListAsync(cancellationToken);
            var justifications = justificationRows.ToDictionary(
                row => row.EntityId,
                row => JustificationNames
                    .Where(name => row.Data.ContainsKey(name))
                    .Select(name => new EntityAttributeDetails { Name = name, Value = row.Data[name] })
                    .ToList()
            );
            foreach (var link in links)
            {
                link.Justification = justifications.GetValueOrDefault(link.Id) ?? new List<EntityAttributeDetails>();
            }

            var counterparties = links
                .Select(link => link.CounterpartyId)
                .Distinct()
                .ToList();
            var companyNames = await db.Companies
                .Where(candidate => counterparties.Contains(candidate.Id))
                .Select(candidate => new
                {
                    candidate.Id,
                    Name = candidate.Designation ?? "(sans nom)",
                })
                .ToDictionaryAsync(candidate => candidate.Id, candidate => candidate.Name, cancellationToken);
            var personNameRows = await db.Persons
                .Where(candidate => counterparties.Contains(candidate.Id))
                .Select(candidate => new
                {
                    candidate.Id,
                    candidate.FirstName,
                    candidate.LastName,
                })
                .ToListAsync(cancellationToken);
            var personNames = personNameRows.ToDictionary(
                candidate => candidate.Id,
                candidate => JoinName(candidate.FirstName, candidate.LastName)
            );

            foreach (var link in links)
            {
                if (companyNames.TryGetValue(link.CounterpartyId, out var companyName))
                {
                    link.CounterpartyName = companyName;
                }
                else if (personNames.TryGetValue(link.CounterpartyId, out var personName))
                {
                    link.CounterpartyName = personName;
                }
                else
                {
                    link.CounterpartyName = "(inconnu)";
                }
            }

            var (activity, breakdown) = await GetEntityActivityAsync(db, entityId, cancellationToken);

            return new EntityDetails
            {
                YearlyActivity = activity,
                TypeBreakdown = breakdown,
                Id = entity.Id,
                Name = string.IsNullOrWhiteSpace(name) ? "(sans nom)" : name,
                Type = company is not null
                    ? "Entreprise"
                    : person is not null ? "Bénéficiaire" : "Entité",
                OutgoingAmount = totals?.OutgoingAmount ?? 0m,
                IncomingAmount = totals?.IncomingAmount ?? 0m,
                FinancialLinkCount = linkCount,
                Page = page,
                Sort = sort,
                Descending = descending,
                PageSize = pageSize,
                PageCount = pageCount,
                Attributes = attributes,
                FinancialLinks = links,
            };
        }

        // One index-friendly query per direction (instead of an OR over both columns), merged by UNION ALL
        // so that sorting and paging happen in the database.
        private static async Task<List<FinancialLinkDetails>> LoadLinksPageAsync(
            AdexContext db,
            Guid entityId,
            string sort,
            bool descending,
            int skip,
            int take,
            CancellationToken cancellationToken
        )
        {
            var byName = sort == "name";
            var outgoing = Project(db, db.FinancialLinks.Where(link => link.From_Id == entityId), true, byName);
            var incoming = Project(db, db.FinancialLinks.Where(link => link.To_Id == entityId), false, byName);

            if (sort == "date")
            {
                // Fast path: each branch uses its (entity, date) index for a top-N, then the two are merged.
                var limit = skip + take;
                var outgoingTop = await (descending
                    ? outgoing.OrderByDescending(link => link.Date)
                    : outgoing.OrderBy(link => link.Date))
                    .ThenBy(link => link.Id).Take(limit).ToListAsync(cancellationToken);
                var incomingTop = await (descending
                    ? incoming.OrderByDescending(link => link.Date)
                    : incoming.OrderBy(link => link.Date))
                    .ThenBy(link => link.Id).Take(limit).ToListAsync(cancellationToken);
                var merged = outgoingTop.Concat(incomingTop);
                return (descending
                    ? merged.OrderByDescending(link => link.Date)
                    : merged.OrderBy(link => link.Date))
                    .ThenBy(link => link.Id)
                    .Skip(skip)
                    .Take(take)
                    .ToList();
            }

            var all = outgoing.Concat(incoming);

            var ordered = sort switch
            {
                "amount" => descending ? all.OrderByDescending(link => link.Amount) : all.OrderBy(link => link.Amount),
                "type" => descending ? all.OrderByDescending(link => link.DeclarationType) : all.OrderBy(link => link.DeclarationType),
                "kind" => descending ? all.OrderByDescending(link => link.Kind) : all.OrderBy(link => link.Kind),
                "direction" => descending ? all.OrderByDescending(link => link.Outgoing) : all.OrderBy(link => link.Outgoing),
                "name" => descending ? all.OrderByDescending(link => link.SortName) : all.OrderBy(link => link.SortName),
                _ => descending ? all.OrderByDescending(link => link.Date) : all.OrderBy(link => link.Date),
            };

            return await ordered
                .ThenBy(link => link.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        private static IQueryable<FinancialLinkDetails> Project(
            AdexContext db,
            IQueryable<FinancialLink> links,
            bool outgoing,
            bool byName
        )
        {
            return links.Select(link => new FinancialLinkDetails
            {
                Id = link.Id,
                CounterpartyId = outgoing ? link.To_Id : link.From_Id,
                DeclarationType = link.DeclarationType ?? "Non renseigné",
                Kind = link.Kind ?? string.Empty,
                Date = link.Date,
                Amount = link.Amount,
                Outgoing = outgoing,
                SortName = !byName
                    ? null
                    : db.Companies
                        .Where(company => company.Id == (outgoing ? link.To_Id : link.From_Id))
                        .Select(company => company.Designation)
                        .FirstOrDefault()
                        ?? db.Persons
                            .Where(person => person.Id == (outgoing ? link.To_Id : link.From_Id))
                            .Select(person => person.FirstName + " " + person.LastName)
                            .FirstOrDefault()
                        ?? string.Empty,
            });
        }
        private static readonly string[] JustificationNames =
        {
            "Motif",
            "Autre motif",
            "Information sur l'événement",
            "Convention liée",
            "Date de début",
            "Date de fin",
            "Date de publication",
            "Statut",
        };

        private static string EscapeLikePattern(string value)
        {
            return value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("%", "\\%", StringComparison.Ordinal)
                .Replace("_", "\\_", StringComparison.Ordinal);
        }

        private static string JoinName(string firstName, string lastName)
        {
            return string.Join(
                " ",
                new[] { firstName, lastName }.Where(value =>
                    !string.IsNullOrWhiteSpace(value)
                )
            );
        }

        private static string FindAttributeName(IEnumerable<EntityAttributeDetails> attributes)
        {
            var values = attributes.ToDictionary(attribute => attribute.Name, attribute => attribute.Value);
            if (values.TryGetValue("denomination_sociale", out var companyName))
            {
                return companyName;
            }

            values.TryGetValue("benef_prenom", out var firstName);
            values.TryGetValue("benef_nom", out var lastName);
            return JoinName(firstName, lastName);
        }
    }
}
