using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class EmailAddressAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.EmailAddress);
            }

            [EmailAddress(MessageId = "ERR_EMAIL")]
            public _String EmailAddress { get; private set; }
        }

        [TestMethod]
        public void EmailAddressAttribute()
        {
            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.EmailAddress[row] = "example@example.com");
                var validationMessages = dataSet._.Validate(dataRow, ValidationSeverity.Error);
                Assert.AreEqual(0, validationMessages.Count);
            }

            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.EmailAddress[row] = "example");
                var validationMessages = dataSet._.Validate(dataRow, ValidationSeverity.Error);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual("ERR_EMAIL", validationMessages[0].Id);
            }
        }
    }
}
