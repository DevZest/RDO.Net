using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class GridTemplateTests
    {
        [TestMethod]
        public void GridTemplate_AddGridColumns()
        {
            var template = new GridTemplate(null).AddGridColumns("25;min:20;max:30", "28px");
            Assert.AreEqual(2, template.GridColumns.Count);
            VerifyGridDefinition(template.GridColumns[0], new GridLength(25), 20.0, 30.0);
            VerifyGridDefinition(template.GridColumns[1], new GridLength(28), 0.0, double.PositiveInfinity);
        }

        [TestMethod]
        public void GridTemplate_AddGridRows()
        {
            var template = new GridTemplate(null).AddGridRows("25;min:20;max:30", "28px");
            Assert.AreEqual(2, template.GridRows.Count);
            VerifyGridDefinition(template.GridRows[0], new GridLength(25), 20.0, 30.0);
            VerifyGridDefinition(template.GridRows[1], new GridLength(28), 0.0, double.PositiveInfinity);
        }

        private void VerifyGridDefinition(GridDefinition gridDef, GridLength expectedLength, double expectedMinLength, double expectedMaxLength)
        {
            Assert.AreEqual(gridDef.Length, expectedLength);
            Assert.AreEqual(gridDef.MinLength, expectedMinLength);
            Assert.AreEqual(gridDef.MaxLength, expectedMaxLength);
        }

        [TestMethod]
        public void GridTemplate_InvalidGridColumnWidth_throws_exception()
        {
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.X).AddGridColumns("*"));
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.XY).AddGridColumns("*"));
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.YX).AddGridColumns("*"));
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.XY).AddGridColumns("Auto; min: 10"));

            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.Z).AddGridColumns("*").WithOrientation(LayoutOrientation.X));
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.Z).AddGridColumns("*").WithOrientation(LayoutOrientation.XY));
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.Z).AddGridColumns("*").WithOrientation(LayoutOrientation.YX));
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.Z).AddGridColumns("Auto; min: 10").WithOrientation(LayoutOrientation.XY));
        }

        [TestMethod]
        public void GridTemplate_InvalidGridRowHeight_throws_exception()
        {
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.Y).AddGridRows("*"));
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.XY).AddGridRows("*"));
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.YX).AddGridRows("*"));
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.YX).AddGridRows("Auto; min: 10"));

            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.Z).AddGridRows("*").WithOrientation(LayoutOrientation.Y));
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.Z).AddGridRows("*").WithOrientation(LayoutOrientation.XY));
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.Z).AddGridRows("*").WithOrientation(LayoutOrientation.YX));
            ExpectArgumentException(x => x.WithOrientation(LayoutOrientation.Z).AddGridRows("Auto").WithOrientation(LayoutOrientation.YX));
        }

        private static void ExpectArgumentException(Action<GridTemplate> action)
        {
            try
            {
                var template = new GridTemplate(null);
                action(template);
                Assert.Fail("An ArgumentException should be thrown.'");
            }
            catch (ArgumentException)
            {
            }
        }
    }
}
