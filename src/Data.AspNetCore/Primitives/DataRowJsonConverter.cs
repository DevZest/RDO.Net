using System;
using Newtonsoft.Json;
using DevZest.Data.Primitives;

namespace DevZest.Data.AspNetCore.Primitives
{
    /// <summary>
    /// Converts <see cref="DataRow"/> into JSON.
    /// </summary>
    public class DataRowJsonConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(DataRow).IsAssignableFrom(objectType);
        }

        /// <inheritdoc/>
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dataRow = value as DataRow;
            if (dataRow == null)
            {
                writer.WriteNull();
                return;
            }

            var jsonWriter = new JsonWriterAdapter(writer, null);
            jsonWriter.Write(dataRow);
        }

        /// <inheritdoc/>
        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
