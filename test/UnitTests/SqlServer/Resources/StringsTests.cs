using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DevZest.Data.SqlServer.Resources;

namespace DevZest.Data.SqlServer.Resources
{
    [TestClass]
    public class StringsTests
    {
        [TestMethod]
        public void Strings_value_not_empty()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(Strings.FunctionNotSupported("dummy")), "If this test failed, check the resourceManagerNamespace value in Resource.tt file.");
        }
    }
}
