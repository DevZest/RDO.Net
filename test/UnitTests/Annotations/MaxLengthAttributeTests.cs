using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class MaxLengthAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Binary1);
                RegisterColumn((TestModel _) => _.Binary2);
            }

            [MaxLength(10)]
            public _Binary Binary1  { get; private set; }

            [MaxLength()]
            public _Binary Binary2 { get; private set; }
        }

        [TestMethod]
        public void MaxLengthAttribute()
        {
            {
                var dataSet = DataSet<TestModel>.Create();
                var dataRow = dataSet.AddRow((_, row) =>
                {
                    _.Binary1[row] = new Binary(new byte[6]);
                    _.Binary2[row] = new Binary(new byte[6]);
                });
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(0, validationMessages.Count);
            }

            {
                var dataSet = DataSet<TestModel>.Create();
                var dataRow = dataSet.AddRow((_, row) =>
                {
                    _.Binary1[row] = new Binary(new byte[11]);
                });
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, UserMessages.MaxLengthAttribute, nameof(TestModel.Binary1), 10),
                    validationMessages[0].Message);
            }
        }
    }
}
