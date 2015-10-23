using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class GridTemplateTests
    {
        [TestMethod]
        public void GridTemplate_AddGridColumns()
        {
        }

        [TestMethod]
        public void GridTemplate_AddGridRows()
        {
            var template = new GridTemplate()
                .SetOrientation(DataRowOrientation.Z)
                .AddGridRows("25;min:37", "25pt", "25cm", "*", "10*", "auto", "Auto");
            Assert.AreEqual(7, template.GridRows.Count);
        }

    }
}
