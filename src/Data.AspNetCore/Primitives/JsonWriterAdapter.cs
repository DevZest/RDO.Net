using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Text.Json;

namespace DevZest.Data.AspNetCore.Primitives
{
    internal class JsonWriterAdapter : JsonWriter
    {
        public JsonWriterAdapter(Utf8JsonWriter jsonWriter, IJsonCustomizer customizer)
            : base(customizer)
        {
            _jsonWriter = jsonWriter;
        }

        private readonly Utf8JsonWriter _jsonWriter;

        protected override void PerformWriteComma()
        {
            //  Utf8JsonWriter will handle comma automatically.
        }

        protected override void PerformWriteStartArray()
        {
            _jsonWriter.WriteStartArray();
        }

        protected override void PerformWriteEndArray()
        {
            _jsonWriter.WriteEndArray();
        }

        protected override void PerformWriteStartObject()
        {
            _jsonWriter.WriteStartObject();
        }

        protected override void PerformWriteEndObject()
        {
            _jsonWriter.WriteEndObject();
        }

        protected override void PerformWritePropertyName(string name)
        {
            _jsonWriter.WritePropertyName(name);
        }

        protected override void PerformWriteValue(JsonValue value)
        {
            if (value.Type == JsonValueType.String)
                _jsonWriter.WriteStringValue(value.Text);
            else
                WriteRawValue(value);
        }

        // Workaround of _jsonWriter.WriteRawValue
        private void WriteRawValue(JsonValue value)
        {
            Debug.Assert(value.Type != JsonValueType.String);

            if (value.Type == JsonValueType.Null)
                _jsonWriter.WriteNullValue();
            else if (value.Type == JsonValueType.True)
                _jsonWriter.WriteBooleanValue(true);
            else if (value.Type == JsonValueType.False)
                _jsonWriter.WriteBooleanValue(false);
            else
            {
                using (JsonDocument doc = JsonDocument.Parse(value.Text))
                {
                    doc.WriteTo(_jsonWriter);
                }
            }
        }

        public override string ToString(bool isPretty)
        {
            throw new NotSupportedException();
        }
    }
}
