using DevZest.Data.Windows.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class LayoutZManagerTests : LayoutManagerTestsBase
    {
        [TestMethod]
        public void LayoutZManager_Measure()
        {
            var dataSet = ProductCategoryDataSet.Mock(3, false);
            var layoutManager = CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("20", "Auto", "*")
                    .GridRows("25", "*", "Auto")
                    .AddBinding(1, 0, new PlaceholderRowBinding(100, 20))
                    .AddBinding(2, 0, new PlaceholderRowBinding(200, 20))
                    .AddBinding(1, 1, new PlaceholderRowBinding(110, 20))
                    .AddBinding(2, 1, new PlaceholderRowBinding(200, 20))
                    .AddBinding(1, 2, new PlaceholderRowBinding(100, 20))
                    .AddBinding(2, 2, new PlaceholderRowBinding(200, 20));
            });
            Assert.IsInstanceOfType(layoutManager, typeof(LayoutZManager));

            var measuredSize = layoutManager.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Assert.AreEqual(new Size(330, 65), measuredSize);
            VerifyBlockViewRect(layoutManager, -1, new Rect(20, 0, 310, 65));
            VerifyRowRect(layoutManager, -1, 0, new Rect(0, 0, 310, 65));
            VerifyRowBindingRect(layoutManager, layoutManager.CurrentRow, 0, new Rect(0, 0, 110, 25));
            VerifyRowBindingRect(layoutManager, layoutManager.CurrentRow, 1, new Rect(110, 0, 200, 25));
            VerifyRowBindingRect(layoutManager, layoutManager.CurrentRow, 2, new Rect(0, 25, 110, 20));
            VerifyRowBindingRect(layoutManager, layoutManager.CurrentRow, 3, new Rect(110, 25, 200, 20));
            VerifyRowBindingRect(layoutManager, layoutManager.CurrentRow, 4, new Rect(0, 45, 110, 20));
            VerifyRowBindingRect(layoutManager, layoutManager.CurrentRow, 5, new Rect(110, 45, 200, 20));

            measuredSize = layoutManager.Measure(new Size(300, 300));
            Assert.AreEqual(new Size(300, 300), measuredSize);
            VerifyBlockViewRect(layoutManager, -1, new Rect(20, 0, 280, 300));
            VerifyRowRect(layoutManager, -1, 0, new Rect(0, 0, 280, 300));
            VerifyRowBindingRect(layoutManager, layoutManager.CurrentRow, 0, new Rect(0, 0, 110, 25));
            VerifyRowBindingRect(layoutManager, layoutManager.CurrentRow, 1, new Rect(110, 0, 170, 25));
            VerifyRowBindingRect(layoutManager, layoutManager.CurrentRow, 2, new Rect(0, 25, 110, 255));
            VerifyRowBindingRect(layoutManager, layoutManager.CurrentRow, 3, new Rect(110, 25, 170, 255));
            VerifyRowBindingRect(layoutManager, layoutManager.CurrentRow, 4, new Rect(0, 280, 110, 20));
            VerifyRowBindingRect(layoutManager, layoutManager.CurrentRow, 5, new Rect(110, 280, 170, 20));
        }

        [TestMethod]
        public void LayoutZManager_GridLineFigures()
        {
            var pen = new Pen();
            var dataSet = ProductCategoryDataSet.Mock(3, false);
            var layoutManager = CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("20", "20")
                    .GridRows("30", "30")
                    .GridLineX(new GridPoint(0, 1), 2, pen)
                    .GridLineY(new GridPoint(1, 0), 2, pen)
                    .AddBinding(0, 0, 1, 1, new PlaceholderRowBinding());
            });

            layoutManager.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var gridLineFigures = layoutManager.GridLineFigures.ToArray();
            Assert.AreEqual(2, gridLineFigures.Length);
            Assert.AreEqual(new Point(0, 30), gridLineFigures[0].StartPoint);
            Assert.AreEqual(new Point(40, 30), gridLineFigures[0].EndPoint);
            Assert.AreEqual(new Point(20, 0), gridLineFigures[1].StartPoint);
            Assert.AreEqual(new Point(20, 60), gridLineFigures[1].EndPoint);
        }
    }
}
