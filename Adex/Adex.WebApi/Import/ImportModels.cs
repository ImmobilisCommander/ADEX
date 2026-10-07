using System;
using System.Collections.Generic;

namespace Adex.WebApi.Import
{
    public enum ImportTarget
    {
        All,
        Normalized,
        Metadata,
    }

    public enum ImportState
    {
        Idle,
        Running,
        Completed,
        CompletedWithErrors,
        Failed,
        Cancelled,
    }

    public class ImportOptions
    {
        public string DataDirectory { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;
    }

    public class ImportStepStatus
    {
        public string Name { get; set; } = string.Empty;

        public ImportState State { get; set; } = ImportState.Idle;

        public int ErrorCount { get; set; }

        public string FailureMessage { get; set; }
    }

    public class ImportStatus
    {
        public ImportState State { get; set; } = ImportState.Idle;

        public ImportTarget Target { get; set; }

        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public List<ImportStepStatus> Steps { get; set; } = new();
    }
}
