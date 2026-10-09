using System;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace Adex.Business
{
    internal sealed class AttributeSink
    {
        private readonly ImportTableWriter<AttributeRow> _writer;
        private readonly ArrayBufferWriter<byte> _buffer = new();
        private Utf8JsonWriter _json;
        private Guid _current;
        private bool _open;

        public AttributeSink(ImportTableWriter<AttributeRow> writer)
        {
            _writer = writer;
        }

        public void Add(Guid entityId, string name, string value)
        {
            if (_open && entityId != _current)
            {
                Flush();
            }

            if (!_open)
            {
                _buffer.Clear();
                _json = new Utf8JsonWriter(_buffer);
                _json.WriteStartObject();
                _current = entityId;
                _open = true;
            }

            _json.WriteString(name, value);
        }

        public void Flush()
        {
            if (!_open)
            {
                return;
            }

            _json.WriteEndObject();
            _json.Flush();
            _writer.Add(new AttributeRow(_current, Encoding.UTF8.GetString(_buffer.WrittenSpan)));
            _json.Dispose();
            _open = false;
        }
    }
}
