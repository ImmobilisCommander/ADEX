using Adex.Common;
using Adex.Model;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace Adex.Library
{
    public class CsvLoader : IDisposable, ICsvLoader
    {
        private readonly CsvConfiguration _configuration = null;
        private readonly CultureInfo _cultureFr = CultureInfo.CreateSpecificCulture("fr-FR");
        private readonly Timer _timer = null;

        private bool disposedValue = false;
        private int _mainCounter = 0;

        public event EventHandler<MessageEventArgs> OnMessage;

        public CsvLoader()
        {
            _configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = delegate (string[] tab, int count, ReadingContext ctxt)
                {
                },
                BadDataFound = delegate (ReadingContext ctxt)
                {
                    OnMessage?.Invoke(this, new MessageEventArgs { Message = ctxt.RawRecord });
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

        ~CsvLoader()
        {
            Dispose(false);
        }

        public void LoadProviders(string path, Dictionary<string, IEntity> entities)
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
                        entities.Add(externalId, new Company()
                        {
                            Reference = externalId,
                            Designation = csv.GetField("denomination_sociale")
                        });
                        counter++;
                    }
                }
            }

            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Added {counter} elements" });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"There are {entities.Count} elements" });
        }

        public void LoadLinks(string path, Dictionary<string, IEntity> companies, Dictionary<string, IEntity> beneficiaries, Dictionary<string, ILink> links)
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
                                Model.Link link = null;

                                var externalId = csv.GetField(idx_entreprise_identifiant);
                                if (!companies.ContainsKey(externalId))
                                {
                                    company = new Company()
                                    {
                                        Reference = externalId,
                                        Designation = csv.GetField(idx_denomination_sociale)
                                    };
                                    companies.Add(company.Reference, company);
                                    counterCompanies++;
                                }
                                else
                                {
                                    company = companies[externalId] as Company;
                                }

                                externalId = csv.GetField(idx_benef_identifiant_valeur)?.Trim();
                                if (!beneficiaries.ContainsKey(externalId))
                                {
                                    var lastName = csv.GetField(idx_benef_nom)?.Trim();
                                    var firstName = csv.GetField(idx_benef_prenom)?.Trim();

                                    benef = new Person()
                                    {
                                        Reference = externalId
                                    };
                                    beneficiaries.Add(benef.Reference, benef);
                                    counterBenef++;
                                }
                                else
                                {
                                    benef = beneficiaries[externalId] as Person;
                                }

                                externalId = csv.GetField(idx_ligne_identifiant).Trim();
                                if (!links.ContainsKey(externalId))
                                {
                                    // remu_convention_liee

                                    var amount = csv.GetField(new string[] { "avant_montant_ttc", "conv_montant_ttc", "remu_montant_ttc" })?.Trim();
                                    var kind = csv.GetField(new string[] { "avant_nature", "conv_objet" })?.Trim();

                                    link = new Model.Link
                                    {
                                        Reference = externalId,
                                        //Amount = Convert.ToDecimal(amount, _cultureFr),
                                        //Kind = kind,
                                        //DateSignature = Convert.ToDateTime(date, _cultureFr),
                                        //Designation = $"{date:yyyyMMdd}-{kind}-{amount}-{company.Designation}-{benef.Designation}",
                                        From = company,
                                        To = benef,
                                    };
                                    links.Add(link.Reference, link as ILink);
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
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Total {companies.Count} companies, {beneficiaries.Count} beneficiaries, {links.Count} insterest bonds" });
        }

        public static string LinksToJson(Dictionary<string, ILink> obj)
        {
            var all = obj.Select(x => new { id = (x.Value as Model.Link).From.Reference, name = (x.Value as Model.Link).From.Reference }).Distinct().ToList();
            all.AddRange(obj.Select(x => new { id = (x.Value as Model.Link).To.Reference, name = (x.Value as Model.Link).To.Reference }).Distinct());

            var links = new List<Link>();

            foreach (var item in all.Where(x => !string.IsNullOrEmpty(x.id)))
            {
                var temp = obj.Where(x => (x.Value as Model.Link).From.Reference == item.id).Where(x => !string.IsNullOrEmpty((x.Value as Model.Link).To.Reference)).Select(x => (x.Value as Model.Link).To.Reference);
                links.Add(new Link { Name = item.id, Size = temp.Distinct().Count(), Imports = temp.Distinct().ToList() });
            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(links.OrderBy(x => x.Name), Newtonsoft.Json.Formatting.Indented);

            return json;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static void ReWriteToUTF8(int? loops = null)
        {
            var files = Directory.GetFiles(@"E:\Git\ImmobilisCommander\ADEX\exports-etalab", "*.csv");

            foreach (var f in files)
            {
                using (var r = new StreamReader(f, true))
                {
                    var newFile = Path.Combine(@"E:\Git\ImmobilisCommander\ADEX\Data", Path.GetFileName(f));
                    if (File.Exists(newFile))
                    {
                        File.Delete(newFile);
                    }
                    using (var w = new StreamWriter(newFile, false, Encoding.UTF8))
                    {
                        if (loops == null)
                        {
                            while (!r.EndOfStream)
                            {
                                w.Write($"{r.ReadLine()}\n");
                            }
                        }
                        else
                        {
                            for (int i = 0; i < loops; i++)
                            {
                                w.Write($"{r.ReadLine()}\n");
                            }
                        }
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: supprimer l'état managé (objets managés).
                }

                _timer?.Dispose();

                disposedValue = true;
            }
        }

        private class Link
        {
            public string Name { get; set; }

            public int Size { get; set; }

            public List<string> Imports { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
