
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void ParseIdentifier()
        {
            {
                var result = "ABC".ParseIdentifier();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("ABC", result[0]);
            }

            {
                var result = "ABC.DEF".ParseIdentifier();
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual("ABC", result[0]);
                Assert.AreEqual("DEF", result[1]);
            }

            {
                var result = "[ABC]".ParseIdentifier();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("ABC", result[0]);
            }

            {
                var result = "[ABC].[DEF]".ParseIdentifier();
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual("ABC", result[0]);
                Assert.AreEqual("DEF", result[1]);
            }

            {
                var result = "[ABC]]]".ParseIdentifier();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("ABC]", result[0]);
            }

            {
                var result = "[ABC]]].[DE]]F]".ParseIdentifier();
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual("ABC]", result[0]);
                Assert.AreEqual("DE]F", result[1]);
            }
        }
    }
}
