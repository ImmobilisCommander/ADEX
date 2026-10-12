using System;
using System.ComponentModel.DataAnnotations;

namespace Adex.Data.ReadModel.Explorer
{
    public sealed class DashboardOptions
    {
        public const string SectionName = "Dashboard";

        [Range(
            typeof(TimeSpan),
            "00:01:00",
            "365.00:00:00",
            ErrorMessage = "Le paramètre « Dashboard:TimeToLive » est obligatoire et doit être une durée (hh:mm:ss ou d.hh:mm:ss) comprise entre 00:01:00 et 365.00:00:00."
        )]
        public required TimeSpan TimeToLive { get; set; }
    }
}
