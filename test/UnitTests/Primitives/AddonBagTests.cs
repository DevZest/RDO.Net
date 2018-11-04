using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Primitives
{
    [TestClass]
    public class AddonBagTests
    {
        private interface IAddon1 : IAddon
        {
        }

        private interface IAddon2 : IAddon
        {
        }

        private class Addon : IAddon1, IAddon2
        {
            public object Key
            {
                get { return typeof(Addon); }
            }
        }

        private class Unfreezable : AddonBag
        {
        }

        private class Freezable : AddonBag, IDesignable
        {

            private bool _designMode = true;
            public bool DesignMode
            {
                get { return _designMode; }
            }

            public void Freeze()
            {
                _designMode = false;
            }
        }

        [TestMethod]
        public void AddonBag_unfreezable()
        {
            var unfreezable = new Unfreezable();

            Assert.IsNull(unfreezable.GetAddon<IAddon1>());
            Assert.IsNull(unfreezable.GetAddon<IAddon2>());
            var addon = new Addon();
            unfreezable.AddOrUpdate(addon);
            Assert.AreEqual(addon, unfreezable.GetAddon<IAddon1>());
            Assert.AreEqual(addon, unfreezable.GetAddon<IAddon2>());

            addon = new Addon();
            unfreezable.AddOrUpdate(addon);
            Assert.AreEqual(addon, unfreezable.GetAddon<IAddon1>());
            Assert.AreEqual(addon, unfreezable.GetAddon<IAddon2>());
        }

        [TestMethod]
        public void AddonBag_freezable()
        {
            var freezable = new Freezable();

            var addon = new Addon();
            freezable.AddOrUpdate(addon);
            Assert.AreEqual(addon, freezable.GetAddon<IAddon1>());
            Assert.AreEqual(addon, freezable.GetAddon<IAddon2>());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddonBag_freezable_throws_exception_changing_after_frozen()
        {
            var freezable = new Freezable();

            freezable.Freeze();
            var addon = new Addon();
            freezable.AddOrUpdate(addon);
        }
    }
}
