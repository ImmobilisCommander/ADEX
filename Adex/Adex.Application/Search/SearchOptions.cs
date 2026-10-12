using System.ComponentModel.DataAnnotations;

namespace Adex.Application.Search
{
    public sealed class SearchOptions
    {
        public const string SectionName = "Search";

        [Range(
            0,
            86400,
            ErrorMessage = "Le paramètre « Search:CacheDurationSeconds » doit être compris entre 0 et 86400."
        )]
        public int CacheDurationSeconds { get; set; } = 300;
    }
}
