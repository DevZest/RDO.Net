using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class BinaryTests
    {
        [TestMethod]
        public void Binary_equality_and_inequality()
        {
            var x = new Binary(new byte[] { 1, 2, 3, 4, 5 });
            object o = x;
            Assert.AreEqual(true, x.Equals(o));

            var y = x;
            o = y;
            Assert.AreEqual(true, x.Equals(o));
            Assert.AreEqual(true, x.Equals(y));
            Assert.AreEqual(true, x == y);
            Assert.AreEqual(false, x != y);

            y = new Binary(new byte[] { 1, 2, 3, 4, 5 });
            o = y;
            Assert.AreEqual(true, x.Equals(o));
            Assert.AreEqual(true, x.Equals(y));
            Assert.AreEqual(true, x == y);
            Assert.AreEqual(false, x != y);

            y = new Binary(new byte[] { 1, 2, 3, 4, 4 });
            o = y;
            Assert.AreEqual(false, x.Equals(o));
            Assert.AreEqual(false, x.Equals(y));
            Assert.AreEqual(false, x == y);
            Assert.AreEqual(true, x != y);
        }
    }
}
