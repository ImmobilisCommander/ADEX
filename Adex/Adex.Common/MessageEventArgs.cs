using System;

namespace Adex.Common
{
    public class MessageEventArgs : EventArgs
    {
        public Level Level { get; set; }

        public string Message { get; set; }
    }
}