// <copyright file="Program.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Adex.App
{
    class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        static Program()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        static void Main(string[] args)
        {
            // https://www.data.gouv.fr/fr/datasets/transparence-sante-1/

            _logger.Info("Starting ********************************");

            //File.WriteAllText(@"C:\Users\julien.lefevre\Documents\Visual Studio 2015\Projects\Tests\EdgeBundling\sample.json", JsonConvert.SerializeObject(TestCode.LoadSampleFromNormalizedDatabase(15000)));

            //var txt = "10001466357";
            //using (var sw = new StreamWriter(@$"E:\Temp\{txt.Replace(" ", "_")}_Data.csv", false, Encoding.UTF8))
            //{
            //    sw.Write($"date;denomination;ligne_identifiant;nature;montant_ttc\n");
            //    Avantages(sw, txt);
            //    Convention(sw, txt);
            //    Remuneration(sw, txt);
            //}

            //TestCode.FillAdexMetadataWithDapper();

            TestCode.GetJsonAdexMetadataWithDapper();

            //TestCode.FillAdexDb();
        }

        private static void Avantages(StreamWriter sw, string txt)
        {
            using (var sr = new StreamReader(@"E:\Git\ImmobilisCommander\ADEX\Data\big_declaration_avantage_2020_05_13_04_00.csv", true))
            {
                var cult = CultureInfo.CreateSpecificCulture("fr-FR");
                int records = 0;
                var header = sr.ReadLine().Split(';');
                var idx_denomination_sociale = Array.IndexOf(header, "denomination_sociale");
                var idx_ligne_identifiant = Array.IndexOf(header, "ligne_identifiant");
                var idx_nature = Array.IndexOf(header, "avant_nature");
                var idx_date = Array.IndexOf(header, "avant_date_signature");
                var idx_montant_ttc = Array.IndexOf(header, "avant_montant_ttc");

                string record1 = string.Empty;

                var sb = new StringBuilder();
                sb.AppendLine(string.Join(';', header));

                while (!sr.EndOfStream)
                {
                    records++;
                    var l = sr.ReadLine();
                    if (!string.IsNullOrEmpty(l))
                    {
                        if (l.ToLower().Contains(txt.ToLower()))
                        {
                            sb.AppendLine(l);
                            var arr = l.Split(';');

                            if (arr.Length == header.Length)
                            {
                                sw.Write($"{arr[idx_date]};{arr[idx_denomination_sociale]};{arr[idx_ligne_identifiant]};{arr[idx_nature]};{arr[idx_montant_ttc]}\n");
                                _logger.Debug(l);
                            }
                            else
                            {
                                _logger.Warn($"{records}: {record1}\n{l}");
                            }
                        }
                    }
                    else
                    {
                        _logger.Warn($"{records}: is empty");
                    }

                    record1 = l;
                }

                File.WriteAllText(@$"E:\Temp\{txt.Replace(" ", "_")}_Avantages_Raw.csv", sb.ToString());
            }
        }

        private static void Convention(StreamWriter sw, string txt)
        {
            using (var sr = new StreamReader(@"E:\Git\ImmobilisCommander\ADEX\Data\big_declaration_convention_2020_05_13_04_00.csv", true))
            {
                var cult = CultureInfo.CreateSpecificCulture("fr-FR");
                int records = 0;
                var header = sr.ReadLine().Split(';');
                var idx_denomination_sociale = Array.IndexOf(header, "denomination_sociale");
                var idx_ligne_identifiant = Array.IndexOf(header, "ligne_identifiant");
                var idx_date = Array.IndexOf(header, "conv_date_signature");
                var idx_nature = Array.IndexOf(header, "conv_objet");
                var idx_montant_ttc = Array.IndexOf(header, "conv_montant_ttc");

                string record1 = string.Empty;
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(';', header));

                while (!sr.EndOfStream)
                {
                    records++;
                    var l = sr.ReadLine();
                    if (!string.IsNullOrEmpty(l))
                    {
                        if (l.ToLower().Contains(txt.ToLower()))
                        {
                            sb.AppendLine(l);
                            var arr = l.Split(';');

                            if (arr.Length == header.Length)
                            {
                                sw.Write($"{arr[idx_date]};{arr[idx_denomination_sociale]};{arr[idx_ligne_identifiant]};{arr[idx_nature]};{arr[idx_montant_ttc]}\n");
                                _logger.Debug(l);
                            }
                            else
                            {
                                _logger.Warn($"{records}: {record1}\n{l}");
                            }
                        }
                    }
                    else
                    {
                        _logger.Warn($"{records}: is empty");
                    }

                    record1 = l;
                }

                File.WriteAllText(@$"E:\Temp\{txt.Replace(" ", "_")}_Convention_Raw.csv", sb.ToString());
            }
        }

        private static void Remuneration(StreamWriter sw, string txt)
        {
            using (var sr = new StreamReader(@"E:\Git\ImmobilisCommander\ADEX\Data\big_declaration_remuneration_2020_05_13_04_00.csv", true))
            {
                var cult = CultureInfo.CreateSpecificCulture("fr-FR");
                int records = 0;
                var header = sr.ReadLine().Split(';');
                var idx_denomination_sociale = Array.IndexOf(header, "denomination_sociale");
                var idx_ligne_identifiant = Array.IndexOf(header, "ligne_identifiant");
                var idx_date = Array.IndexOf(header, "remu_date");
                var idx_montant_ttc = Array.IndexOf(header, "remu_montant_ttc");

                string record1 = string.Empty;
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(';', header));

                while (!sr.EndOfStream)
                {
                    records++;
                    var l = sr.ReadLine();
                    if (!string.IsNullOrEmpty(l))
                    {
                        if (l.ToLower().Contains(txt.ToLower()))
                        {
                            sb.AppendLine(l);
                            var arr = l.Split(';');

                            if (arr.Length == header.Length)
                            {
                                sw.Write($"{arr[idx_date]};{arr[idx_denomination_sociale]};{arr[idx_ligne_identifiant]};remuneration;{arr[idx_montant_ttc]}\n");
                                _logger.Debug(l);
                            }
                            else
                            {
                                _logger.Warn($"{records}: {record1}\n{l}");
                            }
                        }
                    }
                    else
                    {
                        _logger.Warn($"{records}: is empty");
                    }

                    record1 = l;
                }
                File.WriteAllText(@$"E:\Temp\{txt.Replace(" ", "_")}_Remunetation_Raw.csv", sb.ToString());
            }
        }
    }
}
