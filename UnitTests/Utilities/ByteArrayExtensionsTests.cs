using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Utilities
{
    [TestClass]
    public class ByteArrayExtensionsTests
    {
        [TestMethod]
        public void ByteArray_ToHexString()
        {
            var bytes = new byte[] { 5, 10, 127, 255 };
            Assert.AreEqual("050A7FFF", bytes.ToHexString());
        }
    }
}
