using System;
using System.Collections.Generic;

namespace Adex.Application.Import
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
