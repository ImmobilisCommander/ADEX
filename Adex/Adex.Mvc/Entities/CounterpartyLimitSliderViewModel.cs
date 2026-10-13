using System;
using System.Collections.Generic;

namespace Adex.Mvc.Entities
{
    public sealed class CounterpartyLimitSliderViewModel(
        string id,
        string endpointUrl,
        string formAction,
        int limit,
        int maximum,
        IReadOnlyDictionary<string, string> hiddenFields = null
    )
    {
        public string Id => id;

        public string EndpointUrl => endpointUrl;

        public string FormAction => formAction;

        public int Limit => Math.Clamp(limit, 1, Math.Max(1, maximum));

        public int Maximum => Math.Max(1, maximum);

        public bool IsVisible => maximum > 1;

        public IReadOnlyDictionary<string, string> HiddenFields => hiddenFields ?? new Dictionary<string, string>();
    }
}
