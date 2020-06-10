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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace Adex.Business
{
    public class CvsLoaderMetadata : IDisposable, ICsvLoader
    {
        public event EventHandler<MessageEventArgs> OnMessage;

        private CsvConfiguration _configuration = null;
        private CultureInfo _cultureFr = CultureInfo.CreateSpecificCulture("fr-FR");
        private Timer _timer = null;

        private HashSet<Entity> _existingReferences = null;
        private HashSet<Member> _existingMembers = null;
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

            _timer = new Timer(10000);
            _timer.Elapsed += delegate (object sender, ElapsedEventArgs e)
            {
                OnMessage?.Invoke(this, new MessageEventArgs { Message = _mainCounter.ToString().PadLeft(10, '0') });
            };
            _timer.Enabled = true;
            _timer.Start();
        }

        ~CvsLoaderMetadata()
        {
            Dispose(disposing: false);
        }

        public void LoadReferences()
        {
            using (var con = new SqlConnection(DbConnectionString))
            {
                con.Open();
                _existingReferences = con.Query<Entity>("select * from Entities").ToHashSet();
                _existingMembers = con.Query<Member>("select * from Members").ToHashSet();
            }
        }

        public void LoadProviders(string path)
        {
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Processing \"{path}\" file" });

            int counter = 0;
            using (var sr = new StreamReader(path, true))
            {
                using (var csv = new CustomCsvReader(sr, _configuration))
                {
                    csv.Read();
                    csv.ReadHeader();

                    using (var con = new SqlConnection(DbConnectionString))
                    {
                        con.Open();

                        #region AJOUT DES EN-TÊTES MANQUANTS
                        var header = csv.Context.HeaderRecord;
                        var membersToAdd = header.Except(_existingMembers.Select(x => x.Name)).ToList();
                        if (membersToAdd != null && membersToAdd.Any())
                        {
                            foreach (var m in membersToAdd)
                            {
                                var member = new Member { Name = m };
                                member.Id = con.InsertMember(member);
                                _existingMembers.Add(member);
                            }
                        }
                        #endregion

                        while (csv.Read())
                        {
                            var externalId = csv.GetField("identifiant");
                            if (!_existingReferences.Any(x => x.Reference == externalId))
                            {
                                var entity = new Entity()
                                {
                                    Reference = externalId
                                };
                                entity.Id = con.InsertEntity(entity);

                                for (int i = 0; i < csv.Context.HeaderRecord.Length; i++)
                                {
                                    var col = csv.Context.HeaderRecord[i];
                                    var id = con.InsertMetadata(entity.Id, _existingMembers.FirstOrDefault(x => x.Name == col).Id, csv[i].ToString());
                                }

                                _existingReferences.Add(entity);
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
                using (var csv = new CustomCsvReader(sr, _configuration))
                {
                    csv.Configuration.Delimiter = ";";
                    csv.Read();
                    csv.ReadHeader();

                    var idx_entreprise_identifiant = csv.GetFieldIndex(CsvColumnsName.EntrepriseIdentifiant);
                    var idx_denomination_sociale = csv.GetFieldIndex(CsvColumnsName.DenominationSociale);

                    var idx_benef_nom = csv.GetFieldIndex(CsvColumnsName.BenefNom);
                    var idx_benef_prenom = csv.GetFieldIndex(CsvColumnsName.BenefPrenom);
                    var idx_benef_identifiant_valeur = csv.GetFieldIndex(CsvColumnsName.BenefIdentifiantValeur);
                    var idx_benef_denomination_sociale = csv.GetFieldIndex(CsvColumnsName.BenefDenominationSociale);
                    var idx_benef_categorie_code = csv.GetFieldIndex(CsvColumnsName.BenefCategorieCode);
                    var idx_benef_qualite_code = csv.GetFieldIndex(CsvColumnsName.BenefQualiteCode);
                    var idx_benef_specialite_code = csv.GetFieldIndex(CsvColumnsName.BenefSpecialiteCode);
                    var idx_benef_titre_code = csv.GetFieldIndex(CsvColumnsName.BenefTitreCode);

                    var idx_ligne_identifiant = csv.GetFieldIndex(CsvColumnsName.LigneIdentifiant);

                    using (var con = new SqlConnection(DbConnectionString))
                    {
                        con.Open();

                        #region AJOUT DES EN-TÊTES MANQUANTS
                        var header = csv.Context.HeaderRecord;
                        var membersToAdd = header.Except(_existingMembers.Select(x => x.Name)).ToList();

                        if (membersToAdd != null && membersToAdd.Any())
                        {
                            foreach (var m in membersToAdd)
                            {
                                var member = new Member { Name = m };
                                member.Id = con.InsertMember(member);
                                _existingMembers.Add(member);
                            }
                        }
                        #endregion

                        while (csv.Read())
                        {
                            try
                            {
                                var date = csv.GetField(new string[] { CsvColumnsName.AvantDateSignature, CsvColumnsName.ConvDateSignature, CsvColumnsName.RemuDate })?.Trim();

                                var dateSignature = Convert.ToDateTime(date, _cultureFr);

                                if (dateSignature.Year == 2019)
                                {
                                    Entity company = null;
                                    Entity benef = null;
                                    Link link = null;

                                    var externalId = csv.GetField(idx_entreprise_identifiant);
                                    if (!_existingReferences.Any(x => x.Reference == externalId))
                                    {
                                        var denomination = csv.GetField(idx_entreprise_identifiant);
                                        company = new Entity()
                                        {
                                            Reference = externalId
                                        };
                                        company.Id = con.InsertEntity(company);

                                        con.InsertMetadata(company.Id, _existingMembers.FirstOrDefault(x => x.Name == csv.Context.HeaderRecord[idx_entreprise_identifiant]).Id, denomination);

                                        _existingReferences.Add(company);
                                        counterCompanies++;
                                    }
                                    else
                                    {
                                        company = _existingReferences.FirstOrDefault(x => x.Reference == externalId);
                                    }

                                    externalId = csv.GetField(idx_benef_identifiant_valeur)?.Trim();

                                    if (string.IsNullOrEmpty(externalId) || "-;[0];[autre];[br];[so];n/a;na;nc;non renseigne;non renseigné;so;infirmier".Contains(externalId.ToLowerInvariant()))
                                    {
                                        externalId = csv.GetHashCodeBenef();
                                    }

                                    if (!_existingReferences.Any(x => x.Reference == externalId))
                                    {
                                        benef = new Entity()
                                        {
                                            Reference = externalId
                                        };
                                        benef.Id = con.InsertEntity(benef);

                                        if (!"[etu][prs][vet]".Contains(csv.GetField(idx_benef_categorie_code).ToLowerInvariant()))
                                        {
                                            var brandName = csv.GetField(idx_benef_denomination_sociale)?.Trim();
                                            con.InsertMetadata(benef.Id, _existingMembers.FirstOrDefault(x => x.Name == csv.Context.HeaderRecord[idx_benef_categorie_code]).Id, brandName);
                                        }
                                        else
                                        {
                                            var lastName = csv.GetField(idx_benef_nom)?.Trim();
                                            var firstName = csv.GetField(idx_benef_prenom)?.Trim();

                                            con.InsertMetadata(benef.Id, _existingMembers.FirstOrDefault(x => x.Name == csv.Context.HeaderRecord[idx_benef_prenom]).Id, firstName);
                                            con.InsertMetadata(benef.Id, _existingMembers.FirstOrDefault(x => x.Name == csv.Context.HeaderRecord[idx_benef_nom]).Id, lastName);
                                        }

                                        _existingReferences.Add(benef);
                                        counterBenef++;
                                    }
                                    else
                                    {
                                        benef = _existingReferences.FirstOrDefault(x => x.Reference == externalId);
                                    }

                                    externalId = csv.GetField(idx_ligne_identifiant).Trim();
                                    if (!_existingReferences.Any(x => x.Reference == externalId))
                                    {
                                        // remu_convention_liee
                                        var amount = csv.GetField(new string[] { CsvColumnsName.AvantMontantTtc, CsvColumnsName.ConvMontantTtc, CsvColumnsName.RemuMontantTtc })?.Trim();
                                        var kind = csv.GetField(new string[] { "avant_nature", "conv_objet" })?.Trim();

                                        var entity = new Entity()
                                        {
                                            Reference = externalId
                                        };
                                        entity.Id = con.InsertEntity(entity);
                                        _existingReferences.Add(entity);

                                        link = new Link
                                        {
                                            Id = entity.Id,
                                            //Amount = Convert.ToDecimal(amount, _cultureFr),
                                            Kind = kind,
                                            Date = Convert.ToDateTime(date, _cultureFr),
                                            From = company,
                                            To = benef,
                                        };
                                        link.Id = con.InsertLink(link);

                                        counterBonds++;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                OnMessage?.Invoke(this, new MessageEventArgs { Message = $"\"{path}\" {e.Message}: {csv.Context.RawRecord}" });
                            }

                            _mainCounter++;
                            records++;
                        }
                    }
                }
            }

            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Read {records} records" });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Added {counterCompanies} companies, {counterBenef} beneficiaries, {counterBonds} insterest bonds" });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Total {_existingReferences.Count} entities" });
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public List<DatavizItem> LinksToJson(string txt, int? take)
        {
            var retour = new List<DatavizItem>();

            return retour;
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

                _timer?.Dispose();
                _timer = null;

                _disposedValue = true;
            }
        }
    }
}
