using Adex.Library;
using Adex.MetaModel;
using Adex.Model;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

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

            using (var db = new AdexMetaContext())
            {
                //var members = new string[] { "identifiant", "pays_code", "pays", "secteur_activite_code", "secteur", "denomination_sociale", "adresse_1", "adresse_2", "adresse_3", "adresse_4", "code_postal", "ville" };

                //foreach (var m in members)
                //{
                //    db.Members.Add(new Member { Name = m });
                //}

                //db.SaveChanges();

                //AddMember(db, members, new string[] { "QBSTAWWV", "[FR]", "FRANCE", "[PA]", "Prestataires associés", "IP Santé domicile", "16 Rue de Montbrillant", "Buroparc Rive Gauche", "", "", "69003", "LYON" });
                //AddMember(db, members, new string[] { "MQKQLNIC", "[FR]", "FRANCE", "[DM]", "Dispositifs médicaux", "SIGVARIS", "ZI SUD D'ANDREZIEUX", "RUE B. THIMONNIER", "", "", "42173", "SAINT-JUST SAINT-RAMBERT CEDEX" });
                //AddMember(db, members, new string[] { "OETEUQSP", "[FR]", "FRANCE", "[AUT]", "Autres", "HEALTHCARE COMPLIANCE CONSULTING FRANCE SAS", "47 BOULEVARD CHARLES V", "", "", "", "14600", "HONFLEUR" });

                //db.SaveChanges();

                db.Links.Add(new MetaModel.Link { From = db.Entities.FirstOrDefault(x => x.Reference == "QBSTAWWV"), To = db.Entities.FirstOrDefault(x => x.Reference == "MQKQLNIC") });
                db.Links.Add(new MetaModel.Link { From = db.Entities.FirstOrDefault(x => x.Reference == "QBSTAWWV"), To = db.Entities.FirstOrDefault(x => x.Reference == "OETEUQSP") });
                db.Links.Add(new MetaModel.Link { From = db.Entities.FirstOrDefault(x => x.Reference == "MQKQLNIC"), To = db.Entities.FirstOrDefault(x => x.Reference == "OETEUQSP") });

                db.SaveChanges();
            }

            //using (var db = new AdexContext())
            //{
            //    var c = new Company { ExternalId = "MyID", Designation = "TEST SA" };
            //    db.Companies.Add(c);
            //    var p = new Person { ExternalId = "DRAOULT", Designation = "DIDIER_RAOULT", FirstName = "Didier", LastName = "RAOULT" };
            //    db.Persons.Add(p);
            //    db.Links.Add(new Link { ExternalId = "MyID_DRAOULT", Designation = "XXXYYY", From = c, To = p, FromId = c.Id, ToId = p.Id });
            //    db.SaveChanges();
            //}

            //var mgr = new LinkManager();
            //mgr.OnMessage += Loader_OnMessage;

            //File.WriteAllText(@"C:\Users\julien.lefevre\Documents\Visual Studio 2015\Projects\Tests\EdgeBundling\sample.json", mgr.LoadSample(15000));

            _logger.Info("Ending");
        }

        private static void AddMember(AdexMetaContext db, string[] members, string[] a)
        {
            var e = new MetaModel.Entity() { Reference = a[0] };
            db.Entities.Add(e);
            for (int i = 0; i < members.Length; i++)
            {
                var mName = members[i];
                var value = a[i];
                db.Metadatas.Add(new Metadata { Entity = e, Member = db.Members.FirstOrDefault(x => x.Name == mName), Value =  value});
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
                    _logger.Error(e.Message);
                    break;
                default:
                    break;
            }
            Console.WriteLine(e.Message);
        }
    }
}
