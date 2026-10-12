using System;
using System.Collections.Generic;

namespace Adex.Application.Import
{

    public class ImportStatus
    {
        public ImportState State { get; set; } = ImportState.Idle;

        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public List<ImportStepStatus> Steps { get; set; } = new();
    }
}
