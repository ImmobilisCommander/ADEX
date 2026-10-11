using System;

namespace Adex.Mvc.Shared
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public string Message { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
