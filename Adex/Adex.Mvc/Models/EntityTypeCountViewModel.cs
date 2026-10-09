using System;
using System.Collections.Generic;

using System.Globalization;

namespace Adex.Mvc.Models
{

    public sealed class EntityTypeCountViewModel
    {
        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }

        public string CountDisplay => Count.ToString("N0", CultureInfo.GetCultureInfo("fr-FR"));
    }
}
