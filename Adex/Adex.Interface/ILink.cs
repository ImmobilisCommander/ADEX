using System;
using System.Collections.Generic;
using System.Text;

namespace Adex.Interface
{
    public interface ILink
    {
        int FromId { get; }

        int ToId { get; }
    }
}
