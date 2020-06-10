// <copyright file="Program.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using log4net;
using log4net.Config;
using Newtonsoft.Json;
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

            using (var r = new StreamReader(@"E:\Git\ImmobilisCommander\ADEX\Data\big_declaration_avantage_2020_05_13_04_00.csv"))
            {
                var sb = new StringBuilder();
                while (!r.EndOfStream)
                {
                    var s = r.ReadLine(); //    
                    if (s.Contains("BOUAYED"))
                    {
                        sb.Append(s + "\n");
                        System.Console.Write(". ");
                    }
                }
                File.WriteAllText(@"E:\Temp\avantages.csv", sb.ToString());
            }

            //TestCode.FillAdexMetadataWithDapper();

            //TestCode.FillAdexDb();
        }
    }
}
