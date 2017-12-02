using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Primitives
{
    [TestClass]
    public class ResourceContainerTests
    {
        private interface IResource1 : IResource
        {
        }

        private interface IResource2 : IResource
        {
        }

        private class Resource : IResource1, IResource2
        {
            public object Key
            {
                get { return typeof(Resource); }
            }
        }

        private class Unfreezable : ResourceContainer
        {
        }

        private class Freezable : ResourceContainer, IDesignable
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

            Assert.IsNull(unfreezable.GetResource<IResource1>());
            Assert.IsNull(unfreezable.GetResource<IResource2>());
            var resource = new Resource();
            unfreezable.AddOrUpdateResource(resource);
            Assert.AreEqual(resource, unfreezable.GetResource<IResource1>());
            Assert.AreEqual(resource, unfreezable.GetResource<IResource2>());

            resource = new Resource();
            unfreezable.AddOrUpdateResource(resource);
            Assert.AreEqual(resource, unfreezable.GetResource<IResource1>());
            Assert.AreEqual(resource, unfreezable.GetResource<IResource2>());
        }

        [TestMethod]
        public void ResourceContainer_freezable()
        {
            var freezable = new Freezable();

            var resource = new Resource();
            freezable.AddOrUpdateResource(resource);
            Assert.AreEqual(resource, freezable.GetResource<IResource1>());
            Assert.AreEqual(resource, freezable.GetResource<IResource2>());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResourceContainer_freezable_throws_exception_changing_after_frozen()
        {
            var freezable = new Freezable();

            freezable.Freeze();
            var resource = new Resource();
            freezable.AddOrUpdateResource(resource);
        }
    }
}
