using System;
using System.ComponentModel.DataAnnotations;

namespace Adex.Mvc.Configuration
{
    public sealed class MvcCacheOptions
    {
        public const string SectionName = "Cache";

        [Required(ErrorMessage = "Le paramètre « Cache:Dashboard » est obligatoire.")]
        [Range(
            typeof(TimeSpan),
            "00:00:00",
            "365.00:00:00",
            ErrorMessage = "Le paramètre « Cache:Dashboard » doit être une durée (hh:mm:ss ou d.hh:mm:ss) comprise entre 00:00:00 et 365.00:00:00."
        )]
        public required TimeSpan Dashboard { get; set; }

        [Required(ErrorMessage = "Le paramètre « Cache:Entity » est obligatoire.")]
        [Range(
            typeof(TimeSpan),
            "00:00:00",
            "365.00:00:00",
            ErrorMessage = "Le paramètre « Cache:Entity » doit être une durée (hh:mm:ss ou d.hh:mm:ss) comprise entre 00:00:00 et 365.00:00:00."
        )]
        public required TimeSpan Entity { get; set; }

        [Required(ErrorMessage = "Le paramètre « Cache:Counterparties » est obligatoire.")]
        [Range(
            typeof(TimeSpan),
            "00:00:00",
            "365.00:00:00",
            ErrorMessage = "Le paramètre « Cache:Counterparties » doit être une durée (hh:mm:ss ou d.hh:mm:ss) comprise entre 00:00:00 et 365.00:00:00."
        )]
        public required TimeSpan Counterparties { get; set; }

        [Required(ErrorMessage = "Le paramètre « Cache:Search » est obligatoire.")]
        [Range(
            typeof(TimeSpan),
            "00:00:00",
            "365.00:00:00",
            ErrorMessage = "Le paramètre « Cache:Search » doit être une durée (hh:mm:ss ou d.hh:mm:ss) comprise entre 00:00:00 et 365.00:00:00."
        )]
        public required TimeSpan Search { get; set; }

        [Required(ErrorMessage = "Le paramètre « Cache:Sitemap » est obligatoire.")]
        [Range(
            typeof(TimeSpan),
            "00:00:00",
            "365.00:00:00",
            ErrorMessage = "Le paramètre « Cache:Sitemap » doit être une durée (hh:mm:ss ou d.hh:mm:ss) comprise entre 00:00:00 et 365.00:00:00."
        )]
        public required TimeSpan Sitemap { get; set; }
    }
}
