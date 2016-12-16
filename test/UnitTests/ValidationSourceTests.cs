using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class ValidationSourceTests
    {
        [TestMethod]
        public void ValidationSource_New()
        {
            {
                var column1 = new _Int32();
                Assert.AreEqual(column1, ValidationSource<Column>.New(column1));
            }

            {
                var column1 = new _Int32();
                var column2 = new _Int32();
                var columnSet = ValidationSource<Column>.New(column1, column2);
                Assert.AreEqual(2, columnSet.Count);
                Assert.IsTrue(columnSet.Contains(column1));
                Assert.IsTrue(columnSet.Contains(column2));
            }
        }

        [TestMethod]
        public void ValidationSource_Union()
        {
            {
                Assert.AreEqual(ValidationSource<Column>.Empty, ValidationSource<Column>.Empty.Union(ValidationSource<Column>.Empty));
            }

            {
                var column1 = new _Int32();
                Assert.AreEqual(column1, ValidationSource<Column>.Empty.Union(column1));
                Assert.AreEqual(column1, column1.Union(ValidationSource<Column>.Empty));
            }

            {
                var column1 = new _Int32();
                Assert.AreEqual(column1, column1.Union(column1));
            }

            {
                var column1 = new _Int32();
                var column2 = new _Int32();
                var columnSet = column1.Union(column2);
                Assert.AreEqual(2, columnSet.Count);
                Assert.IsTrue(columnSet.Contains(column1));
                Assert.IsTrue(columnSet.Contains(column2));
            }
        }
    }
}
