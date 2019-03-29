using System;
using Newtonsoft.Json;
using DevZest.Data.Primitives;
using System.Reflection;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DataSetJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsDataSet();
        }

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
