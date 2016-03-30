using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class TemplateTests
    {
        [TestMethod]
        public void Template_AddGridColumns()
        {
            var template = new Template(null);
            template.AddGridColumns("25;min:20;max:30", "28px");
            Assert.AreEqual(2, template.GridColumns.Count);
            VerifyGridTrack(template.GridColumns[0], new GridLength(25), 20.0, 30.0);
            VerifyGridTrack(template.GridColumns[1], new GridLength(28), 0.0, double.PositiveInfinity);
        }

        [TestMethod]
        public void Template_AddGridRows()
        {
            var template = new Template(null);
            template.AddGridRows("25;min:20;max:30", "28px");
            Assert.AreEqual(2, template.GridRows.Count);
            VerifyGridTrack(template.GridRows[0], new GridLength(25), 20.0, 30.0);
            VerifyGridTrack(template.GridRows[1], new GridLength(28), 0.0, double.PositiveInfinity);
        }

        private void VerifyGridTrack(GridTrack gridTrack, GridLength expectedLength, double expectedMinLength, double expectedMaxLength)
        {
            Assert.AreEqual(gridTrack.Length, expectedLength);
            Assert.AreEqual(gridTrack.MinLength, expectedMinLength);
            Assert.AreEqual(gridTrack.MaxLength, expectedMaxLength);
        }

        [TestMethod]
        public void Template_InvalidGridColumnWidth_throws_exception()
        {
            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.X;
                x.AddGridColumns("*");
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.Y;
                x.FlowDimension = 0;
                x.AddGridColumns("*");
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.X;
                x.FlowDimension = 0;
                x.AddGridColumns("*");
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.Y;
                x.FlowDimension = 0;
                x.AddGridColumns("Auto; min: 10");
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.Z;
                x.AddGridColumns("*");
                x.RepeatOrientation = RepeatOrientation.X;
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.Z;
                x.AddGridColumns("*");
                x.RepeatOrientation = RepeatOrientation.Y;
                x.FlowDimension = 0;
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.Z;
                x.AddGridColumns("*");
                x.RepeatOrientation = RepeatOrientation.X;
                x.FlowDimension = 0;
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.Z;
                x.AddGridColumns("Auto; min: 10");
                x.RepeatOrientation = RepeatOrientation.Y;
                x.FlowDimension = 0;
                });
        }

        [TestMethod]
        public void Template_InvalidGridRowHeight_throws_exception()
        {
            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.Y;
                x.AddGridRows("*");
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.Y;
                x.FlowDimension = 0;
                x.AddGridRows("*");
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.X;
                x.FlowDimension = 0;
                x.AddGridRows("*");
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.X;
                x.FlowDimension = 0;
                x.AddGridRows("Auto; min: 10");
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.Z;
                x.AddGridRows("*");
                x.RepeatOrientation = RepeatOrientation.Y;
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.Z;
                x.AddGridRows("*");
                x.RepeatOrientation = RepeatOrientation.Y;
                x.FlowDimension = 0;
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.Z;
                x.AddGridRows("*");
                x.RepeatOrientation = RepeatOrientation.X;
                x.FlowDimension = 0;
                });

            ExpectInvalidOperationException(x => {
                x.RepeatOrientation = RepeatOrientation.Z;
                x.AddGridRows("Auto");
                x.RepeatOrientation = RepeatOrientation.X;
                x.FlowDimension = 0;
                });
        }

        private static void ExpectInvalidOperationException(Action<Template> action)
        {
            try
            {
                var template = new Template(null);
                action(template);
                Assert.Fail("An ArgumentException should be thrown.'");
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
