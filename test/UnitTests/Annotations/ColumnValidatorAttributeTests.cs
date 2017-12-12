using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class ColumnValidatorAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Id);
            }

            public _Int32 Id { get; private set; }

            [ColumnValidator(nameof(Id), "value must be less than 5")]
            private static bool IsIdValid(_Int32 column, DataRow dataRow)
            {
                var value = column[dataRow];
                return value == null || value < 5;
            }
        }

        [TestMethod]
        public void ColumnValidatorAttribute()
        {
            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Id[row] = 3);
                var validationMessages = dataSet._.Validate(dataRow, ValidationSeverity.Error);
                Assert.AreEqual(0, validationMessages.Count);
            }

            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Id[row] = 5);
                var validationMessages = dataSet._.Validate(dataRow, ValidationSeverity.Error);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual("value must be less than 5", validationMessages[0].Description);
            }
        }
    }
}
