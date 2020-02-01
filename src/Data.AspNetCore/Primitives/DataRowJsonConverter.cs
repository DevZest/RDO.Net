using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevZest.Data.Primitives;

namespace DevZest.Data.AspNetCore.Primitives
{
    /// <summary>
    /// Converts <see cref="DataRow"/> into JSON.
    /// </summary>
    public class DataRowJsonConverter : JsonConverter<DataRow>
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(DataRow).IsAssignableFrom(objectType);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, DataRow value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var jsonWriter = new JsonWriterAdapter(writer, null);
            jsonWriter.Write(value);
        }

        /// <inheritdoc/>
        public override DataRow Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
