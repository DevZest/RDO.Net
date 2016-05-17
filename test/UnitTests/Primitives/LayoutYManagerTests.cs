using DevZest.Data.Windows.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class LayoutYManagerTests : LayoutManagerTestsBase
    {
        [TestMethod]
        public void LayoutYManager_fixed_rows_only()
        {
            var dataSet = MockProductCategories(9, false);
            var layoutManager = CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("100")
                    .GridRows("10")
                    .RowRange(0, 0, 0, 0)
                    .Layout(Orientation.Vertical, 2);
            });
            Assert.IsInstanceOfType(layoutManager, typeof(LayoutYManager));

            var measuredSize = layoutManager.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Assert.AreEqual(new Size(100, 90), measuredSize);

            measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(new Size(100, 100), measuredSize);
        }
    }
}
