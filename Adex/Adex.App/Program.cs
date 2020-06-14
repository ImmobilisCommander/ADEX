// <copyright file="Program.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using log4net;
using log4net.Config;
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

            // NewMethod();

            TestCode.FillAdexMetadataWithDapper();

            //TestCode.GetJsonAdexMetadataWithDapper();

            //TestCode.FillAdexDb();
        }

        private static void NewMethod()
        {
            using (var sr = new StreamReader(@"E:\Git\ImmobilisCommander\ADEX\Data\big_declaration_avantage_2020_05_13_04_00.csv", true))
            {
                using (var sw = new StreamWriter(@"E:\Temp\idx_ligne_identifiant.csv"))
                {
                    var cult = CultureInfo.CreateSpecificCulture("fr-FR");
                    int records = 0;
                    var header = sr.ReadLine().Split(';');
                    var idx_ligne_identifiant = Array.IndexOf(header, "ligne_identifiant");
                    var idx_date_avantage = Array.IndexOf(header, "avant_date_signature");

                    sw.Write($"ligne_identifiant\n");

                    string record1 = string.Empty;

                    while (!sr.EndOfStream)
                    {
                        records++;
                        var l = sr.ReadLine();
                        if (!string.IsNullOrEmpty(l))
                        {
                            var arr = l.Split(';');

                            if (arr.Length == header.Length)
                            {
                                var date = arr[idx_date_avantage]?.Trim();
                                try
                                {
                                    var dateSignature = default(DateTime);

                                    if (DateTime.TryParse(date, cult, DateTimeStyles.None, out dateSignature))
                                        if (dateSignature.Year == 2019)
                                        {
                                            sw.Write($"{arr[idx_ligne_identifiant]}\n");
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
}
