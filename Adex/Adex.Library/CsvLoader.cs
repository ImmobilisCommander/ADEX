using Adex.Common;
using Adex.Data.Model;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace Adex.Library
{
    public partial class CsvLoader : IDisposable
    {
        private CsvConfiguration _configuration = null;
        private CultureInfo _cultureFr = CultureInfo.CreateSpecificCulture("fr-FR");
        private Timer _timer = null;
        private Dictionary<string, Company> _companies = null;
        private Dictionary<string, Person> _beneficiaries = null;
        private Dictionary<string, Link> _links = null;
        private HashSet<string> _existingReferences = null;
        private bool disposedValue = false;
        private int _mainCounter = 0;

        public event EventHandler<MessageEventArgs> OnMessage;

        public CsvLoader()
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

            _companies = new Dictionary<string, Company>();
            _beneficiaries = new Dictionary<string, Person>();
            _links = new Dictionary<string, Link>();
        }

        ~CsvLoader()
        {
            Dispose(false);
        }

        public void LoadReferences()
        { 
            using (var db = new AdexContext())
            {
                _existingReferences = new HashSet<string>(db.Entities.Select(x => x.Reference));
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

                    while (csv.Read())
                    {
                        var externalId = csv.GetField("identifiant");
                        if (!_existingReferences.Contains(externalId) && !_companies.ContainsKey(externalId))
                        {
                            _companies.Add(externalId, new Company()
                            {
                                Reference = externalId,
                                Designation = csv.GetField("denomination_sociale")
                            });
                        }
                        counter++;
                    }
                }
            }

            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Found {counter} records in file \"{path}\"", Level = Level.Debug });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"There are {_companies.Count} new companies", Level = Level.Debug });
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

                    var idx_entreprise_identifiant = csv.GetFieldIndex("entreprise_identifiant");
                    var idx_denomination_sociale = csv.GetFieldIndex("denomination_sociale");

                    var idx_benef_identifiant_valeur = csv.GetFieldIndex("benef_identifiant_valeur");
                    var idx_benef_nom = csv.GetFieldIndex("benef_nom");
                    var idx_benef_prenom = csv.GetFieldIndex("benef_prenom");

                    var idx_ligne_identifiant = csv.GetFieldIndex("ligne_identifiant");

                    while (csv.Read())
                    {
                        try
                        {
                            var date = csv.GetField(new string[] { "avant_date_signature", "conv_date_signature", "remu_date" })?.Trim();

                            var dateSignature = Convert.ToDateTime(date, _cultureFr);

                            if (dateSignature.Year == 2019)
                            {
                                Company company = null;
                                Person benef = null;
                                Link link = null;

                                var externalId = csv.GetField(idx_entreprise_identifiant);
                                if (!_companies.ContainsKey(externalId))
                                {
                                    company = new Company()
                                    {
                                        Reference = externalId,
                                        Designation = csv.GetField(idx_denomination_sociale)
                                    };
                                    _companies.Add(company.Reference, company);
                                    counterCompanies++;
                                }
                                else
                                {
                                    company = _companies[externalId];
                                }

                                externalId = csv.GetField(idx_benef_identifiant_valeur)?.Trim();
                                if (!_beneficiaries.ContainsKey(externalId))
                                {
                                    var lastName = csv.GetField(idx_benef_nom)?.Trim();
                                    var firstName = csv.GetField(idx_benef_prenom)?.Trim();

                                    benef = new Person()
                                    {
                                        Reference = externalId,
                                        FirstName = firstName,
                                        LastName = lastName
                                    };
                                    _beneficiaries.Add(benef.Reference, benef);
                                    counterBenef++;
                                }
                                else
                                {
                                    benef = _beneficiaries[externalId];
                                }

                                externalId = csv.GetField(idx_ligne_identifiant).Trim();
                                if (!_links.ContainsKey(externalId))
                                {
                                    // remu_convention_liee
                                    var amount = csv.GetField(new string[] { "avant_montant_ttc", "conv_montant_ttc", "remu_montant_ttc" })?.Trim();
                                    var kind = csv.GetField(new string[] { "avant_nature", "conv_objet" })?.Trim();

                                    link = new FinancialLink
                                    {
                                        Reference = externalId,
                                        Amount = Convert.ToDecimal(amount, _cultureFr),
                                        Kind = kind,
                                        Date = Convert.ToDateTime(date, _cultureFr),
                                        From = company,
                                        To = benef,
                                    };
                                    _links.Add(link.Reference, link);
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

            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Read {records} records" });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Added {counterCompanies} companies, {counterBenef} beneficiaries, {counterBonds} insterest bonds" });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Total {_companies.Count} companies, {_beneficiaries.Count} beneficiaries, {_links.Count} insterest links" });
        }

        public void Save()
        {
            using (var db = new AdexContext())
            {
                using (var t = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Companies.AddRange(_companies.Select(x => x.Value));
                        db.Persons.AddRange(_beneficiaries.Select(x => x.Value));
                        db.Links.AddRange(_links.Select(x => x.Value));
                        db.SaveChanges();
                        t.Commit();
                    }
                    catch (Exception e)
                    {
                        OnMessage?.Invoke(this, new MessageEventArgs { Message = e.Message, Level = Level.Error });
                        t.Rollback();
                    }
                }
            }

            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"{_companies.Count} new companies have been saved", Level = Level.Info });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"{_beneficiaries.Count} new beneficiaries have been saved", Level = Level.Info });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"{_links.Count} new links have been saved", Level = Level.Info });

            _companies.Clear();
            _beneficiaries.Clear();
            _links.Clear();
        }

        public string LinksToJson()
        {
            var all = _links.Select(x => new { id = x.Value.From.Reference, name = x.Value.From.Reference }).Distinct().ToList();
            all.AddRange(_links.Select(x => new { id = x.Value.To.Reference, name = x.Value.To.Reference }).Distinct());

            var links = new List<DatavizItem>();

            foreach (var item in all.Where(x => !string.IsNullOrEmpty(x.id)))
            {
                var temp = _links.Where(x => x.Value.From.Reference == item.id).Where(x => !string.IsNullOrEmpty(x.Value.To.Reference)).Select(x => x.Value.To.Reference);
                links.Add(new DatavizItem { Name = item.id, Size = temp.Distinct().Count(), Imports = temp.Distinct().ToList() });
            }

            var json = JsonConvert.SerializeObject(links.OrderBy(x => x.Name), Formatting.Indented);

            return json;
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
                    _configuration = null;
                    _cultureFr = null;
                }

                _timer?.Dispose();
                _timer = null;

                disposedValue = true;
            }
        }
    }
}
