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

            [ColumnValidator(nameof(Id))]
            private static IColumnValidationMessages ValidateId(_Int32 column, DataRow dataRow)
            {
                var value = column[dataRow];
                return value < 5 ? ColumnValidationMessages.Empty : new ColumnValidationMessage(ValidationSeverity.Error, "value must be less than 5", column);
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
