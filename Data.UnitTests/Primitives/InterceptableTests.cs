using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Primitives
{
    [TestClass]
    public class InterceptableTests
    {
        private interface IInterceptor1 : IInterceptor
        {
        }

        private interface IInterceptor2 : IInterceptor
        {
        }

        private class Interceptor : IInterceptor1, IInterceptor2
        {
            public string FullName
            {
                get { return typeof(Interceptor).FullName; }
            }
        }

        private class Unfreezable : Interceptable
        {
        }

        private class Freezable : Interceptable, IDesignable
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
        public void Interceptable_unfreezable()
        {
            var unfreezable = new Unfreezable();

            Assert.IsNull(unfreezable.GetInterceptor<IInterceptor1>());
            Assert.IsNull(unfreezable.GetInterceptor<IInterceptor2>());
            var interceptor = new Interceptor();
            unfreezable.AddOrUpdateInterceptor(interceptor);
            Assert.AreEqual(interceptor, unfreezable.GetInterceptor<IInterceptor1>());
            Assert.AreEqual(interceptor, unfreezable.GetInterceptor<IInterceptor2>());

            interceptor = new Interceptor();
            unfreezable.AddOrUpdateInterceptor(interceptor);
            Assert.AreEqual(interceptor, unfreezable.GetInterceptor<IInterceptor1>());
            Assert.AreEqual(interceptor, unfreezable.GetInterceptor<IInterceptor2>());
        }

        [TestMethod]
        public void Interceptable_freezable()
        {
            var freezable = new Freezable();

            var interceptor = new Interceptor();
            freezable.AddOrUpdateInterceptor(interceptor);
            Assert.AreEqual(interceptor, freezable.GetInterceptor<IInterceptor1>());
            Assert.AreEqual(interceptor, freezable.GetInterceptor<IInterceptor2>());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Interceptable_freezable_throws_exception_changing_after_frozen()
        {
            var freezable = new Freezable();

            freezable.Freeze();
            var interceptor = new Interceptor();
            freezable.AddOrUpdateInterceptor(interceptor);
        }
    }
}
