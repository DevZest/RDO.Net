using DevZest.Data.Addons;
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

        private class Unsealable : AddonBag
        {
        }

        private class Sealable : AddonBag, ISealable
        {

            private bool _sealable = false;
            public bool IsSealed
            {
                get { return _sealable; }
            }

            public void Seal()
            {
                _sealable = true;
            }
        }

        [TestMethod]
        public void AddonBag_unsealable()
        {
            var unsealaable = new Unsealable();

            Assert.IsNull(unsealaable.GetAddon<IAddon1>());
            Assert.IsNull(unsealaable.GetAddon<IAddon2>());
            var addon = new Addon();
            unsealaable.AddOrUpdate(addon);
            Assert.AreEqual(addon, unsealaable.GetAddon<IAddon1>());
            Assert.AreEqual(addon, unsealaable.GetAddon<IAddon2>());

            addon = new Addon();
            unsealaable.AddOrUpdate(addon);
            Assert.AreEqual(addon, unsealaable.GetAddon<IAddon1>());
            Assert.AreEqual(addon, unsealaable.GetAddon<IAddon2>());
        }

        [TestMethod]
        public void AddonBag_sealable()
        {
            var sealable = new Sealable();

            var addon = new Addon();
            sealable.AddOrUpdate(addon);
            Assert.AreEqual(addon, sealable.GetAddon<IAddon1>());
            Assert.AreEqual(addon, sealable.GetAddon<IAddon2>());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void sealable_throws_exception_changing_after_sealed()
        {
            var sealable = new Sealable();

            sealable.Seal();
            var addon = new Addon();
            sealable.AddOrUpdate(addon);
        }
    }
}
