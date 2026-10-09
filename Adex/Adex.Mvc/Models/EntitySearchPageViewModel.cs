using System;
using System.Collections.Generic;

namespace Adex.Mvc.Models
{

    public sealed class EntitySearchPageViewModel
    {
        public string Query { get; set; } = string.Empty;

        public string ErrorMessage { get; set; }

        public List<EntitySearchResultViewModel> Results { get; set; } = new();
    }
}
