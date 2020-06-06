using System;
using System.Collections.Generic;
using System.Text;

namespace Adex.Common
{
    public interface ILink
    {
        int From_Id { get; }

        int To_Id { get; }

        string Kind { get; set; }

        DateTime Date { get; set; }
    }
}
