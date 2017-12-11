using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class StringLengthAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Text);
            }

            [StringLength(10, MinimumLength = 5, Message = "ERR_StringLength")]
            public _String Text { get; private set; }
        }

        [TestMethod]
        public void StringLengthAttribute()
        {
            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Text[row] = "123456");
                var validationMessages = dataSet._.Validate(dataRow, ValidationSeverity.Error);
                Assert.AreEqual(0, validationMessages.Count);
            }

            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Text[row] = "1234");
                var validationMessages = dataSet._.Validate(dataRow, ValidationSeverity.Error);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual("ERR_StringLength", validationMessages[0].Description);
            }

            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Text[row] = "12345678901");
                var validationMessages = dataSet._.Validate(dataRow, ValidationSeverity.Error);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual("ERR_StringLength", validationMessages[0].Description);
            }
        }
    }
}
