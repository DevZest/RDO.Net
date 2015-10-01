using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Utilities
{
    [TestClass]
    public class UniqueNameManagerTests
    {
        [TestMethod]
        public void UniqueNameManager_get_unique_name()
        {
            var suffixes = new Dictionary<string, int>();
            Assert.AreEqual("name", suffixes.GetUniqueName("name"));
            Assert.AreEqual("name1", suffixes.GetUniqueName("name"));
            Assert.AreEqual("name2", suffixes.GetUniqueName("name"));
            Assert.AreEqual("name1_1", suffixes.GetUniqueName("name1"));
            Assert.AreEqual("name1_2", suffixes.GetUniqueName("name1"));
        }
    }
}
