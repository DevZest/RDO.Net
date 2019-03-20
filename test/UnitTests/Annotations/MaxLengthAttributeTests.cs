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
                RegisterColumn((TestModel _) => _.Binary);
            }

            [MaxLength(10)]
            public _Binary Binary  { get; private set; }
        }

        [TestMethod]
        public void MaxLengthAttribute()
        {
            {
                var dataSet = DataSet<TestModel>.Create();
                var dataRow = dataSet.AddRow((_, row) =>
                {
                    _.Binary[row] = new Binary(new byte[6]);
                });
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(0, validationMessages.Count);
            }

            {
                var dataSet = DataSet<TestModel>.Create();
                var dataRow = dataSet.AddRow((_, row) =>
                {
                    _.Binary[row] = new Binary(new byte[11]);
                });
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, UserMessages.MaxLengthAttribute, nameof(TestModel.Binary), 10),
                    validationMessages[0].Message);
            }
        }
    }
}
