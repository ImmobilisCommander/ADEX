using System;
using System.Collections.Generic;

namespace Adex.WebApi.Import
{

    public class ImportStepStatus
    {
        public string Name { get; set; } = string.Empty;

        public ImportState State { get; set; } = ImportState.Idle;

        public int ErrorCount { get; set; }

        public string FailureMessage { get; set; }

        public string Message { get; set; }
    }
}
