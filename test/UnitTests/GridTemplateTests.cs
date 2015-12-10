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
            ExpectArgumentException(x => x.SetOrientation(GridFlow.X).AddGridColumns("*"));
            ExpectArgumentException(x => x.SetOrientation(GridFlow.XY).AddGridColumns("*"));
            ExpectArgumentException(x => x.SetOrientation(GridFlow.YX).AddGridColumns("*"));
            ExpectArgumentException(x => x.SetOrientation(GridFlow.XY).AddGridColumns("Auto; min: 10"));

            ExpectArgumentException(x => x.SetOrientation(GridFlow.Z).AddGridColumns("*").SetOrientation(GridFlow.X));
            ExpectArgumentException(x => x.SetOrientation(GridFlow.Z).AddGridColumns("*").SetOrientation(GridFlow.XY));
            ExpectArgumentException(x => x.SetOrientation(GridFlow.Z).AddGridColumns("*").SetOrientation(GridFlow.YX));
            ExpectArgumentException(x => x.SetOrientation(GridFlow.Z).AddGridColumns("Auto; min: 10").SetOrientation(GridFlow.XY));
        }

        [TestMethod]
        public void GridTemplate_InvalidGridRowHeight_throws_exception()
        {
            ExpectArgumentException(x => x.SetOrientation(GridFlow.Y).AddGridRows("*"));
            ExpectArgumentException(x => x.SetOrientation(GridFlow.XY).AddGridRows("*"));
            ExpectArgumentException(x => x.SetOrientation(GridFlow.YX).AddGridRows("*"));
            ExpectArgumentException(x => x.SetOrientation(GridFlow.YX).AddGridRows("Auto; min: 10"));

            ExpectArgumentException(x => x.SetOrientation(GridFlow.Z).AddGridRows("*").SetOrientation(GridFlow.Y));
            ExpectArgumentException(x => x.SetOrientation(GridFlow.Z).AddGridRows("*").SetOrientation(GridFlow.XY));
            ExpectArgumentException(x => x.SetOrientation(GridFlow.Z).AddGridRows("*").SetOrientation(GridFlow.YX));
            ExpectArgumentException(x => x.SetOrientation(GridFlow.Z).AddGridRows("Auto; min: 10").SetOrientation(GridFlow.YX));
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

        [TestMethod]
        public void GridTemplate_ScrollMode()
        {
            var template = new GridTemplate(null);
            template.SetOrientation(GridFlow.Z);
            Assert.AreEqual(ScrollMode.None, template.ScrollMode);
            template.SetOrientation(GridFlow.Y);
            Assert.AreEqual(ScrollMode.Virtualizing, template.ScrollMode);
            template.SetScrollMode(ScrollMode.Normal);
            Assert.AreEqual(ScrollMode.Normal, template.ScrollMode);
        }
    }
}
