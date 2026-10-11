using System.ComponentModel.DataAnnotations;

namespace Adex.WebApi.Explorer
{
    public sealed class DashboardOptions
    {
        public const string SectionName = "Dashboard";

        [Range(1, 8760, ErrorMessage = "Le paramètre « Dashboard:TimeToLiveHours » doit être compris entre 1 et 8760.")]
        public int TimeToLiveHours { get; set; } = 24;
    }
}
