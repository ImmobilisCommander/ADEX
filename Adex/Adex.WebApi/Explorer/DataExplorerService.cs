using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adex.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Adex.WebApi.Explorer
{
    public sealed class DataExplorerService
    {
        private const int SearchResultLimit = 20;
        private const int FinancialLinkLimit = 100;

        private readonly IDbContextFactory<AdexContext> _contextFactory;

        public DataExplorerService(IDbContextFactory<AdexContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<DashboardModel> GetDashboardAsync(CancellationToken cancellationToken)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var companiesCount = await db.Companies.CountAsync(cancellationToken);
            var personsCount = await db.Persons.CountAsync(cancellationToken);
            var financialLinkCount = await db.FinancialLinks.CountAsync(cancellationToken);
            var totalAmount =
                await db.FinancialLinks.SumAsync(
                    link => (decimal?)link.Amount,
                    cancellationToken
                ) ?? 0m;

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

            var endpointAmounts = db.FinancialLinks
                .Select(link => new { EntityId = link.From_Id, link.Amount })
                .Concat(
                    db.FinancialLinks.Select(link => new
                    {
                        EntityId = link.To_Id,
                        link.Amount,
                    })
                );
            var amountsByEntity = endpointAmounts
                .GroupBy(endpoint => endpoint.EntityId)
                .Select(group => new
                {
                    EntityId = group.Key,
                    Amount = group.Sum(endpoint => endpoint.Amount),
                });

            var companyTop = await (
                from company in db.Companies.AsNoTracking()
                join amount in amountsByEntity on company.Id equals amount.EntityId
                orderby amount.Amount descending
                select new TopEntity
                {
                    Reference = company.Reference,
                    Name = company.Designation ?? company.Reference,
                    Type = "Entreprise",
                    Amount = amount.Amount,
                }
            )
                .Take(10)
                .ToListAsync(cancellationToken);

            var personTop = await (
                from person in db.Persons.AsNoTracking()
                join amount in amountsByEntity on person.Id equals amount.EntityId
                orderby amount.Amount descending
                select new
                {
                    Reference = person.Reference,
                    person.FirstName,
                    person.LastName,
                    Amount = amount.Amount,
                }
            )
                .Take(10)
                .ToListAsync(cancellationToken);

            var topEntities = companyTop
                .Concat(
                    personTop.Select(person => new TopEntity
                    {
                        Reference = person.Reference,
                        Name = JoinName(person.FirstName, person.LastName),
                        Type = "Bénéficiaire",
                        Amount = person.Amount,
                    })
                )
                .OrderByDescending(entity => entity.Amount)
                .ThenBy(entity => entity.Name)
                .Take(10)
                .ToList();

            return new DashboardModel
            {
                EntityCount = companiesCount + personsCount,
                FinancialLinkCount = financialLinkCount,
                TotalAmount = totalAmount,
                EntityTypes = new List<EntityTypeCount>
                {
                    new() { Type = "Entreprise", Count = companiesCount },
                    new() { Type = "Bénéficiaire", Count = personsCount },
                },
                FinancialLinkTypes = linkTypes,
                TopEntities = topEntities,
            };
        }

        public async Task<List<EntitySearchResult>> SearchEntitiesAsync(
            string query,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<EntitySearchResult>();
            }

            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var pattern = $"%{EscapeLikePattern(query.Trim())}%";
            const string escapeCharacter = "\\";

            var companies = await db.Companies
                .AsNoTracking()
                .Where(company =>
                    EF.Functions.ILike(company.Reference, pattern, escapeCharacter)
                    || (
                        company.Designation != null
                        && EF.Functions.ILike(company.Designation, pattern, escapeCharacter)
                    )
                )
                .OrderBy(company => company.Designation)
                .Take(SearchResultLimit)
                .Select(company => new EntitySearchResult
                {
                    Reference = company.Reference,
                    Name = company.Designation ?? company.Reference,
                    Type = "Entreprise",
                })
                .ToListAsync(cancellationToken);

            var persons = await db.Persons
                .AsNoTracking()
                .Where(person =>
                    EF.Functions.ILike(person.Reference, pattern, escapeCharacter)
                    || (
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
                    Reference = person.Reference,
                    person.FirstName,
                    person.LastName,
                })
                .ToListAsync(cancellationToken);

            return companies
                .Concat(
                    persons.Select(person => new EntitySearchResult
                    {
                        Reference = person.Reference,
                        Name = JoinName(person.FirstName, person.LastName),
                        Type = "Bénéficiaire",
                    })
                )
                .OrderBy(result => result.Name)
                .ThenBy(result => result.Reference)
                .Take(SearchResultLimit)
                .ToList();
        }

        public async Task<EntityDetails> GetEntityAsync(
            string reference,
            CancellationToken cancellationToken
        )
        {
            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var company = await db.Companies
                .AsNoTracking()
                .SingleOrDefaultAsync(entity => entity.Reference == reference, cancellationToken);
            var person = company is null
                ? await db.Persons
                    .AsNoTracking()
                    .SingleOrDefaultAsync(entity => entity.Reference == reference, cancellationToken)
                : null;
            var otherEntity = company is null && person is null
                ? await db.Entities
                    .AsNoTracking()
                    .SingleOrDefaultAsync(entity => entity.Reference == reference, cancellationToken)
                : null;
            if (company is null && person is null && otherEntity is null)
            {
                return null;
            }

            var entity = (Entity)company ?? person ?? otherEntity;
            var attributes = await db.EntityAttributes
                .AsNoTracking()
                .Where(attribute => attribute.EntityId == entity.Id)
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
                    : string.Join(
                        " ",
                        new[] { person.FirstName, person.LastName }.Where(value =>
                            !string.IsNullOrWhiteSpace(value)
                        )
                    ));

            var financialLinkCount = await db.FinancialLinks.CountAsync(
                link => link.From_Id == entity.Id || link.To_Id == entity.Id,
                cancellationToken
            );
            var outgoingAmount =
                await db.FinancialLinks
                    .Where(link => link.From_Id == entity.Id)
                    .SumAsync(link => (decimal?)link.Amount, cancellationToken) ?? 0m;
            var incomingAmount =
                await db.FinancialLinks
                    .Where(link => link.To_Id == entity.Id)
                    .SumAsync(link => (decimal?)link.Amount, cancellationToken) ?? 0m;

            var links = await db.FinancialLinks
                .AsNoTracking()
                .Where(link => link.From_Id == entity.Id || link.To_Id == entity.Id)
                .OrderByDescending(link => link.Date)
                .ThenBy(link => link.Reference)
                .Select(link => new FinancialLinkDetails
                {
                    Reference = link.Reference,
                    CounterpartyReference = link.From_Id == entity.Id
                        ? link.To.Reference
                        : link.From.Reference,
                    DeclarationType = link.DeclarationType ?? "Non renseigné",
                    Kind = link.Kind ?? string.Empty,
                    Date = link.Date,
                    Amount = link.Amount,
                    Outgoing = link.From_Id == entity.Id,
                })
                .Take(FinancialLinkLimit + 1)
                .ToListAsync(cancellationToken);

            var linksTruncated = links.Count > FinancialLinkLimit;
            if (linksTruncated)
            {
                links.RemoveAt(links.Count - 1);
            }

            var counterparties = links
                .Select(link => link.CounterpartyReference)
                .Distinct()
                .ToList();
            var companyNames = await db.Companies
                .AsNoTracking()
                .Where(candidate => counterparties.Contains(candidate.Reference))
                .Select(candidate => new
                {
                    candidate.Reference,
                    Name = candidate.Designation ?? candidate.Reference,
                })
                .ToDictionaryAsync(candidate => candidate.Reference, candidate => candidate.Name, cancellationToken);
            var personNameRows = await db.Persons
                .AsNoTracking()
                .Where(candidate => counterparties.Contains(candidate.Reference))
                .Select(candidate => new
                {
                    candidate.Reference,
                    candidate.FirstName,
                    candidate.LastName,
                })
                .ToListAsync(cancellationToken);
            var personNames = personNameRows.ToDictionary(
                candidate => candidate.Reference,
                candidate => JoinName(candidate.FirstName, candidate.LastName)
            );

            foreach (var link in links)
            {
                if (companyNames.TryGetValue(link.CounterpartyReference, out var companyName))
                {
                    link.CounterpartyName = companyName;
                }
                else if (personNames.TryGetValue(link.CounterpartyReference, out var personName))
                {
                    link.CounterpartyName = personName;
                }
                else
                {
                    link.CounterpartyName = link.CounterpartyReference;
                }
            }

            return new EntityDetails
            {
                Reference = entity.Reference,
                Name = string.IsNullOrWhiteSpace(name) ? entity.Reference : name,
                Type = company is not null
                    ? "Entreprise"
                    : person is not null ? "Bénéficiaire" : "Entité",
                OutgoingAmount = outgoingAmount,
                IncomingAmount = incomingAmount,
                FinancialLinkCount = financialLinkCount,
                FinancialLinksTruncated = linksTruncated,
                Attributes = attributes,
                FinancialLinks = links,
            };
        }

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
