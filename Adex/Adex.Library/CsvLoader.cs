using Adex.Model;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Adex.Library
{
    public class CsvLoader
    {
        public event EventHandler<MessageEventArgs> OnMessage;

        readonly CsvConfiguration _configuration = null;

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
        }

        public void Load(string path, List<Company> entities)
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
                        entities.Add(new Company()
                        {
                            ExternalId = csv.GetField("identifiant"),
                            Designation = csv.GetField("denomination_sociale")
                        });
                        counter++;
                    }
                }
            }

            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Added {counter} elements" });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"There are {entities.Count} elements" });
        }

        public void Load(string path, Dictionary<string, Beneficiary> entities)
        {
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Processing \"{path}\" file" });
            int counter = 0;
            using (var sr = new StreamReader(path, true))
            {
                using (var csv = new CustomCsvReader(sr, _configuration))
                {
                    csv.Configuration.Delimiter = ";";
                    csv.Read();
                    csv.ReadHeader();

                    var idx_benef_identifiant_valeur = csv.GetFieldIndex("benef_identifiant_valeur");
                    var idx_benef_nom = csv.GetFieldIndex("benef_nom");
                    var idx_benef_prenom = csv.GetFieldIndex("benef_prenom");

                    while (csv.Read())
                    {
                        var externalId = csv.GetField(idx_benef_identifiant_valeur).Trim();
                        var lastName = csv.GetField(idx_benef_nom).Trim();
                        var firstName = csv.GetField(idx_benef_prenom).Trim();

                        var key = $"{externalId}{lastName}{firstName}";
                        if (!entities.ContainsKey(key))
                        {
                            entities.Add(key, new Beneficiary()
                            {
                                ExternalId = externalId,
                                LastName = lastName,
                                FirstName = firstName
                            });
                            counter++;
                        }
                    }
                }
            }

            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Added {counter} elements" });
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"There are {entities.Count} elements" });
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
    }
}
