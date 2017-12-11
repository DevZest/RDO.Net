using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class RegularExpressionAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Name);
            }

            [RegularExpression(@"^[a-zA-Z''-'\s]{1,40}$", Message = "ERR_Name")]
            public new _String Name { get; private set; }
        }

        [TestMethod]
        public void RegularExpressionAttribute()
        {
            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Name[row] = "John Doe O'Dell");
                var validationMessages = dataSet._.Validate(dataRow, ValidationSeverity.Error);
                Assert.AreEqual(0, validationMessages.Count);
            }

            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Name[row] = "John Doe O'Dell John Doe O'Dell John Doe O'Dell John Doe O'Dell");
                var validationMessages = dataSet._.Validate(dataRow, ValidationSeverity.Error);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual("ERR_Name", validationMessages[0].Description);
            }
        }
    }
}
