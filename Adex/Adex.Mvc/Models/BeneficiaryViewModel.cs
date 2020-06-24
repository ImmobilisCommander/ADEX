using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adex.Mvc.Models
{
    public class BeneficiaryViewModel
    {
        [JsonProperty("identifiant")]
        public string Identifier { get; set; }

        [JsonProperty("pays_code")] 
        public string CountryCode { get; set; }

        [JsonProperty("pays")] 
        public string Country { get; set; }

        [JsonProperty("secteur_activite_code")] 
        public string ActivityCode { get; set; }

        [JsonProperty("secteur")] 
        public string Activity { get; set; }

        [JsonProperty("denomination_sociale")] 
        public string SocialDenomination { get; set; }

        [JsonProperty("adresse_1")] 
        public string Adress1 { get; set; }

        [JsonProperty("adresse_2")] 
        public string Adress2 { get; set; }
        
        [JsonProperty("adresse_3")] 
        public string Adress3 { get; set; }
        
        [JsonProperty("adresse_4")] 
        public string Adress4 { get; set; }

        [JsonProperty("code_postal")] 
        public string ZipCode { get; set; }
        
        [JsonProperty("ville")] 
        public string Town { get; set; }

    }
}
