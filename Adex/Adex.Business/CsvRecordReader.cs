using System;
using System.IO;
using System.Text;

namespace Adex.Business
{
    /// <summary>
    /// Minimal streaming reader for semicolon-separated, double-quote-escaped CSV with embedded
    /// line breaks. Only the requested columns are materialised as strings.
    /// </summary>
    internal sealed class CsvRecordReader : IDisposable
    {
        private readonly TextReader _reader;
        private readonly char[] _buffer = new char[1 << 16];
        private readonly StringBuilder _field = new(512);
        private int _position;
        private int _length;

        public CsvRecordReader(TextReader reader)
        {
            _reader = reader;
        }

        public long RecordNumber { get; private set; }

        /// <summary>Reads the next record into <paramref name="fields"/>; unwanted columns are left null.</summary>
        public bool ReadRecord(string[] fields, bool[] keep)
        {
            var index = 0;
            var inQuotes = false;
            var quotePending = false;
            var hasContent = false;
            var keepField = keep[0];
            _field.Clear();

            while (true)
            {
                if (_position == _length)
                {
                    _length = _reader.Read(_buffer, 0, _buffer.Length);
                    _position = 0;
                    if (_length == 0)
                    {
                        break;
                    }
                }

                var c = _buffer[_position++];
                if (quotePending)
                {
                    quotePending = false;
                    if (c == '"')
                    {
                        if (keepField)
                        {
                            _field.Append('"');
                        }

                        continue;
                    }

                    inQuotes = false;
                }

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        quotePending = true;
                    }
                    else if (keepField)
                    {
                        _field.Append(c);
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
                        Store(fields, keep, ref index);
                        keepField = index < keep.Length && keep[index];
                        hasContent = true;
                        break;
                    case '\r':
                        break;
                    case '\n':
                        if (!hasContent && index == 0)
                        {
                            break;
                        }

                        Store(fields, keep, ref index);
                        return Finish(index, fields.Length);
                    default:
                        hasContent = true;
                        if (keepField)
                        {
                            _field.Append(c);
                        }

                        break;
                }
            }

            if (!hasContent && index == 0)
            {
                return false;
            }

            Store(fields, keep, ref index);
            return Finish(index, fields.Length);
        }

        private void Store(string[] fields, bool[] keep, ref int index)
        {
            if (index < fields.Length)
            {
                fields[index] = keep[index] ? _field.ToString() : null;
            }

            index++;
            _field.Clear();
        }

        private bool Finish(int count, int expected)
        {
            RecordNumber++;
            if (count != expected)
            {
                throw new InvalidDataException(
                    $"Enregistrement {RecordNumber} mal formé : {count} colonnes au lieu de {expected}."
                );
            }

            return true;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
