using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Primitives
{
    [TestClass]
    public class ResourceContainerTests
    {
        private interface IResource1 : IExtension
        {
        }

        private interface IResource2 : IExtension
        {
        }

        private class Resource : IResource1, IResource2
        {
            public object Key
            {
                get { return typeof(Resource); }
            }
        }

        private class Unfreezable : ExtensibleObject
        {
        }

        private class Freezable : ExtensibleObject, IDesignable
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
        public void ResourceContainer_unfreezable()
        {
            var unfreezable = new Unfreezable();

            Assert.IsNull(unfreezable.GetExtension<IResource1>());
            Assert.IsNull(unfreezable.GetExtension<IResource2>());
            var resource = new Resource();
            unfreezable.AddOrUpdateExtension(resource);
            Assert.AreEqual(resource, unfreezable.GetExtension<IResource1>());
            Assert.AreEqual(resource, unfreezable.GetExtension<IResource2>());

            resource = new Resource();
            unfreezable.AddOrUpdateExtension(resource);
            Assert.AreEqual(resource, unfreezable.GetExtension<IResource1>());
            Assert.AreEqual(resource, unfreezable.GetExtension<IResource2>());
        }

        [TestMethod]
        public void ResourceContainer_freezable()
        {
            var freezable = new Freezable();

            var resource = new Resource();
            freezable.AddOrUpdateExtension(resource);
            Assert.AreEqual(resource, freezable.GetExtension<IResource1>());
            Assert.AreEqual(resource, freezable.GetExtension<IResource2>());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResourceContainer_freezable_throws_exception_changing_after_frozen()
        {
            var freezable = new Freezable();

            freezable.Freeze();
            var resource = new Resource();
            freezable.AddOrUpdateExtension(resource);
        }
    }
}
