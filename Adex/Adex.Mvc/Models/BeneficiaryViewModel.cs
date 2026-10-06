using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Adex.Mvc.Models
{
    public class BeneficiaryViewModel
    {
        [JsonPropertyName("identifiant")]
        public string Identifier { get; set; }

        [JsonPropertyName("pays_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("pays")]
        public string Country { get; set; }

        [JsonPropertyName("secteur_activite_code")]
        public string ActivityCode { get; set; }

        [JsonPropertyName("secteur")]
        public string Activity { get; set; }

        [JsonPropertyName("denomination_sociale")]
        public string SocialDenomination { get; set; }

        [JsonPropertyName("adresse_1")]
        public string Adress1 { get; set; }

        [JsonPropertyName("adresse_2")]
        public string Adress2 { get; set; }

        [JsonPropertyName("adresse_3")]
        public string Adress3 { get; set; }

        [JsonPropertyName("adresse_4")]
        public string Adress4 { get; set; }

        [JsonPropertyName("code_postal")]
        public string ZipCode { get; set; }

        [JsonPropertyName("ville")]
        public string Town { get; set; }
    }
}
