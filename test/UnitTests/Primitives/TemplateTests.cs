using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class TemplateTests
    {
        [TestMethod]
        public void Template_AddGridColumns()
        {
            var template = new Template();
            template.AddGridColumns("25;min:20;max:30", "28px");
            Assert.AreEqual(2, template.InternalGridColumns.Count);
            VerifyGridTrack(template.InternalGridColumns[0], new GridLength(25), 20.0, 30.0);
            VerifyGridTrack(template.InternalGridColumns[1], new GridLength(28), 0.0, double.PositiveInfinity);
        }

        [TestMethod]
        public void Template_AddGridRows()
        {
            var template = new Template();
            template.AddGridRows("25;min:20;max:30", "28px");
            Assert.AreEqual(2, template.InternalGridRows.Count);
            VerifyGridTrack(template.InternalGridRows[0], new GridLength(25), 20.0, 30.0);
            VerifyGridTrack(template.InternalGridRows[1], new GridLength(28), 0.0, double.PositiveInfinity);
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
                x.Block(Orientation.Horizontal);
                x.AddGridColumns("*");
                });

            ExpectInvalidOperationException(x => {
                x.Block(Orientation.Vertical, 0);
                x.AddGridColumns("*");
                });

            ExpectInvalidOperationException(x => {
                x.Block(Orientation.Horizontal, 0);
                x.AddGridColumns("*");
                });

            ExpectInvalidOperationException(x => {
                x.Block(Orientation.Vertical, 0);
                x.AddGridColumns("Auto; min: 10");
                });

            ExpectInvalidOperationException(x => {
                x.AddGridColumns("*");
                x.Block(Orientation.Horizontal);
                });

            ExpectInvalidOperationException(x => {
                x.AddGridColumns("*");
                x.Block(Orientation.Vertical, 0);
                });

            ExpectInvalidOperationException(x => {
                x.AddGridColumns("*");
                x.Block(Orientation.Horizontal, 0);
                });

            ExpectInvalidOperationException(x => {
                x.AddGridColumns("Auto; min: 10");
                x.Block(Orientation.Vertical, 0);
                });
        }

        [TestMethod]
        public void Template_InvalidGridRowHeight_throws_exception()
        {
            ExpectInvalidOperationException(x => {
                x.Block(Orientation.Vertical);
                x.AddGridRows("*");
                });

            ExpectInvalidOperationException(x => {
                x.Block(Orientation.Vertical, 0);
                x.AddGridRows("*");
                });

            ExpectInvalidOperationException(x => {
                x.Block(Orientation.Horizontal, 0);
                x.AddGridRows("*");
                });

            ExpectInvalidOperationException(x => {
                x.Block(Orientation.Horizontal, 0);
                x.AddGridRows("Auto; min: 10");
                });

            ExpectInvalidOperationException(x => {
                x.AddGridRows("*");
                x.Block(Orientation.Vertical);
                });

            ExpectInvalidOperationException(x => {
                x.AddGridRows("*");
                x.Block(Orientation.Vertical, 0);
                });

            ExpectInvalidOperationException(x => {
                x.AddGridRows("*");
                x.Block(Orientation.Horizontal, 0);
                });

            ExpectInvalidOperationException(x => {
                x.AddGridRows("Auto");
                x.Block(Orientation.Horizontal, 0);
                });
        }

        private static void ExpectInvalidOperationException(Action<Template> action)
        {
            try
            {
                var template = new Template();
                action(template);
                Assert.Fail("An ArgumentException should be thrown.'");
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
