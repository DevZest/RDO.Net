using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Presenters.Primitives
{
    [TestClass]
    public class TabularTextTests
    {
        [TestMethod]
        public void TabularText_Parse()
        {
            {
                var s = string.Empty;
                var result = TabularText.Parse(s, TabularText.CommaDelimiter);
                Assert.AreEqual(0, result.Count);
                Assert.AreEqual(0, result._.TextColumns.Count);
            }

            {
                var s = Environment.NewLine;
                var result = TabularText.Parse(s, TabularText.CommaDelimiter);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(0, result._.TextColumns.Count);
            }

            {
                var s = Environment.NewLine + Environment.NewLine;
                var result = TabularText.Parse(s, TabularText.CommaDelimiter);
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual(0, result._.TextColumns.Count);
            }

            {
                var s = string.Format("a,b,c");
                var result = TabularText.Parse(s, TabularText.CommaDelimiter);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(3, result._.TextColumns.Count);
            }

            {
                var s = string.Format("a,b,c") + Environment.NewLine;
                var result = TabularText.Parse(s, TabularText.CommaDelimiter);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(3, result._.TextColumns.Count);
            }

            {
                var s = "\"a,b,c\",b";
                var result = TabularText.Parse(s, TabularText.CommaDelimiter);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(2, result._.TextColumns.Count);
            }
        }
    }
}
