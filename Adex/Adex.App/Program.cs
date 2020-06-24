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
            _logger.Info("Starting ********************************");

            //File.WriteAllText(@"C:\Users\julien.lefevre\Documents\Visual Studio 2015\Projects\Tests\EdgeBundling\sample.json", JsonConvert.SerializeObject(TestCode.LoadSampleFromNormalizedDatabase(15000)));

            var txt = "MOUNAYER";
            using (var sw = new StreamWriter(@$"E:\Temp\{txt.Replace(" ", "_")}.csv"))
            {
                sw.Write($"date;ligne_identifiant;montant_ttc\n");
                Avantages(sw, txt);
                Convention(sw, txt);
                Remuneration(sw, txt);
            }

            //TestCode.FillAdexMetadataWithDapper();

            //TestCode.GetJsonAdexMetadataWithDapper();

            //TestCode.FillAdexDb();
        }

        private static void Avantages(StreamWriter sw, string txt)
        {
            using (var sr = new StreamReader(@"E:\Git\ImmobilisCommander\ADEX\Data\big_declaration_avantage_2020_05_13_04_00.csv", true))
            {
                var cult = CultureInfo.CreateSpecificCulture("fr-FR");
                int records = 0;
                var header = sr.ReadLine().Split(';');
                var idx_ligne_identifiant = Array.IndexOf(header, "ligne_identifiant");
                var idx_date = Array.IndexOf(header, "avant_date_signature");
                var idx_montant_ttc = Array.IndexOf(header, "avant_montant_ttc");

                string record1 = string.Empty;

                while (!sr.EndOfStream)
                {
                    records++;
                    var l = sr.ReadLine();
                    if (!string.IsNullOrEmpty(l))
                    {
                        if (l.ToLower().Contains(txt.ToLower()))
                        {
                            var arr = l.Split(';');

                            if (arr.Length == header.Length)
                            {
                                var date = arr[idx_date]?.Trim();
                                try
                                {
                                    var dateSignature = default(DateTime);

                                    if (DateTime.TryParse(date, cult, DateTimeStyles.None, out dateSignature))
                                        if (dateSignature.Year == 2019)
                                        {
                                            sw.Write($"{arr[idx_date]};{arr[idx_ligne_identifiant]};{arr[idx_montant_ttc]}\n");
                                            _logger.Debug(l);
                                        }
                                }
                                catch (Exception e)
                                {
                                    _logger.Error(e.Message);
                                }
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
            }
        }

        private static void Convention(StreamWriter sw, string txt)
        {
            using (var sr = new StreamReader(@"E:\Git\ImmobilisCommander\ADEX\Data\big_declaration_convention_2020_05_13_04_00.csv", true))
            {
                var cult = CultureInfo.CreateSpecificCulture("fr-FR");
                int records = 0;
                var header = sr.ReadLine().Split(';');
                var idx_ligne_identifiant = Array.IndexOf(header, "ligne_identifiant");
                var idx_date = Array.IndexOf(header, "conv_date_signature");
                var idx_montant_ttc = Array.IndexOf(header, "conv_montant_ttc");

                string record1 = string.Empty;

                while (!sr.EndOfStream)
                {
                    records++;
                    var l = sr.ReadLine();
                    if (!string.IsNullOrEmpty(l))
                    {
                        if (l.ToLower().Contains(txt.ToLower()))
                        {
                            var arr = l.Split(';');

                            if (arr.Length == header.Length)
                            {
                                var date = arr[idx_date]?.Trim();
                                try
                                {
                                    var dateSignature = default(DateTime);

                                    if (DateTime.TryParse(date, cult, DateTimeStyles.None, out dateSignature))
                                        if (dateSignature.Year == 2019)
                                        {
                                            sw.Write($"{arr[idx_date]};{arr[idx_ligne_identifiant]};{arr[idx_montant_ttc]}\n");
                                            _logger.Debug(l);
                                        }
                                }
                                catch (Exception e)
                                {
                                    _logger.Error(e.Message);
                                }
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
            }
        }

        private static void Remuneration(StreamWriter sw, string txt)
        {
            using (var sr = new StreamReader(@"E:\Git\ImmobilisCommander\ADEX\Data\big_declaration_remuneration_2020_05_13_04_00.csv", true))
            {
                var cult = CultureInfo.CreateSpecificCulture("fr-FR");
                int records = 0;
                var header = sr.ReadLine().Split(';');
                var idx_ligne_identifiant = Array.IndexOf(header, "ligne_identifiant");
                var idx_date = Array.IndexOf(header, "remu_date");
                var idx_montant_ttc = Array.IndexOf(header, "remu_montant_ttc");

                string record1 = string.Empty;

                while (!sr.EndOfStream)
                {
                    records++;
                    var l = sr.ReadLine();
                    if (!string.IsNullOrEmpty(l))
                    {
                        if (l.ToLower().Contains(txt.ToLower()))
                        {
                            var arr = l.Split(';');

                            if (arr.Length == header.Length)
                            {
                                var date = arr[idx_date]?.Trim();
                                try
                                {
                                    var dateSignature = default(DateTime);

                                    if (DateTime.TryParse(date, cult, DateTimeStyles.None, out dateSignature))
                                        if (dateSignature.Year == 2019)
                                        {
                                            sw.Write($"{arr[idx_date]};{arr[idx_ligne_identifiant]};{arr[idx_montant_ttc]}\n");
                                            _logger.Debug(l);
                                        }
                                }
                                catch (Exception e)
                                {
                                    _logger.Error(e.Message);
                                }
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
            }
        }
    }
}
