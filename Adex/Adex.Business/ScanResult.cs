using System.Collections.Generic;

namespace Adex.Business
{
    internal sealed class ScanResult
    {
        public Dictionary<long, Winner> Companies { get; } = new();

        public Dictionary<long, Winner> Persons { get; } = new();

        public Dictionary<long, LinkWinner> Links { get; } = new();

        public long Rows { get; set; }

        public long Skipped { get; set; }
    }
}
