using DevZest.Data.Windows.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class LayoutManagerZTests : LayoutManagerTestsBase
    {
        [TestMethod]
        public void LayoutManagerZ_Measure()
        {
            var dataSet = MockProductCategories(3, false);
            var layoutManager = CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.AddGridColumns("20", "Auto", "*")
                    .AddGridRows("25", "*", "Auto")
                    [1, 0].RowElement(100, 20)
                    [2, 0].RowElement(200, 20)
                    [1, 1].RowElement(110, 20)
                    [2, 1].RowElement(200, 20)
                    [1, 2].RowElement(100, 20)
                    [2, 2].RowElement(200, 20);
            });
            Assert.IsInstanceOfType(layoutManager, typeof(LayoutManagerZ));

            var measuredSize = layoutManager.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Assert.AreEqual(new Size(330, 65), measuredSize);
            VerifyArrangeRect(layoutManager, 0, layoutManager.CurrentRow, new Rect(0, 0, 110, 25));
            VerifyArrangeRect(layoutManager, 1, layoutManager.CurrentRow, new Rect(110, 0, 200, 25));
            VerifyArrangeRect(layoutManager, 2, layoutManager.CurrentRow, new Rect(0, 25, 110, 20));
            VerifyArrangeRect(layoutManager, 3, layoutManager.CurrentRow, new Rect(110, 25, 200, 20));
            VerifyArrangeRect(layoutManager, 4, layoutManager.CurrentRow, new Rect(0, 45, 110, 20));
            VerifyArrangeRect(layoutManager, 5, layoutManager.CurrentRow, new Rect(110, 45, 200, 20));

            measuredSize = layoutManager.Measure(new Size(300, 300));
            Assert.AreEqual(new Size(300, 300), measuredSize);
            VerifyArrangeRect(layoutManager, 0, layoutManager.CurrentRow, new Rect(0, 0, 110, 25));
            VerifyArrangeRect(layoutManager, 1, layoutManager.CurrentRow, new Rect(110, 0, 170, 25));
            VerifyArrangeRect(layoutManager, 2, layoutManager.CurrentRow, new Rect(0, 25, 110, 255));
            VerifyArrangeRect(layoutManager, 3, layoutManager.CurrentRow, new Rect(110, 25, 170, 255));
            VerifyArrangeRect(layoutManager, 4, layoutManager.CurrentRow, new Rect(0, 280, 110, 20));
            VerifyArrangeRect(layoutManager, 5, layoutManager.CurrentRow, new Rect(110, 280, 170, 20));
        }
    }
}
