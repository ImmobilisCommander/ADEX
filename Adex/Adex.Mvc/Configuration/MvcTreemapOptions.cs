using System.ComponentModel.DataAnnotations;

namespace Adex.Mvc.Configuration
{
    public sealed class MvcTreemapOptions
    {
        public const string SectionName = "Treemap";

        [Range(
            2,
            100,
            ErrorMessage = "Le paramètre « Treemap:MaxNodesPerLevel » doit être compris entre 2 et 100."
        )]
        public required int MaxNodesPerLevel { get; set; }
    }
}
