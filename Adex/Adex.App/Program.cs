using Adex.Library;
using Adex.Model;
using log4net;
using log4net.Config;
using System.Collections.Generic;
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
            CsvLoader.ReWriteToUTF8(1000);

            var companies = new Dictionary<string, Entity>();
            var beneficiaries = new Dictionary<string, Entity>();
            var bonds = new Dictionary<string, InterestBond>();

            using (var loader = new CsvLoader())
            {
                loader.OnMessage += Loader_OnMessage;

                loader.LoadProviders(@"E:\Git\ImmobilisCommander\ADEX\Data\entreprise_2020_05_13_04_00.csv", companies);
                loader.LoadInterestBonds(@"E:\Git\ImmobilisCommander\ADEX\Data\declaration_avantage_2020_05_13_04_00.csv", companies, beneficiaries, bonds);
                loader.LoadInterestBonds(@"E:\Git\ImmobilisCommander\ADEX\Data\declaration_convention_2020_05_13_04_00.csv", companies, beneficiaries, bonds);
                loader.LoadInterestBonds(@"E:\Git\ImmobilisCommander\ADEX\Data\declaration_remuneration_2020_05_13_04_00.csv", companies, beneficiaries, bonds);

                //using (var w = new StreamWriter(@"E:\Git\ImmobilisCommander\ADEX\Data\DistinctBeneficiariesId.csv", false, System.Text.Encoding.UTF8))
                //{
                //    w.Write("ExternalId\n");
                //    foreach (var b in beneficiaries.Select(x => x.Value.ExternalId).Distinct())
                //    {
                //        w.Write($"{b}\n");
                //    }
                //}

                //using (var w = new StreamWriter(@"E:\Git\ImmobilisCommander\ADEX\Data\Beneficiaries.csv", false, System.Text.Encoding.UTF8))
                //{
                //    w.Write("ExternalId;Designation\n");
                //    foreach (var b in beneficiaries)
                //    {
                //        w.Write($"{b.Value.ExternalId};{b.Value.Designation}\n");
                //    }
                //}

                //using (var db = new AdexContext())
                //{
                //    db.Companies.Add(new Company() { Designation = "test" });
                //    db.SaveChanges();
                //}

                loader.OnMessage -= Loader_OnMessage;
            }

            _logger.Info("Ending");
        }

        private static void Loader_OnMessage(object sender, MessageEventArgs e)
        {
            _logger.Error(e.Message);
            System.Console.WriteLine(e.Message);
        }
    }
}
