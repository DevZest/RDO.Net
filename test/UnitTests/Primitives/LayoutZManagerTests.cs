using DevZest.Data.Windows.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class LayoutZManagerTests : LayoutManagerTestsBase
    {
        [TestMethod]
        public void LayoutZManager_Measure()
        {
            var dataSet = MockProductCategories(3, false);
            var layoutManager = CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("20", "Auto", "*")
                    .GridRows("25", "*", "Auto")
                    .RowItem(100, 20).At(1, 0)
                    .RowItem(200, 20).At(2, 0)
                    .RowItem(110, 20).At(1, 1)
                    .RowItem(200, 20).At(2, 1)
                    .RowItem(100, 20).At(1, 2)
                    .RowItem(200, 20).At(2, 2);
            });
            Assert.IsInstanceOfType(layoutManager, typeof(LayoutZManager));

            var measuredSize = layoutManager.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Assert.AreEqual(new Size(330, 65), measuredSize);
            VerifyBlockViewRect(layoutManager, 0, new Rect(20, 0, 310, 65));
            VerifyRowRect(layoutManager, 0, 0, new Rect(0, 0, 310, 65));
            VerifyRowItemRect(layoutManager, layoutManager.CurrentRow, 0, new Rect(0, 0, 110, 25));
            VerifyRowItemRect(layoutManager, layoutManager.CurrentRow, 1, new Rect(110, 0, 200, 25));
            VerifyRowItemRect(layoutManager, layoutManager.CurrentRow, 2, new Rect(0, 25, 110, 20));
            VerifyRowItemRect(layoutManager, layoutManager.CurrentRow, 3, new Rect(110, 25, 200, 20));
            VerifyRowItemRect(layoutManager, layoutManager.CurrentRow, 4, new Rect(0, 45, 110, 20));
            VerifyRowItemRect(layoutManager, layoutManager.CurrentRow, 5, new Rect(110, 45, 200, 20));

            measuredSize = layoutManager.Measure(new Size(300, 300));
            Assert.AreEqual(new Size(300, 300), measuredSize);
            VerifyBlockViewRect(layoutManager, 0, new Rect(20, 0, 280, 300));
            VerifyRowRect(layoutManager, 0, 0, new Rect(0, 0, 280, 300));
            VerifyRowItemRect(layoutManager, layoutManager.CurrentRow, 0, new Rect(0, 0, 110, 25));
            VerifyRowItemRect(layoutManager, layoutManager.CurrentRow, 1, new Rect(110, 0, 170, 25));
            VerifyRowItemRect(layoutManager, layoutManager.CurrentRow, 2, new Rect(0, 25, 110, 255));
            VerifyRowItemRect(layoutManager, layoutManager.CurrentRow, 3, new Rect(110, 25, 170, 255));
            VerifyRowItemRect(layoutManager, layoutManager.CurrentRow, 4, new Rect(0, 280, 110, 20));
            VerifyRowItemRect(layoutManager, layoutManager.CurrentRow, 5, new Rect(110, 280, 170, 20));
        }
    }
}
