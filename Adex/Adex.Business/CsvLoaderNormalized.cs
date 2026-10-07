// <copyright file="CsvLoaderNormalized.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Adex.Common;
using Adex.Data.Model;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Adex.Business
{
    public partial class CsvLoaderNormalized : IDisposable, ICsvLoader, ILinkSearchService
    {
        private CultureInfo _cultureFr = CultureInfo.CreateSpecificCulture("fr-FR");
        private System.Timers.Timer _timer = null;
        private Dictionary<string, Company> _companies = null;
        private Dictionary<string, Person> _beneficiaries = null;
        private Dictionary<string, Link> _links = null;
        private HashSet<string> _existingReferences = null;
        private Dictionary<string, Company> _existingCompanies = null;
        private Dictionary<string, Person> _existingBeneficiaries = null;
        private Dictionary<string, Entity> _existingOtherEntities = null;
        private readonly Dictionary<string, Dictionary<string, string>> _pendingAttributes = new();
        private bool disposedValue = false;
        private int _mainCounter = 0;
        private readonly IDbContextFactory<AdexContext> _contextFactory;

        public event EventHandler<MessageEventArgs> OnMessage;

        private CsvConfiguration CreateConfiguration(string delimiter = ",")
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter,
                MissingFieldFound = args =>
                    OnMessage?.Invoke(
                        this,
                        new MessageEventArgs
                        {
                            Message =
                                $"Missing field found at index {args.Index}: \"{string.Join(",", args.HeaderNames ?? Array.Empty<string>())}\"",
                            Level = Level.Error,
                        }
                    ),
                BadDataFound = args =>
                    OnMessage?.Invoke(
                        this,
                        new MessageEventArgs { Message = args.RawRecord, Level = Level.Error }
                    ),
                HasHeaderRecord = true,
                Encoding = Encoding.UTF8,
            };
        }

        public CsvLoaderNormalized(IDbContextFactory<AdexContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _timer = new System.Timers.Timer(10000);
            _timer.Elapsed += delegate(object sender, ElapsedEventArgs e)
            {
                OnMessage?.Invoke(
                    this,
                    new MessageEventArgs { Message = _mainCounter.ToString().PadLeft(10, '0') }
                );
            };
            _timer.Enabled = true;
            _timer.Start();

            _companies = new Dictionary<string, Company>();
            _beneficiaries = new Dictionary<string, Person>();
            _links = new Dictionary<string, Link>();
        }

        ~CsvLoaderNormalized()
        {
            Dispose(false);
        }

        public async Task LoadReferencesAsync(CancellationToken cancellationToken)
        {
            await using (var db = await _contextFactory.CreateDbContextAsync(cancellationToken))
            {
                _existingReferences = new HashSet<string>(
                    await db.Entities.Select(x => x.Reference).ToListAsync(cancellationToken)
                );
                _existingCompanies = await db.Companies
                    .AsNoTracking()
                    .ToDictionaryAsync(
                        company => company.Reference,
                        company => new Company
                        {
                            Id = company.Id,
                            Reference = company.Reference,
                            Designation = company.Designation,
                        },
                        cancellationToken
                    );
                _existingBeneficiaries = await db.Persons
                    .AsNoTracking()
                    .ToDictionaryAsync(
                        person => person.Reference,
                        person => new Person
                        {
                            Id = person.Id,
                            Reference = person.Reference,
                            FirstName = person.FirstName,
                            LastName = person.LastName,
                        },
                        cancellationToken
                    );
                _existingOtherEntities = await db.Entities
                    .AsNoTracking()
                    .Where(entity =>
                        !db.Companies.Any(company => company.Id == entity.Id)
                        && !db.Persons.Any(person => person.Id == entity.Id)
                    )
                    .ToDictionaryAsync(
                        entity => entity.Reference,
                        entity => new Entity { Id = entity.Id, Reference = entity.Reference },
                        cancellationToken
                    );
            }
        }

        public async Task LoadProvidersAsync(string path, CancellationToken cancellationToken)
        {
            OnMessage?.Invoke(
                this,
                new MessageEventArgs { Message = $"Processing \"{path}\" file" }
            );

            int counter = 0;
            using (var sr = new StreamReader(path, true))
            {
                using (var csv = new CustomCsvReader(sr, CreateConfiguration()))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await csv.ReadAsync();
                    csv.ReadHeader();

                    while (await csv.ReadAsync())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var externalId = csv.GetField("identifiant");
                        if (_companies.TryGetValue(externalId, out var company))
                        {
                            AddAttributes(company, csv);
                        }
                        else if (_existingCompanies.TryGetValue(externalId, out company))
                        {
                            AddAttributes(company, csv);
                        }
                        else if (_existingOtherEntities.TryGetValue(externalId, out var entity))
                        {
                            AddAttributes(entity, csv);
                        }
                        else
                        {
                            company = new Company
                            {
                                Reference = externalId,
                                Designation = csv.GetField("denomination_sociale"),
                            };
                            AddAttributes(company, csv);
                            _companies.Add(externalId, company);
                            _existingReferences.Add(externalId);
                        }
                        counter++;
                    }
                }
            }

            OnMessage?.Invoke(
                this,
                new MessageEventArgs
                {
                    Message = $"Found {counter} records in file \"{path}\"",
                    Level = Level.Debug,
                }
            );
            OnMessage?.Invoke(
                this,
                new MessageEventArgs
                {
                    Message = $"There are {_companies.Count} new companies",
                    Level = Level.Debug,
                }
            );
        }

        public async Task LoadLinksAsync(string path, CancellationToken cancellationToken)
        {
            OnMessage?.Invoke(
                this,
                new MessageEventArgs { Message = $"Processing file \"{path}\"" }
            );

            int records = 0;
            int counterCompanies = 0;
            int counterBenef = 0;
            int counterBonds = 0;

            using (var sr = new StreamReader(path, true))
            {
                using (var csv = new CustomCsvReader(sr, CreateConfiguration(";")))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await csv.ReadAsync();
                    csv.ReadHeader();

                    var idx_entreprise_identifiant = csv.GetFieldIndex("entreprise_identifiant");
                    var idx_denomination_sociale = csv.GetFieldIndex("denomination_sociale");

                    var idx_benef_identifiant_valeur = csv.GetFieldIndex(
                        "benef_identifiant_valeur"
                    );
                    var idx_benef_nom = csv.GetFieldIndex("benef_nom");
                    var idx_benef_prenom = csv.GetFieldIndex("benef_prenom");

                    var idx_ligne_identifiant = csv.GetFieldIndex("ligne_identifiant");

                    while (await csv.ReadAsync())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            var date = csv.GetField(
                                    new string[]
                                    {
                                        "avant_date_signature",
                                        "conv_date_signature",
                                        "remu_date",
                                    }
                                )
                                ?.Trim();

                            var dateSignature = Convert.ToDateTime(date, _cultureFr);

                            if (
                                dateSignature.Year == 2019
                                && !string.IsNullOrEmpty(csv.GetField(idx_benef_identifiant_valeur))
                            )
                            {
                                Company company;
                                Person benef;

                                var externalId = csv.GetField(idx_entreprise_identifiant);
                                if (!_companies.TryGetValue(externalId, out company))
                                {
                                    if (!_existingCompanies.TryGetValue(externalId, out company))
                                    {
                                        if (_existingOtherEntities.TryGetValue(externalId, out var existingCompanyEntity))
                                        {
                                            company = new Company
                                            {
                                                Id = existingCompanyEntity.Id,
                                                Reference = existingCompanyEntity.Reference,
                                            };
                                        }
                                        else
                                        {
                                            company = new Company
                                            {
                                                Reference = externalId,
                                                Designation = csv.GetField(idx_denomination_sociale),
                                            };
                                            _companies.Add(company.Reference, company);
                                            _existingReferences.Add(externalId);
                                            counterCompanies++;
                                        }
                                    }
                                }
                                AddAttributes(company, csv);

                                externalId = csv.GetField(idx_benef_identifiant_valeur)?.Trim();
                                if (!_beneficiaries.TryGetValue(externalId, out benef))
                                {
                                    if (!_existingBeneficiaries.TryGetValue(externalId, out benef))
                                    {
                                        if (_existingOtherEntities.TryGetValue(externalId, out var existingBeneficiaryEntity))
                                        {
                                            benef = new Person
                                            {
                                                Id = existingBeneficiaryEntity.Id,
                                                Reference = existingBeneficiaryEntity.Reference,
                                            };
                                        }
                                        else
                                        {
                                            var lastName = csv.GetField(idx_benef_nom)?.Trim();
                                            var firstName = csv.GetField(idx_benef_prenom)?.Trim();

                                            benef = new Person
                                            {
                                                Reference = externalId,
                                                FirstName = firstName,
                                                LastName = lastName,
                                            };
                                            _beneficiaries.Add(benef.Reference, benef);
                                            _existingReferences.Add(externalId);
                                            counterBenef++;
                                        }
                                    }
                                }
                                AddAttributes(benef, csv);

                                externalId = csv.GetField(idx_ligne_identifiant).Trim();
                                if (!_existingReferences.Any(x => x == externalId))
                                {
                                    // remu_convention_liee
                                    var amount = csv.GetField(
                                            new string[]
                                            {
                                                "avant_montant_ttc",
                                                "conv_montant_ttc",
                                                "remu_montant_ttc",
                                            }
                                        )
                                        ?.Trim();
                                    var kind = csv.GetField(
                                            new string[] { "avant_nature", "conv_objet" }
                                        )
                                        ?.Trim();

                                    var link = new FinancialLink
                                    {
                                        Reference = externalId,
                                        Amount = Convert.ToDecimal(amount, _cultureFr),
                                        Kind = kind,
                                        DeclarationType = GetDeclarationType(csv),
                                        Date = Convert.ToDateTime(date, _cultureFr),
                                        From = company.Id == 0 ? company : null,
                                        From_Id = company.Id,
                                        To = benef.Id == 0 ? benef : null,
                                        To_Id = benef.Id,
                                    };
                                    AddAttributes(link, csv);
                                    _links.Add(link.Reference, link);
                                    _existingReferences.Add(externalId);
                                    counterBonds++;
                                }
                                else
                                {
                                    AddPendingAttributes(externalId, csv);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            OnMessage?.Invoke(
                                this,
                                new MessageEventArgs
                                {
                                    Message = $"\"{path}\" {e.Message}: {csv.Context.Parser.RawRecord}",
                                }
                            );
                        }

                        _mainCounter++;
                        records++;
                    }
                }
            }

            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Read {records} records" });
            OnMessage?.Invoke(
                this,
                new MessageEventArgs
                {
                    Message =
                        $"Added {counterCompanies} companies, {counterBenef} beneficiaries, {counterBonds} insterest bonds",
                }
            );
            OnMessage?.Invoke(
                this,
                new MessageEventArgs
                {
                    Message =
                        $"Total {_companies.Count} companies, {_beneficiaries.Count} beneficiaries, {_links.Count} insterest links",
                }
            );
        }

        public async Task SaveAsync(CancellationToken cancellationToken)
        {
            await using (var db = await _contextFactory.CreateDbContextAsync(cancellationToken))
            {
                await using (var t = await db.Database.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        db.Companies.AddRange(_companies.Select(x => x.Value));
                        db.Persons.AddRange(_beneficiaries.Select(x => x.Value));
                        db.Links.AddRange(_links.Select(x => x.Value));
                        await db.SaveChangesAsync(cancellationToken);
                        await PersistPendingAttributesAsync(db, cancellationToken);
                        await t.CommitAsync(cancellationToken);

                        OnMessage?.Invoke(
                            this,
                            new MessageEventArgs
                            {
                                Message = $"{_companies.Count()} new companies have been saved",
                                Level = Level.Info,
                            }
                        );
                        OnMessage?.Invoke(
                            this,
                            new MessageEventArgs
                            {
                                Message =
                                    $"{_beneficiaries.Count()} new beneficiaries have been saved",
                                Level = Level.Info,
                            }
                        );
                        OnMessage?.Invoke(
                            this,
                            new MessageEventArgs
                            {
                                Message = $"{_links.Count()} new links have been saved",
                                Level = Level.Info,
                            }
                        );
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        await t.RollbackAsync(CancellationToken.None);
                        throw;
                    }
                    catch (Exception e)
                    {
                        OnMessage?.Invoke(
                            this,
                            new MessageEventArgs
                            {
                                Message = e.GetFullErrorMessage(),
                                Level = Level.Error,
                            }
                        );
                        await t.RollbackAsync(CancellationToken.None);
                        throw;
                    }
                }
            }

            _companies.Clear();
            _beneficiaries.Clear();
            _links.Clear();
            _pendingAttributes.Clear();
        }

        private void AddAttributes(Entity entity, CustomCsvReader csv)
        {
            if (entity.Id != 0)
            {
                AddPendingAttributes(entity.Reference, csv);
                return;
            }

            foreach (var name in csv.HeaderRecord ?? Array.Empty<string>())
            {
                var value = csv.GetField(name);
                if (
                    !string.IsNullOrWhiteSpace(value)
                    && !entity.Attributes.Any(attribute => attribute.Name == name)
                )
                {
                    entity.Attributes.Add(new EntityAttribute { Name = name, Value = value });
                }
            }
        }

        private void AddPendingAttributes(string reference, CustomCsvReader csv)
        {
            if (!_pendingAttributes.TryGetValue(reference, out var attributes))
            {
                attributes = new Dictionary<string, string>();
                _pendingAttributes.Add(reference, attributes);
            }

            foreach (var name in csv.HeaderRecord ?? Array.Empty<string>())
            {
                var value = csv.GetField(name);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    attributes[name] = value;
                }
            }
        }

        private async Task PersistPendingAttributesAsync(
            AdexContext db,
            CancellationToken cancellationToken
        )
        {
            if (_pendingAttributes.Count == 0)
            {
                return;
            }

            var references = _pendingAttributes.Keys.ToList();
            var entityIds = await db.Entities
                .Where(entity => references.Contains(entity.Reference))
                .ToDictionaryAsync(entity => entity.Reference, entity => entity.Id, cancellationToken);

            var ids = entityIds.Values.ToList();
            var existingAttributes = await db.EntityAttributes
                .Where(attribute => ids.Contains(attribute.EntityId))
                .ToDictionaryAsync(
                    attribute => (attribute.EntityId, attribute.Name),
                    cancellationToken
                );

            foreach (var entityAttributes in _pendingAttributes)
            {
                if (!entityIds.TryGetValue(entityAttributes.Key, out var entityId))
                {
                    throw new InvalidOperationException(
                        $"Cannot attach imported attributes to unknown entity '{entityAttributes.Key}'."
                    );
                }

                foreach (var item in entityAttributes.Value)
                {
                    var key = (entityId, item.Key);
                    if (existingAttributes.TryGetValue(key, out var existing))
                    {
                        existing.Value = item.Value;
                    }
                    else
                    {
                        var attribute = new EntityAttribute
                        {
                            EntityId = entityId,
                            Name = item.Key,
                            Value = item.Value,
                        };
                        db.EntityAttributes.Add(attribute);
                        existingAttributes.Add(key, attribute);
                    }
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private static string GetDeclarationType(CustomCsvReader csv)
        {
            if (csv.GetFieldIndex("avant_date_signature") >= 0)
            {
                return "Avantage";
            }

            if (csv.GetFieldIndex("conv_date_signature") >= 0)
            {
                return "Convention";
            }

            if (csv.GetFieldIndex("remu_date") >= 0)
            {
                return "Rémunération";
            }

            return "Non renseigné";
        }

        public async Task<GraphDataSet> LinksToJsonAsync(
            string txt,
            int take,
            CancellationToken cancellationToken
        )
        {
            var retour = new GraphDataSet();

            var links = new List<Link>();
            await using (var db = await _contextFactory.CreateDbContextAsync(cancellationToken))
            {
                if (!string.IsNullOrEmpty(txt))
                {
                    if (await db.Entities.AnyAsync(x => x.Reference.Contains(txt), cancellationToken))
                    {
                        links.AddRange(
                            await db.Links.Include(link => link.From)
                                .Include(link => link.To)
                                .Where(x => x.From.Reference.Contains(txt))
                                .ToListAsync(cancellationToken)
                        );
                        links.AddRange(
                            await db.Links.Include(link => link.From)
                                .Include(link => link.To)
                                .Where(x => x.To.Reference.Contains(txt))
                                .ToListAsync(cancellationToken)
                        );
                    }
                }
                else
                {
                    links = await db.Links.Include(link => link.From)
                        .Include(link => link.To)
                        .ToListAsync(cancellationToken);
                }
            }
            var all = links
                .Select(x => new { id = x.From.Reference, name = x.From.Reference })
                .Distinct()
                .ToList();
            all.AddRange(
                links.Select(x => new { id = x.To.Reference, name = x.To.Reference }).Distinct()
            );

            foreach (
                var item in all.Where(x => !string.IsNullOrEmpty(x.id)).Take(take)
            )
            {
                var temp = links
                    .Where(x => x.From.Reference == item.id)
                    .Where(x => !string.IsNullOrEmpty(x.To.Reference))
                    .Select(x => x.To.Reference);
                retour.BundlingItems.Add(
                    new EdgeBundlingItem
                    {
                        Name = item.id,
                        Size = temp.Distinct().Count(),
                        Imports = temp.Distinct().ToList(),
                    }
                );
            }

            return retour;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _companies?.Clear();
                    _beneficiaries?.Clear();
                    _links?.Clear();
                    _companies = null;
                    _beneficiaries = null;
                    _links = null;

                    _cultureFr = null;
                }

                _timer?.Dispose();
                _timer = null;

                disposedValue = true;
            }
        }

    }
}
