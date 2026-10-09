using System;
using System.Collections.Generic;

namespace Adex.WebApi.Import
{

    public enum ImportState
    {
        Idle,
        Running,
        Completed,
        CompletedWithErrors,
        Failed,
        Cancelled,
    }
}
