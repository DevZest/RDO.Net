using DevZest.Samples.AdventureWorksLT;
using Newtonsoft.Json;
using System.IO;
using Xunit;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DataSetJsonConverterTests
    {
        [Fact]
        public void Serialize()
        {
            var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategories);
            var converter = new DataSetJsonConverter();
            StringWriter sw = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.None;
            converter.WriteJson(jsonWriter, dataSet, new JsonSerializer());
            Assert.Equal(dataSet.ToJsonString(false), sw.ToString());
        }

        [Fact]
        public void Deserialize()
        {
            var result = JsonConvert.DeserializeObject<DataSet<ProductCategory>>(Json.ProductCategories, new DataSetJsonConverter());

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
