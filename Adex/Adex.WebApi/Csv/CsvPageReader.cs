using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.WebApi.Csv
{
    /// <summary>
    /// Reads a page of records from a semicolon-separated, double-quote-escaped CSV file whose
    /// first record is the header. The file is consumed with a forward-only cursor: it is never
    /// seeked or rewound, and reading stops as soon as the page (plus one matching record, to
    /// detect a next page) has been read. Filters use a case and accent insensitive "contains".
    /// </summary>
    public sealed class CsvPageReader(ILogger<CsvPageReader> logger)
    {
        public async Task<CsvPage> ReadAsync(
            string path,
            int page,
            int pageSize,
            IReadOnlyDictionary<string, string> filters,
            CancellationToken cancellationToken
        )
        {
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                1 << 16,
                FileOptions.SequentialScan | FileOptions.Asynchronous
            );
            using var reader = new StreamReader(stream, new UTF8Encoding(false), true, 1 << 16);

            var cursor = new CsvRecordCursor(reader);
            var header = await cursor.ReadRecordAsync(cancellationToken);
            if (header is null)
            {
                return new CsvPage();
            }

            var filterable = header.Where(CsvFilterableColumns.Names.Contains).ToList();
            var criteria = new List<(int Index, string Value)>();
            foreach (var (column, value) in filters)
            {
                var index = header.FindIndex(x => string.Equals(x, column, StringComparison.OrdinalIgnoreCase));
                if (index < 0)
                {
                    return new CsvPage { Columns = header, FilterableColumns = filterable };
                }

                criteria.Add((index, value));
            }

            var skip = (long)(page - 1) * pageSize;
            long skipped = 0;
            var rows = new List<IReadOnlyList<string>>(pageSize);
            var hasNext = false;
            while (await cursor.ReadRecordAsync(cancellationToken) is { } record)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!Matches(record, criteria))
                {
                    continue;
                }

                if (skipped < skip)
                {
                    skipped++;
                    continue;
                }

                if (rows.Count == pageSize)
                {
                    hasNext = true;
                    break;
                }

                while (record.Count < header.Count)
                {
                    record.Add(string.Empty);
                }

                rows.Add(record);
            }

            return new CsvPage
            {
                Columns = header,
                FilterableColumns = filterable,
                Rows = rows,
                HasNextPage = hasNext,
            };
        }

        private static bool Matches(List<string> record, List<(int Index, string Value)> criteria)
        {
            foreach (var (index, value) in criteria)
            {
                if (
                    index >= record.Count
                    || CultureInfo.InvariantCulture.CompareInfo.IndexOf(
                        record[index],
                        value,
                        CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace
                    ) < 0
                )
                {
                    return false;
                }
            }

            return true;
        }
    }
}
