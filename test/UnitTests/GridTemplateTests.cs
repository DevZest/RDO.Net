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
            var template = new GridTemplate(null);
            template.AddGridColumns("25;min:20;max:30", "28px");
            Assert.AreEqual(2, template.GridColumns.Count);
            VerifyGridSpec(template.GridColumns[0], new GridLength(25), 20.0, 30.0);
            VerifyGridSpec(template.GridColumns[1], new GridLength(28), 0.0, double.PositiveInfinity);
        }

        [TestMethod]
        public void GridTemplate_AddGridRows()
        {
            var template = new GridTemplate(null);
            template.AddGridRows("25;min:20;max:30", "28px");
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
            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.X;
                x.AddGridColumns("*");
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.XY;
                x.AddGridColumns("*");
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.YX;
                x.AddGridColumns("*");
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.XY;
                x.AddGridColumns("Auto; min: 10");
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.Z;
                x.AddGridColumns("*");
                x.Orientation = GridOrientation.X;
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.Z;
                x.AddGridColumns("*");
                x.Orientation = GridOrientation.XY;
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.Z;
                x.AddGridColumns("*");
                x.Orientation = GridOrientation.YX;
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.Z;
                x.AddGridColumns("Auto; min: 10");
                x.Orientation = GridOrientation.XY;
                });
        }

        [TestMethod]
        public void GridTemplate_InvalidGridRowHeight_throws_exception()
        {
            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.Y;
                x.AddGridRows("*");
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.XY;
                x.AddGridRows("*");
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.YX;
                x.AddGridRows("*");
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.YX;
                x.AddGridRows("Auto; min: 10");
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.Z;
                x.AddGridRows("*");
                x.Orientation = GridOrientation.Y;
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.Z;
                x.AddGridRows("*");
                x.Orientation = GridOrientation.XY;
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.Z;
                x.AddGridRows("*");
                x.Orientation = GridOrientation.YX;
                });

            ExpectArgumentException(x => {
                x.Orientation = GridOrientation.Z;
                x.AddGridRows("Auto");
                x.Orientation = GridOrientation.YX;
                });
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
