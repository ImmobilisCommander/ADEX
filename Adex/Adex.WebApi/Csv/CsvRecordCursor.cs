using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.WebApi.Csv
{
    /// <summary>
    /// Forward-only asynchronous reader of semicolon-separated, double-quote-escaped CSV records.
    /// </summary>
    public sealed class CsvRecordCursor
    {
        private readonly TextReader _reader;
        private readonly char[] _buffer = new char[1 << 14];
        private int _position;
        private int _length;

        public CsvRecordCursor(TextReader reader)
        {
            _reader = reader;
        }

        public async ValueTask<List<string>> ReadRecordAsync(CancellationToken cancellationToken)
        {
            var fields = new List<string>();
            var field = new StringBuilder();
            var inQuotes = false;
            var hasContent = false;
            while (true)
            {
                if (_position >= _length && !await FillAsync(cancellationToken))
                {
                    break;
                }

                var c = _buffer[_position++];
                if (inQuotes)
                {
                    if (c != '"')
                    {
                        field.Append(c);
                        continue;
                    }

                    if (_position >= _length)
                    {
                        await FillAsync(cancellationToken);
                    }

                    if (_position < _length && _buffer[_position] == '"')
                    {
                        _position++;
                        field.Append('"');
                    }
                    else
                    {
                        inQuotes = false;
                    }

                    continue;
                }

                switch (c)
                {
                    case '"':
                        inQuotes = true;
                        hasContent = true;
                        break;
                    case ';':
                        fields.Add(field.ToString());
                        field.Clear();
                        hasContent = true;
                        break;
                    case '\r':
                        break;
                    case '\n':
                        if (!hasContent && fields.Count == 0)
                        {
                            break;
                        }

                        fields.Add(field.ToString());
                        return fields;
                    default:
                        field.Append(c);
                        hasContent = true;
                        break;
                }
            }

            if (!hasContent && fields.Count == 0)
            {
                return null;
            }

            fields.Add(field.ToString());
            return fields;
        }

        private async ValueTask<bool> FillAsync(CancellationToken cancellationToken)
        {
            _length = await _reader.ReadAsync(_buffer.AsMemory(), cancellationToken);
            _position = 0;
            return _length > 0;
        }
    }
}
