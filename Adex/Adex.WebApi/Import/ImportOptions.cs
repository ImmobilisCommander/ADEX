using System;
using System.Collections.Generic;

namespace Adex.WebApi.Import
{

    public class ImportOptions
    {
        public string DataDirectory { get; set; } = string.Empty;

        public string FileName { get; set; } = "declarations.csv";

        public string ApiKey { get; set; } = string.Empty;
    }
}
