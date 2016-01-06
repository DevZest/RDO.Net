using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class IgnoreColumnAttributeTests
    {
        private class MyModel : Model
        {
            public static readonly Accessor<MyModel, _Int32> ColumnAccessor = RegisterColumn((MyModel x) => x.Column);

            [IgnoreColumn]
            public _Int32 Column { get; private set; }
        }

        [TestMethod]
        public void IgnoreColumnAttribute_Test()
        {
            var model = new MyModel();
            Assert.AreEqual(false, model.Column.ShouldSerialize);
        }
    }
}
