using System;
using System.Collections.Generic;
using System.Text;

namespace Adex.Library
{
    public enum Level
    { 
        Debug,
        Info,
        Warn,
        Error
    }

    public class MessageEventArgs : EventArgs
    {
        public Level Level { get; set; }

        public string Message { get; set; }
    }
}
