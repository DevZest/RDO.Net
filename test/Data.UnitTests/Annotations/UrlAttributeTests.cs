using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class UrlAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Url);
            }

            [Url]
            public _String Url { get; private set; }
        }

        [TestMethod]
        public void UrlAttribute()
        {
            {
                var dataSet = DataSet<TestModel>.Create();
                var dataRow = dataSet.AddRow((_, row) => _.Url[row] = "http://devzest.com");
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(0, validationMessages.Count);
            }

            {
                var dataSet = DataSet<TestModel>.Create();
                var dataRow = dataSet.AddRow((_, row) => _.Url[row] = "devzest.com");
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, UserMessages.UrlAttribute, nameof(TestModel.Url)) , validationMessages[0].Message);
            }
        }
    }
}
