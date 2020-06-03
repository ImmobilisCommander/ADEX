using System;
using System.Collections.Generic;
using System.Text;

namespace Adex.Common
{
    public interface IEntity
    {
        int Id { get; set; }

        string Reference { get; set; }
    }
}
