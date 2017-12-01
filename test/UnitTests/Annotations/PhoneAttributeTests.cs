using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class PhoneAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Phone);
            }

            [Phone(MessageId = "ERR_Phone")]
            public _String Phone { get; private set; }
        }

        [TestMethod]
        public void PhoneAttribute()
        {
            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Phone[row] = "(555)-1234567 ext. 1203");
                var validationMessages = dataSet._.Validate(dataRow, ValidationSeverity.Error);
                Assert.AreEqual(0, validationMessages.Count);
            }

            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Phone[row] = "(555)-123456A");
                var validationMessages = dataSet._.Validate(dataRow, ValidationSeverity.Error);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual("ERR_Phone", validationMessages[0].Id);
            }
}
    }
}
