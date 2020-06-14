// <copyright file="CvsLoaderMetadata.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using Adex.Common;
using Adex.Data.MetaModel;
using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Timers;

namespace Adex.Business
{
    internal class NodeComparer : IEqualityComparer<ForceDirectedNodeItem>
    {
        public bool Equals([AllowNull] ForceDirectedNodeItem x, [AllowNull] ForceDirectedNodeItem y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] ForceDirectedNodeItem obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    public class CvsLoaderMetadata : IDisposable, ICsvLoader
    {
        public event EventHandler<MessageEventArgs> OnMessage;

        private CsvConfiguration _configuration = null;
        private CultureInfo _cultureFr = CultureInfo.CreateSpecificCulture("fr-FR");

        private Dictionary<string, Entity> _existingReferences = null;
        private Dictionary<string, Member> _existingMembers = null;
        private int _mainCounter = 0;
        private bool _disposedValue;

        public string DbConnectionString { get; set; }

        public CvsLoaderMetadata()
        {
            _configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = delegate (string[] tab, int count, ReadingContext ctxt)
                {
                    OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Missing field found at index {count}: \"{tab[count]}\"", Level = Level.Error });
                },
                BadDataFound = delegate (ReadingContext ctxt)
                {
                    OnMessage?.Invoke(this, new MessageEventArgs { Message = ctxt.RawRecord, Level = Level.Error });
                },
                HasHeaderRecord = true,
                Encoding = Encoding.UTF8
            };
        }

        ~CvsLoaderMetadata()
        {
            Dispose(disposing: false);
        }

        public void LoadReferences()
        {
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Loading reference data" });

            using (var con = new SqlConnection(DbConnectionString))
            {
                con.Open();
                _existingReferences = con.Query<Entity>("select * from Entities").ToDictionary(x => x.Reference);
                _existingMembers = con.Query<Member>("select * from Members").ToDictionary(x => x.Name);
            }

            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Reference data loaded" });
        }

        public void LoadProviders(string path)
        {
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Processing \"{path}\" file" });

            int counter = 0;
            using (var sr = new StreamReader(path, true))
            {
                using (var csv = new CustomCsvReader(sr, _configuration))
                {
                    csv.Configuration.Delimiter = ",";
                    csv.Read();
                    csv.ReadHeader();

                    using (var con = new SqlConnection(DbConnectionString))
                    {
                        con.Open();

                        #region AJOUT DES EN-TÊTES MANQUANTS
                        var header = csv.Context.HeaderRecord;
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

                        while (csv.Read())
                        {
                            var externalId = csv.GetField("identifiant");
                            if (!_existingReferences.ContainsKey(externalId))
                            {
                                var entity = new Entity()
                                {
                                    Reference = externalId
                                };
                                entity.Id = con.InsertEntity(entity);

                                for (int i = 0; i < csv.Context.HeaderRecord.Length; i++)
                                {
                                    var col = csv.Context.HeaderRecord[i];
                                    var id = con.InsertMetadata(entity.Id, _existingMembers[col].Id, csv[i].ToString());
                                }

                                _existingReferences.Add(entity.Reference, entity);
                                counter++;
                            }

                            _mainCounter++;
                        }
                    }
                }
            }

            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Found {_mainCounter} records in file \"{path}\"", Level = Level.Debug });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"There are {counter} new companies", Level = Level.Debug });
        }

        public void LoadLinks(string path)
        {
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Processing file \"{path}\"" });

            int records = 0;
            int counterCompanies = 0;
            int counterBenef = 0;
            int counterBonds = 0;

            using (var sr = new StreamReader(path, true))
            {
                var header = sr.ReadLine().Split(';');

                var idx_entreprise_identifiant = header.GetFieldIndex(CsvColumnsName.EntrepriseIdentifiant);
                var idx_denomination_sociale = header.GetFieldIndex(CsvColumnsName.DenominationSociale);

                var idx_benef_nom = header.GetFieldIndex(CsvColumnsName.BenefNom);
                var idx_benef_prenom = header.GetFieldIndex(CsvColumnsName.BenefPrenom);
                var idx_benef_identifiant_valeur = header.GetFieldIndex(CsvColumnsName.BenefIdentifiantValeur);
                var idx_benef_denomination_sociale = header.GetFieldIndex(CsvColumnsName.BenefDenominationSociale);
                var idx_benef_categorie_code = header.GetFieldIndex(CsvColumnsName.BenefCategorieCode);
                var idx_benef_qualite_code = header.GetFieldIndex(CsvColumnsName.BenefQualiteCode);
                var idx_benef_specialite_code = header.GetFieldIndex(CsvColumnsName.BenefSpecialiteCode);
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

                using (var con = new SqlConnection(DbConnectionString))
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

                                if (DateTime.TryParse(date, _cultureFr, DateTimeStyles.None, out dateSignature))
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
                                                var denomination = csv.TryGetValue(idx_denomination_sociale);
                                                company = new Entity()
                                                {
                                                    Reference = externalId
                                                };
                                                company.Id = con.InsertEntity(company);

                                                con.InsertMetadata(company.Id, _existingMembers[header[idx_entreprise_identifiant]].Id, externalId);
                                                con.InsertMetadata(company.Id, _existingMembers[header[idx_denomination_sociale]].Id, denomination);

                                                _existingReferences.Add(company.Reference, company);
                                                counterCompanies++;
                                            }
                                            else
                                            {
                                                company = _existingReferences[externalId];
                                            }

                                            externalId = csv[idx_benef_identifiant_valeur].Trim();
                                            if (string.IsNullOrEmpty(externalId) || "-;[0];[autre];[br];[so];n/a;na;nc;non renseigne;non renseigné;so;infirmier".Contains(externalId.ToLowerInvariant()))
                                            {
                                                externalId = csv.GetHashCodeBenef();
                                            }
                                            Entity benef = null;
                                            if (!_existingReferences.ContainsKey(externalId))
                                            {
                                                benef = new Entity()
                                                {
                                                    Reference = externalId
                                                };
                                                benef.Id = con.InsertEntity(benef);

                                                if (!"[etu][prs][vet]".Contains(csv[idx_benef_categorie_code]?.Trim().ToLowerInvariant()))
                                                {
                                                    var brandName = csv[idx_benef_denomination_sociale]?.Trim();
                                                    con.InsertMetadata(benef.Id, _existingMembers[header[idx_benef_categorie_code]].Id, brandName);
                                                }
                                                else
                                                {
                                                    var lastName = csv[idx_benef_nom]?.Trim();
                                                    var firstName = csv[idx_benef_prenom]?.Trim();

                                                    con.InsertMetadata(benef.Id, _existingMembers[header[idx_benef_prenom]].Id, firstName);
                                                    con.InsertMetadata(benef.Id, _existingMembers[header[idx_benef_nom]].Id, lastName);
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
                                            var idx_amount = header.GetFieldIndex(CsvColumnsName.AvantMontantTtc);
                                            var headerAmountName = CsvColumnsName.AvantMontantTtc;
                                            var idx_kind = header.GetFieldIndex(CsvColumnsName.AvantNature);
                                            if (idx_amount < 0)
                                            {
                                                idx_amount = header.GetFieldIndex(CsvColumnsName.ConvMontantTtc);
                                                headerAmountName = CsvColumnsName.ConvMontantTtc;
                                                idx_kind = header.GetFieldIndex(CsvColumnsName.AvantNature);
                                            }
                                            if (idx_amount < 0)
                                            {
                                                idx_amount = header.GetFieldIndex(CsvColumnsName.RemuMontantTtc);
                                                headerAmountName = CsvColumnsName.RemuMontantTtc;
                                            }

                                            var amount = csv[idx_amount]?.Trim();
                                            var kind = csv.TryGetValue(idx_kind)?.Trim();
                                            var entity = new Entity()
                                            {
                                                Reference = externalIdLink
                                            };
                                            entity.Id = con.InsertEntity(entity);
                                            _existingReferences.Add(entity.Reference, entity);

                                            var link = new Link
                                            {
                                                Id = entity.Id,
                                                //Amount = Convert.ToDecimal(amount, _cultureFr),
                                                Kind = kind,
                                                Date = Convert.ToDateTime(date, _cultureFr),
                                                From = company,
                                                To = benef,
                                            };
                                            con.InsertLink(link);
                                            con.InsertMetadata(link.Id, _existingMembers[headerAmountName].Id, amount);

                                            counterBonds++;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"{records} \"{path}\" {e.Message}: {line}" });
                        }

                        _mainCounter++;
                        records++;

                        if (records % 10000 == 0)
                        {
                            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"{records}: Added {counterCompanies} companies, {counterBenef} beneficiaries, {counterBonds} insterest bonds" });
                        }
                    }
                }
            }

            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Read {records} records" });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Added {counterCompanies} companies, {counterBenef} beneficiaries, {counterBonds} insterest bonds" });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Total {_existingReferences.Count} entities" });
        }

        public GraphDataSet LinksToJson(string txt, int? take)
        {
            var retour = new GraphDataSet() { ForceDirectedData = new ForceDirectedData() };

            string query = @"select *
from
	(
	select a.Reference as Company, am.Value as Designation, b.Reference as Beneficiary, bm1.Value + ' ' + bm2.Value as SocialDenomination, count(*) as NumberOfLinks, SUM(CONVERT(decimal, lm.Value)) as Amount
	from
		Entities a
		inner join Metadatas am on am.Entity_Id = a.Id
		inner join Members amb on amb.Id = am.Member_Id and amb.Name = 'denomination_sociale'

		inner join Links l on l.From_Id = a.Id
		inner join Metadatas lm on lm.Entity_Id = l.Id
		inner join Members lmb on lmb.Id = lm.Member_Id and lmb.Name like '%_montant_ttc'

		inner join Entities b on b.Id = l.To_Id
		inner join Metadatas bm1 on bm1.Entity_Id = b.Id
		inner join Members bmb1 on bmb1.Id = bm1.Member_Id and bmb1.Name = 'benef_nom'
		inner join Metadatas bm2 on bm2.Entity_Id = b.Id
		inner join Members bmb2 on bmb2.Id = bm2.Member_Id and bmb2.Name = 'benef_prenom'
	Group by
		a.Reference, am.Value, b.Reference, bm1.Value, bm2.Value
	Having
		count(*) > 10

	union all

	select a.Reference, am.Value, b.Reference, bm1.Value, count(*), SUM(CONVERT(decimal, lm.Value))
	from
		Entities a
		inner join Metadatas am on am.Entity_Id = a.Id
		inner join Members amb on amb.Id = am.Member_Id and amb.Name = 'denomination_sociale'

		inner join Links l on l.From_Id = a.Id
		inner join Metadatas lm on lm.Entity_Id = l.Id
		inner join Members lmb on lmb.Id = lm.Member_Id and lmb.Name like '%_montant_ttc'

		inner join Entities b on b.Id = l.To_Id
		inner join Metadatas bm1 on bm1.Entity_Id = b.Id
		inner join Members bmb1 on bmb1.Id = bm1.Member_Id and bmb1.Name = 'denomination_sociale'
	Group by
		a.Reference, am.Value, b.Reference, bm1.Value
	Having
		count(*) > 10	
	) as tbl
order by NumberOfLinks desc";

            IEnumerable<QueryResult> result = null;

            using (var con = new SqlConnection(DbConnectionString))
            {
                result = con.Query<QueryResult>(query);
            }

            retour.BundlingItems.AddRange(result.Select(x => x.Company).Distinct().Select(x => new EdgeBundlingItem { Name = x, Imports = new List<string>() }));
            retour.BundlingItems.AddRange(result.Select(x => x.Beneficiary).Distinct().Select(x => new EdgeBundlingItem { Name = x, Imports = new List<string>() }));

            foreach (var grp in result.GroupBy(x => new { company = x.Company, designation = x.Designation }))
            {
                retour.ForceDirectedData.ForceDirectedNodes.Add(new ForceDirectedNodeItem { Id = grp.Key.company, Name = grp.Key.designation, Amount = grp.Sum(s => s.Amount), Group = "1" });
            }

            foreach (var grp in result.GroupBy(x => new { benef = x.Beneficiary, denomination = x.SocialDenomination }))
            {
                retour.ForceDirectedData.ForceDirectedNodes.Add(new ForceDirectedNodeItem { Id = grp.Key.benef, Name = grp.Key.denomination, Amount = grp.Sum(s => s.Amount), Group = "2" });
            }

            retour.ForceDirectedData.ForceDirectedLinks.AddRange(result.Select(a => new ForceDirectedLinkItem
            {
                Source = a.Company,
                Target = a.Beneficiary,
                Size = Convert.ToInt32(a.NumberOfLinks) + Convert.ToInt32(result.Where(b => b.Company == a.Company).Select(c => c.Amount).Sum())
            }));

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
                    _configuration = null;
                    _cultureFr = null;
                }

                _disposedValue = true;
            }
        }
    }
}
