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
            VerifyGridSpec(template.GridColumns[0], new GridLength(25), 20.0, 30.0);
            VerifyGridSpec(template.GridColumns[1], new GridLength(28), 0.0, double.PositiveInfinity);
        }

        [TestMethod]
        public void GridTemplate_AddGridRows()
        {
            var template = new GridTemplate(null).AddGridRows("25;min:20;max:30", "28px");
            Assert.AreEqual(2, template.GridRows.Count);
            VerifyGridSpec(template.GridRows[0], new GridLength(25), 20.0, 30.0);
            VerifyGridSpec(template.GridRows[1], new GridLength(28), 0.0, double.PositiveInfinity);
        }

        private void VerifyGridSpec(GridSpec gridSpec, GridLength expectedLength, double expectedMinLength, double expectedMaxLength)
        {
            Assert.AreEqual(gridSpec.Length, expectedLength);
            Assert.AreEqual(gridSpec.MinLength, expectedMinLength);
            Assert.AreEqual(gridSpec.MaxLength, expectedMaxLength);
        }

        [TestMethod]
        public void GridTemplate_InvalidGridColumnWidth_throws_exception()
        {
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.X).AddGridColumns("*"));
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.XY).AddGridColumns("*"));
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.YX).AddGridColumns("*"));
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.XY).AddGridColumns("Auto; min: 10"));

            ExpectArgumentException(x => x.WithOrientation(GridOrientation.Z).AddGridColumns("*").WithOrientation(GridOrientation.X));
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.Z).AddGridColumns("*").WithOrientation(GridOrientation.XY));
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.Z).AddGridColumns("*").WithOrientation(GridOrientation.YX));
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.Z).AddGridColumns("Auto; min: 10").WithOrientation(GridOrientation.XY));
        }

        [TestMethod]
        public void GridTemplate_InvalidGridRowHeight_throws_exception()
        {
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.Y).AddGridRows("*"));
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.XY).AddGridRows("*"));
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.YX).AddGridRows("*"));
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.YX).AddGridRows("Auto; min: 10"));

            ExpectArgumentException(x => x.WithOrientation(GridOrientation.Z).AddGridRows("*").WithOrientation(GridOrientation.Y));
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.Z).AddGridRows("*").WithOrientation(GridOrientation.XY));
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.Z).AddGridRows("*").WithOrientation(GridOrientation.YX));
            ExpectArgumentException(x => x.WithOrientation(GridOrientation.Z).AddGridRows("Auto").WithOrientation(GridOrientation.YX));
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
