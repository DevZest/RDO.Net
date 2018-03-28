using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest
{
    [TestClass]
    public class ObjectExtensionsTests
    {
        private sealed class Foo
        {
            public int Count { get; set; }
        }

        [TestMethod]
        public void ObjectExtensions_GetPropertyOrFieldGetter()
        {
            var foo = new Foo();
            var getter = foo.GetPropertyOrFieldGetter<int>(nameof(Foo.Count));
            Assert.AreEqual(0, getter());
        }

        [TestMethod]
        public void ObjectExtensions_GetPropertyOrFieldSetter()
        {
            var foo = new Foo();
            var setter = foo.GetPropertyOrFieldSetter<int>(nameof(Foo.Count));
            setter(5);
            Assert.AreEqual(5, foo.Count);
        }
    }
}
