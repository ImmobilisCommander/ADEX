using System;
using System.Collections.Generic;
using System.Text;

namespace Adex.Common
{
    public interface ILink
    {
        int FromId { get; }

        int ToId { get; }

        string Kind { get; set; }

        DateTime Date { get; set; }
    }
}
