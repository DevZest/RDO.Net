using Newtonsoft.Json;
using System;

namespace DevZest.Data.AspNetCore.Primitives
{
    internal class JsonWriterAdapter : Data.Primitives.JsonWriter
    {
        public JsonWriterAdapter(JsonWriter jsonWriter, IJsonCustomizer customizer)
            : base(customizer)
        {
            _jsonWriter = jsonWriter;
        }

        private readonly JsonWriter _jsonWriter;

        protected override void PerformWriteComma()
        {
            //  Newton JSON will handle comma automatically.
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
                _jsonWriter.WriteValue(value.Text);
            else
                _jsonWriter.WriteRawValue(value.Text);
        }

        public override string ToString(bool isPretty)
        {
            throw new NotSupportedException();
        }
    }
}
