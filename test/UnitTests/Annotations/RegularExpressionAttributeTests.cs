using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class RegularExpressionAttributeTests
    {
        private const string PATTERN = @"^[a-zA-Z''-'\s]{1,40}$";
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Name);
            }

            [RegularExpression(PATTERN)]
            public new _String Name { get; private set; }
        }

        [TestMethod]
        public void RegularExpressionAttribute()
        {
            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Name[row] = "John Doe O'Dell");
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(0, validationMessages.Count);
            }

            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Name[row] = "John Doe O'Dell John Doe O'Dell John Doe O'Dell John Doe O'Dell");
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, UserMessages.RegularExpressionAttribute, nameof(TestModel.Name), PATTERN),
                    validationMessages[0].Message);
            }
        }
    }
}
