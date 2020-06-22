// <copyright file="TestCode.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using Adex.Business;
using Adex.Common;
using Adex.Data.MetaModel;
using Adex.Data.Model;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Adex.App
{
    /// <summary>
    /// This class is meant to keep aside the code used for testing, prototyping. It is not meant to be used for production
    /// </summary>
    internal static class TestCode
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TestCode));

        public static void FillAdexDb()
        {
            using (var db = new AdexContext())
            {
                var c = new Company { Reference = "MyID", Designation = "TEST SA" };
                db.Companies.Add(c);
                var p = new Person { Reference = "DRAOULT", FirstName = "Didier", LastName = "RAOULT" };
                db.Persons.Add(p);
                db.Links.Add(new Data.Model.Link { Reference = "MyID_DRAOULT", From = c, To = p });
                db.SaveChanges();
            }
        }

        internal static void GetJsonAdexMetadataWithDapper()
        {
            using (var loader = new CvsLoaderMetadata())
            {
                loader.OnMessage += Loader_OnMessage;

                loader.DbConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AdexMeta;Integrated Security=True;Connect Timeout=3600;";
                File.WriteAllText(@"C:\Users\julien.lefevre\Documents\Visual Studio 2015\Projects\Tests\EdgeBundling\data.json", JsonConvert.SerializeObject(loader.LinksToJson("", null).ForceDirectedData, Formatting.Indented));

                loader.OnMessage -= Loader_OnMessage;
            }
        }

        public static void FillAdexMetadataWithDbContext()
        {
            Action<AdexMetaContext, string[], string[]> addMetadata = delegate (AdexMetaContext db, string[] members, string[] a)
            {
                var e = new Data.MetaModel.Entity() { Reference = a[0] };
                db.Entities.Add(e);
                for (int i = 0; i < members.Length; i++)
                {
                    var mName = members[i];
                    var value = a[i];
                    db.Metadatas.Add(new Metadata { Entity = e, Member = db.Members.FirstOrDefault(x => x.Name == mName), Value = value });
                }
            };

            using (var db = new AdexMetaContext())
            {
                var members = new string[] { "identifiant", "pays_code", "pays", "secteur_activite_code", "secteur", "denomination_sociale", "adresse_1", "adresse_2", "adresse_3", "adresse_4", "code_postal", "ville" };
                foreach (var m in members)
                {
                    db.Members.Add(new Member { Name = m });
                }
                db.SaveChanges();

                addMetadata(db, members, new string[] { "QBSTAWWV", "[FR]", "FRANCE", "[PA]", "Prestataires associés", "IP Santé domicile", "16 Rue de Montbrillant", "Buroparc Rive Gauche", "", "", "69003", "LYON" });
                addMetadata(db, members, new string[] { "MQKQLNIC", "[FR]", "FRANCE", "[DM]", "Dispositifs médicaux", "SIGVARIS", "ZI SUD D'ANDREZIEUX", "RUE B. THIMONNIER", "", "", "42173", "SAINT-JUST SAINT-RAMBERT CEDEX" });
                addMetadata(db, members, new string[] { "OETEUQSP", "[FR]", "FRANCE", "[AUT]", "Autres", "HEALTHCARE COMPLIANCE CONSULTING FRANCE SAS", "47 BOULEVARD CHARLES V", "", "", "", "14600", "HONFLEUR" });

                db.SaveChanges();

                var a = new Data.MetaModel.Link
                {
                    Reference = "",
                    From = db.Entities.FirstOrDefault(x => x.Reference == "QBSTAWWV"),
                    To = db.Entities.FirstOrDefault(x => x.Reference == "MQKQLNIC")
                };
                a.Reference = $"{a.From.Reference}-{a.To.Reference}";
                db.Links.Add(a);

                var b = new Data.MetaModel.Link
                {
                    Reference = "",
                    From = db.Entities.FirstOrDefault(x => x.Reference == "QBSTAWWV"),
                    To = db.Entities.FirstOrDefault(x => x.Reference == "OETEUQSP")
                };
                b.Reference = $"{b.From.Reference}-{b.To.Reference}";
                db.Links.Add(b);

                var c = new Data.MetaModel.Link
                {
                    Reference = "",
                    From = db.Entities.FirstOrDefault(x => x.Reference == "MQKQLNIC"),
                    To = db.Entities.FirstOrDefault(x => x.Reference == "OETEUQSP")
                };
                c.Reference = $"{c.From.Reference}-{c.To.Reference}";
                db.Links.Add(c);

                db.SaveChanges();
            }
        }

        public static void FillAdexMetadataWithDapper()
        {
            using (var loader = new CvsLoaderMetadata())
            {
                loader.OnMessage += Loader_OnMessage;

                loader.DbConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AdexMeta;Integrated Security=True;";
                loader.LoadReferences();

                loader.LoadProviders(@"E:\Git\ImmobilisCommander\ADEX\Data\big_entreprise_2020_05_13_04_00.csv");
                loader.LoadLinks(@"E:\Git\ImmobilisCommander\ADEX\Data\big_declaration_avantage_2020_05_13_04_00.csv");
                loader.LoadLinks(@"E:\Git\ImmobilisCommander\ADEX\Data\big_declaration_convention_2020_05_13_04_00.csv");
                loader.LoadLinks(@"E:\Git\ImmobilisCommander\ADEX\Data\big_declaration_remuneration_2020_05_13_04_00.csv");

                loader.OnMessage -= Loader_OnMessage;
            }
        }

        public static GraphDataSet LoadSampleFromMetadataDatabase(int? size)
        {
            GraphDataSet retour = null;

            RewriteSampleFiles(size);

            using (var loader = new CvsLoaderMetadata())
            {
                loader.OnMessage += Loader_OnMessage;

                loader.DbConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AdexMeta;Integrated Security=True;";
                loader.LoadReferences();

                loader.LoadProviders(@"E:\Git\ImmobilisCommander\ADEX\Data\entreprise_2020_05_13_04_00.csv");
                loader.LoadLinks(@"E:\Git\ImmobilisCommander\ADEX\Data\declaration_avantage_2020_05_13_04_00.csv");
                loader.LoadLinks(@"E:\Git\ImmobilisCommander\ADEX\Data\declaration_convention_2020_05_13_04_00.csv");
                loader.LoadLinks(@"E:\Git\ImmobilisCommander\ADEX\Data\declaration_remuneration_2020_05_13_04_00.csv");

                retour = loader.LinksToJson(null, size);

                loader.OnMessage -= Loader_OnMessage;
            }

            return retour;
        }

        public static GraphDataSet LoadSampleFromNormalizedDatabase(int? size)
        {
            var retour = new GraphDataSet();

            RewriteSampleFiles(size);

            using (var loader = new CsvLoaderNormalized())
            {
                loader.OnMessage += Loader_OnMessage;
                loader.LoadReferences();

                loader.LoadProviders(@"E:\Git\ImmobilisCommander\ADEX\Data\entreprise_2020_05_13_04_00.csv");
                loader.LoadLinks(@"E:\Git\ImmobilisCommander\ADEX\Data\declaration_avantage_2020_05_13_04_00.csv");
                loader.LoadLinks(@"E:\Git\ImmobilisCommander\ADEX\Data\declaration_convention_2020_05_13_04_00.csv");
                loader.LoadLinks(@"E:\Git\ImmobilisCommander\ADEX\Data\declaration_remuneration_2020_05_13_04_00.csv");

                loader.Save();

                retour = loader.LinksToJson(null, size);

                loader.OnMessage -= Loader_OnMessage;
            }

            return retour;
        }

        private static void RewriteSampleFiles(int? size)
        {
            var files = Directory.GetFiles(@"E:\Git\ImmobilisCommander\ADEX\exports-etalab", "*.csv");
            foreach (var f in files)
            {
                FileHelper.ReWriteToUTF8(f, @"E:\Git\ImmobilisCommander\ADEX\Data", size);
            }
        }

        private static void Loader_OnMessage(object sender, MessageEventArgs e)
        {
            switch (e.Level)
            {
                case Level.Debug:
                    _logger.Debug(e.Message);
                    break;
                case Level.Info:
                    _logger.Info(e.Message);
                    break;
                case Level.Warn:
                    _logger.Warn(e.Message);
                    break;
                case Level.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    _logger.Error(e.Message);
                    break;
                default:
                    break;
            }
            Console.WriteLine($"{sender} {e.Message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
