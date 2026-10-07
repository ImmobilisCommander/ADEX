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
        private const int FinancialLinkLimit = 100;

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

            var companiesCount = await db.Companies.CountAsync(cancellationToken);
            var personsCount = await db.Persons.CountAsync(cancellationToken);

            var linkTypes = await db.FinancialLinks
                .GroupBy(link => link.DeclarationType ?? "Non renseigné")
                .Select(group => new FinancialLinkTypeSummary
                {
                    Type = group.Key,
                    Count = group.Count(),
                    Amount = group.Sum(link => link.Amount),
                })
                .OrderByDescending(summary => summary.Amount)
                .ToListAsync(cancellationToken);

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

            return new DashboardModel
            {
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
            var attributes = await db.EntityAttributes
                .Where(attribute => attribute.EntityId == entityId)
                .OrderBy(attribute => attribute.Name)
                .Select(attribute => new EntityAttributeDetails
                {
                    Name = attribute.Name,
                    Value = attribute.Value,
                })
                .ToListAsync(cancellationToken);
            var name = company?.Designation
                ?? (person is null
                    ? FindAttributeName(attributes)
                    : JoinName(person.FirstName, person.LastName));

            var totals = await db.EntityTotals
                .Where(total => total.EntityId == entityId)
                .SingleOrDefaultAsync(cancellationToken);

            // One index-friendly query per direction instead of an OR over both columns.
            var outgoingLinks = await LoadLinksAsync(db, entityId, outgoing: true, cancellationToken);
            var incomingLinks = await LoadLinksAsync(db, entityId, outgoing: false, cancellationToken);
            var links = outgoingLinks
                .Concat(incomingLinks)
                .OrderByDescending(link => link.Date)
                .ThenBy(link => link.Id)
                .Take(FinancialLinkLimit + 1)
                .ToList();

            var linksTruncated = links.Count > FinancialLinkLimit;
            if (linksTruncated)
            {
                links.RemoveAt(links.Count - 1);
            }

            var linkIds = links.Select(link => link.Id).ToList();
            var justificationRows = await db.EntityAttributes
                .Where(attribute => linkIds.Contains(attribute.EntityId) && JustificationNames.Contains(attribute.Name))
                .Select(attribute => new { attribute.EntityId, attribute.Name, attribute.Value })
                .ToListAsync(cancellationToken);
            var justifications = justificationRows
                .GroupBy(row => row.EntityId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(row => Array.IndexOf(JustificationNames, row.Name))
                        .Select(row => new EntityAttributeDetails { Name = row.Name, Value = row.Value })
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

            return new EntityDetails
            {
                Id = entity.Id,
                Name = string.IsNullOrWhiteSpace(name) ? "(sans nom)" : name,
                Type = company is not null
                    ? "Entreprise"
                    : person is not null ? "Bénéficiaire" : "Entité",
                OutgoingAmount = totals?.OutgoingAmount ?? 0m,
                IncomingAmount = totals?.IncomingAmount ?? 0m,
                FinancialLinkCount = totals?.LinkCount ?? 0,
                FinancialLinksTruncated = linksTruncated,
                Attributes = attributes,
                FinancialLinks = links,
            };
        }

        private static Task<List<FinancialLinkDetails>> LoadLinksAsync(
            AdexContext db,
            Guid entityId,
            bool outgoing,
            CancellationToken cancellationToken
        )
        {
            var links = outgoing
                ? db.FinancialLinks.Where(link => link.From_Id == entityId)
                : db.FinancialLinks.Where(link => link.To_Id == entityId);

            return links
                .OrderByDescending(link => link.Date)
                .Select(link => new FinancialLinkDetails
                {
                    Id = link.Id,
                    CounterpartyId = outgoing ? link.To_Id : link.From_Id,
                    DeclarationType = link.DeclarationType ?? "Non renseigné",
                    Kind = link.Kind ?? string.Empty,
                    Date = link.Date,
                    Amount = link.Amount,
                    Outgoing = outgoing,
                })
                .Take(FinancialLinkLimit + 1)
                .ToListAsync(cancellationToken);
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
