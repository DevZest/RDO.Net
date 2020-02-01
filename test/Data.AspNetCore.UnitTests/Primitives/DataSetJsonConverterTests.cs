using DevZest.Samples.AdventureWorksLT;
using System.Text.Json;
using System.IO;
using Xunit;
using System.Text;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DataSetJsonConverterTests
    {
        private static JsonSerializerOptions s_serializerOptions;

        static DataSetJsonConverterTests()
        {
            s_serializerOptions = new JsonSerializerOptions();
            s_serializerOptions.Converters.Add(new DataSetJsonConverter());
        }

        [Fact]
        public void Serialize()
        {
            var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategories);
            var jsonString = JsonSerializer.Serialize(dataSet, typeof(DataSet), s_serializerOptions);
            Assert.Equal(dataSet.ToJsonString(false), jsonString);
        }

        [Fact]
        public void Deserialize()
        {
            var result = JsonSerializer.Deserialize<DataSet<ProductCategory>>(Json.ProductCategories, s_serializerOptions);

            var child = result._.SubCategories;
            Assert.Equal(4, result.Count);
            Assert.Equal(3, child.GetChildDataSet(0).Count);
            Assert.Equal(14, child.GetChildDataSet(1).Count);
            Assert.Equal(8, child.GetChildDataSet(2).Count);
            Assert.Equal(12, child.GetChildDataSet(3).Count);

            Assert.Equal(Json.ProductCategories, result.ToJsonString(true));
        }
    }
}
