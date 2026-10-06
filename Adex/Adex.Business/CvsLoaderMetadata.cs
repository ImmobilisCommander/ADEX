// <copyright file="CvsLoaderMetadata.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using Adex.Common;
using Adex.Data.MetaModel;

using CsvHelper.Configuration;

using Dapper;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Adex.Business
{
    public class CvsLoaderMetadata : IDisposable, ICsvLoader, ILinkSearchService, IMetadataLookupService
    {
        public event EventHandler<MessageEventArgs> OnMessage;

        private CultureInfo _cultureFr = CultureInfo.CreateSpecificCulture("fr-FR");

        private Dictionary<string, Entity> _existingReferences = null;
        private Dictionary<string, Member> _existingMembers = null;
        private int _mainCounter = 0;
        private bool _disposedValue;

        private readonly string _dbConnectionString;

        public CvsLoaderMetadata(string dbConnectionString)
        {
            _dbConnectionString = string.IsNullOrWhiteSpace(dbConnectionString)
                ? throw new ArgumentException(
                    "A metadata database connection string is required.",
                    nameof(dbConnectionString)
                )
                : dbConnectionString;
        }

        private CsvConfiguration CreateConfiguration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
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

        ~CvsLoaderMetadata()
        {
            Dispose(disposing: false);
        }

        public void LoadReferences()
        {
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Loading reference data" });

            using (var con = new NpgsqlConnection(_dbConnectionString))
            {
                _existingReferences = con.Query<Entity>("select * from \"Entities\"")
                    .ToDictionary(x => x.Reference);
                _existingMembers = con.Query<Member>("select * from \"Members\"")
                    .ToDictionary(x => x.Name);
            }

            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Reference data loaded" });
        }

        public void LoadProviders(string path)
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
                    csv.Read();
                    csv.ReadHeader();

                    using (var con = new NpgsqlConnection(_dbConnectionString))
                    {
                        con.Open();

                        #region AJOUT DES EN-TÊTES MANQUANTS
                        var header = csv.HeaderRecord;
                        var membersToAdd = header
                            .Except(_existingMembers.Select(x => x.Key))
                            .ToList();
                        if (membersToAdd != null && membersToAdd.Any())
                        {
                            foreach (var m in membersToAdd)
                            {
                                var member = new Member { Name = m };
                                member.Id = con.InsertMember(member);
                                _existingMembers.Add(member.Name, member);
                            }
                        }
                        #endregion

                        while (csv.Read())
                        {
                            var externalId = csv.GetField("identifiant");
                            if (!_existingReferences.ContainsKey(externalId))
                            {
                                var entity = new Entity() { Reference = externalId };
                                entity.Id = con.InsertEntity(entity);

                                for (int i = 0; i < csv.HeaderRecord.Length; i++)
                                {
                                    var col = csv.HeaderRecord[i];
                                    var id = con.InsertMetadata(
                                        entity.Id,
                                        _existingMembers[col].Id,
                                        csv[i].ToString()
                                    );
                                }

                                _existingReferences.Add(entity.Reference, entity);
                                counter++;
                            }

                            _mainCounter++;
                        }
                    }
                }
            }

            OnMessage?.Invoke(
                this,
                new MessageEventArgs
                {
                    Message = $"Found {_mainCounter} records in file \"{path}\"",
                    Level = Level.Debug,
                }
            );
            OnMessage?.Invoke(
                this,
                new MessageEventArgs
                {
                    Message = $"There are {counter} new companies",
                    Level = Level.Debug,
                }
            );
        }

        public void LoadLinks(string path)
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
                var header = sr.ReadLine().Split(';');

                var idx_entreprise_identifiant = header.GetFieldIndex(
                    CsvColumnsName.EntrepriseIdentifiant
                );
                var idx_denomination_sociale = header.GetFieldIndex(
                    CsvColumnsName.DenominationSociale
                );

                var idx_benef_nom = header.GetFieldIndex(CsvColumnsName.BenefNom);
                var idx_benef_prenom = header.GetFieldIndex(CsvColumnsName.BenefPrenom);
                var idx_benef_identifiant_valeur = header.GetFieldIndex(
                    CsvColumnsName.BenefIdentifiantValeur
                );
                var idx_benef_denomination_sociale = header.GetFieldIndex(
                    CsvColumnsName.BenefDenominationSociale
                );
                var idx_benef_categorie_code = header.GetFieldIndex(
                    CsvColumnsName.BenefCategorieCode
                );
                var idx_benef_qualite_code = header.GetFieldIndex(CsvColumnsName.BenefQualiteCode);
                var idx_benef_specialite_code = header.GetFieldIndex(
                    CsvColumnsName.BenefSpecialiteCode
                );
                var idx_benef_titre_code = header.GetFieldIndex(CsvColumnsName.BenefTitreCode);

                var idx_ligne_identifiant = header.GetFieldIndex(CsvColumnsName.LigneIdentifiant);

                var idx_date_avantage = header.GetFieldIndex(CsvColumnsName.AvantDateSignature);
                if (idx_date_avantage < 0)
                {
                    idx_date_avantage = header.GetFieldIndex(CsvColumnsName.ConvDateSignature);
                }
                if (idx_date_avantage < 0)
                {
                    idx_date_avantage = header.GetFieldIndex(CsvColumnsName.RemuDate);
                }

                using (var con = new NpgsqlConnection(_dbConnectionString))
                {
                    con.Open();

                    #region AJOUT DES EN-TÊTES MANQUANTS
                    var membersToAdd = header.Except(_existingMembers.Select(x => x.Key)).ToList();

                    if (membersToAdd != null && membersToAdd.Any())
                    {
                        foreach (var m in membersToAdd)
                        {
                            var member = new Member { Name = m };
                            member.Id = con.InsertMember(member);
                            _existingMembers.Add(member.Name, member);
                        }
                    }
                    #endregion

                    string line = string.Empty;
                    while (!sr.EndOfStream)
                    {
                        try
                        {
                            line = sr.ReadLine();
                            var csv = line.Split(';');
                            if (csv.Length == header.Length)
                            {
                                var date = csv[idx_date_avantage]?.Trim();

                                var dateSignature = default(DateTime);

                                if (
                                    DateTime.TryParse(
                                        date,
                                        _cultureFr,
                                        DateTimeStyles.None,
                                        out dateSignature
                                    )
                                )
                                {
                                    if (dateSignature.Year == 2019)
                                    {
                                        var externalIdLink = csv[idx_ligne_identifiant].Trim();
                                        if (!_existingReferences.ContainsKey(externalIdLink))
                                        {
                                            Entity company = null;
                                            var externalId = csv[idx_entreprise_identifiant].Trim();
                                            if (!_existingReferences.ContainsKey(externalId))
                                            {
                                                var denomination = csv.TryGetValue(
                                                    idx_denomination_sociale
                                                );
                                                company = new Entity() { Reference = externalId };
                                                company.Id = con.InsertEntity(company);

                                                con.InsertMetadata(
                                                    company.Id,
                                                    _existingMembers[
                                                        header[idx_entreprise_identifiant]
                                                    ].Id,
                                                    externalId
                                                );
                                                con.InsertMetadata(
                                                    company.Id,
                                                    _existingMembers[
                                                        header[idx_denomination_sociale]
                                                    ].Id,
                                                    denomination
                                                );

                                                _existingReferences.Add(company.Reference, company);
                                                counterCompanies++;
                                            }
                                            else
                                            {
                                                company = _existingReferences[externalId];
                                            }

                                            externalId = csv[idx_benef_identifiant_valeur].Trim();
                                            if (
                                                string.IsNullOrEmpty(externalId)
                                                || "-;[0];[autre];[br];[so];n/a;na;nc;non renseigne;non renseigné;so;infirmier".Contains(
                                                    externalId.ToLowerInvariant()
                                                )
                                            )
                                            {
                                                externalId = csv.GetHashCodeBenef();
                                            }
                                            Entity benef = null;
                                            if (!_existingReferences.ContainsKey(externalId))
                                            {
                                                benef = new Entity() { Reference = externalId };
                                                benef.Id = con.InsertEntity(benef);

                                                if (
                                                    !"[etu][prs][vet]".Contains(
                                                        csv[idx_benef_categorie_code]
                                                            ?.Trim()
                                                            .ToLowerInvariant()
                                                    )
                                                )
                                                {
                                                    var brandName = csv[
                                                        idx_benef_denomination_sociale
                                                    ]
                                                        ?.Trim();
                                                    con.InsertMetadata(
                                                        benef.Id,
                                                        _existingMembers[
                                                            header[idx_benef_categorie_code]
                                                        ].Id,
                                                        brandName
                                                    );
                                                }
                                                else
                                                {
                                                    var lastName = csv[idx_benef_nom]?.Trim();
                                                    var firstName = csv[idx_benef_prenom]?.Trim();

                                                    con.InsertMetadata(
                                                        benef.Id,
                                                        _existingMembers[
                                                            header[idx_benef_prenom]
                                                        ].Id,
                                                        firstName
                                                    );
                                                    con.InsertMetadata(
                                                        benef.Id,
                                                        _existingMembers[header[idx_benef_nom]].Id,
                                                        lastName
                                                    );
                                                }

                                                _existingReferences.Add(benef.Reference, benef);
                                                counterBenef++;
                                            }
                                            else
                                            {
                                                benef = _existingReferences[externalId];
                                            }

                                            // remu_convention_liee
                                            // TODO not very nice for the moment. Should call a distinct parser for links depending on the datasource: declaration_avantage, declaration_remuneration, declaration_convention
                                            var idx_amount = header.GetFieldIndex(
                                                CsvColumnsName.AvantMontantTtc
                                            );
                                            var headerAmountName = CsvColumnsName.AvantMontantTtc;
                                            var idx_kind = header.GetFieldIndex(
                                                CsvColumnsName.AvantNature
                                            );
                                            if (idx_amount < 0)
                                            {
                                                idx_amount = header.GetFieldIndex(
                                                    CsvColumnsName.ConvMontantTtc
                                                );
                                                headerAmountName = CsvColumnsName.ConvMontantTtc;
                                                idx_kind = header.GetFieldIndex(
                                                    CsvColumnsName.AvantNature
                                                );
                                            }
                                            if (idx_amount < 0)
                                            {
                                                idx_amount = header.GetFieldIndex(
                                                    CsvColumnsName.RemuMontantTtc
                                                );
                                                headerAmountName = CsvColumnsName.RemuMontantTtc;
                                            }

                                            var amount = csv[idx_amount]?.Trim();
                                            var kind = csv.TryGetValue(idx_kind)?.Trim();
                                            var entity = new Entity()
                                            {
                                                Reference = externalIdLink,
                                            };
                                            entity.Id = con.InsertEntity(entity);
                                            _existingReferences.Add(entity.Reference, entity);

                                            var link = new Link
                                            {
                                                Id = entity.Id,
                                                Kind = kind,
                                                Date = Convert.ToDateTime(date, _cultureFr),
                                                From = company,
                                                To = benef,
                                            };
                                            con.InsertLink(link);
                                            con.InsertMetadata(
                                                link.Id,
                                                _existingMembers[headerAmountName].Id,
                                                Convert.ToDecimal(amount, _cultureFr).ToString()
                                            );

                                            counterBonds++;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            OnMessage?.Invoke(
                                this,
                                new MessageEventArgs
                                {
                                    Message = $"{records} \"{path}\" {e.Message}: {line}",
                                }
                            );
                        }

                        _mainCounter++;
                        records++;

                        if (records % 10000 == 0)
                        {
                            OnMessage?.Invoke(
                                this,
                                new MessageEventArgs
                                {
                                    Message =
                                        $"{records}: Added {counterCompanies} companies, {counterBenef} beneficiaries, {counterBonds} insterest bonds",
                                }
                            );
                        }
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
                new MessageEventArgs { Message = $"Total {_existingReferences.Count} entities" }
            );
        }

        public GraphDataSet LinksToJson(string txt, int? take)
        {
            var retour = new GraphDataSet() { ForceDirectedData = new ForceDirectedData() };

            string query =
                @"select a.""Reference"" as ""Company"", am.""Value"" as ""Designation"", p.""Reference"" as ""Beneficiary"", pm1.""Value"" || ' ' || pm2.""Value"" as ""SocialDenomination"", b.""NumberOfLinks"", b.""Amount""
from
	(
	select
		l.""From_Id"", l.""To_Id"", count(*) as ""NumberOfLinks"", SUM(lm.""Value""::numeric) as ""Amount""
	from
		""Links"" l
		inner join ""Metadatas"" lm on lm.""Entity_Id"" = l.""Id""
		inner join ""Members"" lmb on lmb.""Id"" = lm.""Member_Id""
	where
		lmb.""Name"" like '%_montant_ttc'
	group by
		l.""From_Id"", l.""To_Id""
	having
		SUM(lm.""Value""::numeric) > 30000
	) b

	inner join ""Entities"" a on a.""Id"" = b.""From_Id""
	inner join ""Metadatas"" am on am.""Entity_Id"" = a.""Id""
	inner join ""Members"" amb on amb.""Id"" = am.""Member_Id"" and amb.""Name"" = 'denomination_sociale'

	inner join ""Entities"" p on p.""Id"" = b.""To_Id""
	inner join ""Metadatas"" pm1 on pm1.""Entity_Id"" = p.""Id""
	inner join ""Members"" pmb1 on pmb1.""Id"" = pm1.""Member_Id"" and pmb1.""Name"" = 'benef_nom'
	inner join ""Metadatas"" pm2 on pm2.""Entity_Id"" = p.""Id""
	inner join ""Members"" pmb2 on pmb2.""Id"" = pm2.""Member_Id"" and pmb2.""Name"" = 'benef_prenom'";
            var result = new List<QueryResult>();

            using (var con = new NpgsqlConnection(_dbConnectionString))
            {
                con.Open();

                result = con.Query<QueryResult>(query, commandTimeout: 3600).ToList();
            }

            //retour.BundlingItems.AddRange(result.Select(x => x.Company).Distinct().Select(x => new EdgeBundlingItem { Name = x, Imports = new List<string>() }));
            //retour.BundlingItems.AddRange(result.Select(x => x.Beneficiary).Distinct().Select(x => new EdgeBundlingItem { Name = x, Imports = new List<string>() }));

            foreach (
                var grp in result.GroupBy(x => new
                {
                    company = x.Company,
                    designation = x.Designation,
                })
            )
            {
                retour.ForceDirectedData.ForceDirectedNodes.Add(
                    new ForceDirectedNodeItem
                    {
                        Id = grp.Key.company,
                        Name = grp.Key.designation,
                        Amount = grp.Sum(s => s.Amount),
                        Group = "1",
                    }
                );
            }

            foreach (
                var grp in result.GroupBy(x => new
                {
                    benef = x.Beneficiary,
                    denomination = x.SocialDenomination,
                })
            )
            {
                retour.ForceDirectedData.ForceDirectedNodes.Add(
                    new ForceDirectedNodeItem
                    {
                        Id = grp.Key.benef,
                        Name = grp.Key.denomination,
                        Amount = grp.Sum(s => s.Amount),
                        Group = "2",
                    }
                );
            }

            retour.ForceDirectedData.ForceDirectedLinks.AddRange(
                result.Select(a => new ForceDirectedLinkItem
                {
                    Source = a.Company,
                    Target = a.Beneficiary,
                    NbLinks = Convert.ToInt32(a.NumberOfLinks),
                    Amount = Convert.ToInt32(a.Amount),
                    Size = 1,
                })
            );

            return retour;
        }

        public Dictionary<string, string> GetBeneficiary(string reference)
        {
            var retour = new Dictionary<string, string>();

            var query =
                $@"select
	c.""Name"" as ""Key"", b.""Value""
from
	""Metadatas"" b
	inner join ""Members"" c on c.""Id"" = b.""Member_Id""
where
	b.""Entity_Id"" = (select ""Id"" from ""Entities"" where ""Reference"" = @reference)";

            using (var con = new NpgsqlConnection(_dbConnectionString))
            {
                var rows = con.Query<(string Key, string Value)>(
                    query,
                    new { reference },
                    commandTimeout: 3600
                );
                foreach (var row in rows)
                {
                    retour.Add(row.Key, row.Value);
                }
            }

            return retour;
        }

        public Dictionary<string, string> Search(string txt)
        {
            var retour = new Dictionary<string, string>();

            using (var con = new NpgsqlConnection(_dbConnectionString))
            {
                var entity = con.QuerySingleOrDefault<Entity>(
                    "select * from \"Entities\" where \"Reference\" = @reference",
                    new { reference = txt },
                    commandTimeout: 3600
                );
                if (entity is null)
                {
                    return retour;
                }

                var rows = con.Query<(string Key, string Value)>(
                    @"select m.""Name"" as ""Key"", d.""Value""
from ""Metadatas"" d
inner join ""Members"" m on m.""Id"" = d.""Member_Id""
where d.""Entity_Id"" = @entityId",
                    new { entityId = entity.Id },
                    commandTimeout: 3600
                );
                foreach (var row in rows)
                {
                    retour.Add(row.Key, row.Value);
                }
            }

            return retour;
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        internal class QueryResult
        {
            public string Company { get; set; }
            public string Designation { get; set; }
            public string Beneficiary { get; set; }
            public string SocialDenomination { get; set; }
            public int NumberOfLinks { get; set; }
            public decimal Amount { get; set; }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cultureFr = null;
                }

                _disposedValue = true;
            }
        }
    }
}
