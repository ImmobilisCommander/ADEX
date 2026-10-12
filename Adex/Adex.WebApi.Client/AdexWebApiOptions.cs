using System.ComponentModel.DataAnnotations;

namespace Adex.WebApi.Client
{
    public sealed class AdexWebApiOptions
    {
        public const string SectionName = "AdexApi";

        [Required(
            AllowEmptyStrings = false,
            ErrorMessage = "Le paramètre « AdexApi:BaseAddress » est obligatoire."
        )]
        [Url(ErrorMessage = "Le paramètre « AdexApi:BaseAddress » doit être une URL absolue.")]
        public string BaseAddress { get; set; } = string.Empty;
    }
}
