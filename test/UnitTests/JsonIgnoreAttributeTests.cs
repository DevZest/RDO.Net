using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class JsonIgnoreAttributeTests
    {
        private class MyModel : Model
        {
            public static readonly Property<_Int32> _Column = RegisterColumn((MyModel x) => x.Column);

            [JsonIgnore]
            public _Int32 Column { get; private set; }
        }

        [TestMethod]
        public void JsonIgnoreAttribute_Test()
        {
            var model = new MyModel();
            Assert.AreEqual(false, model.Column.ShouldSerialize);
        }
    }
}
