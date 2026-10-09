using System;
using System.Collections.Generic;

namespace Adex.WebApi.Explorer
{
    // Part cumulée du montant détenue par les premiers x % de la population classée par montant décroissant.
    public sealed class ConcentrationPoint
    {
        public double PopulationPercent { get; set; }

        public double AmountPercent { get; set; }
    }

}
