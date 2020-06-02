using Adex.Model;
using System;
using System.Collections.Generic;

namespace Adex.Library
{
    public class LinkManager
    {
        public event EventHandler<MessageEventArgs> OnMessage;

        public string LoadSample(int size)
        {
            string retour = null;

            CsvLoader.ReWriteToUTF8(size);

            var companies = new Dictionary<string, Entity>();
            var beneficiaries = new Dictionary<string, Entity>();
            var bonds = new Dictionary<string, Link>();

            using (var loader = new CsvLoader())
            {
                loader.OnMessage += OnMessage;

                loader.LoadProviders(@"E:\Git\ImmobilisCommander\ADEX\Data\entreprise_2020_05_13_04_00.csv", companies);
                loader.LoadInterestBonds(@"E:\Git\ImmobilisCommander\ADEX\Data\declaration_avantage_2020_05_13_04_00.csv", companies, beneficiaries, bonds);
                loader.LoadInterestBonds(@"E:\Git\ImmobilisCommander\ADEX\Data\declaration_convention_2020_05_13_04_00.csv", companies, beneficiaries, bonds);
                loader.LoadInterestBonds(@"E:\Git\ImmobilisCommander\ADEX\Data\declaration_remuneration_2020_05_13_04_00.csv", companies, beneficiaries, bonds);

                retour = loader.BondsToJson(bonds);

                loader.OnMessage -= OnMessage;
            }

            return retour;
        }
    }
}
