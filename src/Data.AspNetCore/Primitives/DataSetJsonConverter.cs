using System;
using Newtonsoft.Json;
using DevZest.Data.Primitives;
using System.Reflection;

namespace DevZest.Data.AspNetCore.Primitives
{
    /// <summary>
    /// Converts <see cref="DataSet"/> into JSON.
    /// </summary>
    public class DataSetJsonConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsDataSet();
        }

        /// <inheritdoc/>
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dataSet = value as DataSet;
            if (dataSet == null)
            {
                writer.WriteNull();
                return;
            }

            var jsonWriter = new JsonWriterAdapter(writer, null);
            jsonWriter.Write(dataSet);
        }

        /// <inheritdoc/>
        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonReader = new JsonReaderAdapter(reader, null);

            var dataSet = existingValue as DataSet;
            if (dataSet == null)
                dataSet = CreateDataSet(objectType);
            jsonReader.Deserialize(dataSet, true);

            return dataSet;
        }

        private class DummyModel : Model
        {
        }

        private static DataSet CreateDataSet(Type dataSetType)
        {
            var createMethod = dataSetType.GetMethod(nameof(DataSet<DummyModel>.Create), BindingFlags.Static | BindingFlags.Public);
            return (DataSet)createMethod.Invoke(null, new object[] { null });
        }
    }
}
