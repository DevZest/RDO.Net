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
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.X).AddGridColumns("*"));
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.XY).AddGridColumns("*"));
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.YX).AddGridColumns("*"));
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.XY).AddGridColumns("Auto"));

            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.Z).AddGridColumns("*").SetOrientation(DataRowOrientation.X));
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.Z).AddGridColumns("*").SetOrientation(DataRowOrientation.XY));
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.Z).AddGridColumns("*").SetOrientation(DataRowOrientation.YX));
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.Z).AddGridColumns("Auto").SetOrientation(DataRowOrientation.XY));
        }

        [TestMethod]
        public void GridTemplate_InvalidGridRowHeight_throws_exception()
        {
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.Y).AddGridRows("*"));
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.XY).AddGridRows("*"));
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.YX).AddGridRows("*"));
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.YX).AddGridRows("Auto"));

            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.Z).AddGridRows("*").SetOrientation(DataRowOrientation.Y));
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.Z).AddGridRows("*").SetOrientation(DataRowOrientation.XY));
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.Z).AddGridRows("*").SetOrientation(DataRowOrientation.YX));
            ExpectArgumentException(x => x.SetOrientation(DataRowOrientation.Z).AddGridRows("Auto").SetOrientation(DataRowOrientation.YX));
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
            template.SetOrientation(DataRowOrientation.Z);
            Assert.AreEqual(ScrollMode.None, template.ScrollMode);
            template.SetOrientation(DataRowOrientation.Y);
            Assert.AreEqual(ScrollMode.Virtualizing, template.ScrollMode);
            template.SetScrollMode(ScrollMode.Normal);
            Assert.AreEqual(ScrollMode.Normal, template.ScrollMode);
        }
    }
}
