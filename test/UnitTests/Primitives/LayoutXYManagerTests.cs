using DevZest.Data.Windows.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class LayoutXYManagerTests : LayoutManagerTestsBase
    {
        [TestMethod]
        public void LayoutXYManager_fixed_rows_only()
        {
            var dataSet = MockProductCategories(9, false);
            var layoutManager = CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("100")
                    .GridRows("20")
                    .RowRange(0, 0, 0, 0)
                    .Layout(Orientation.Vertical);
            });

            {
                var measuredSize = layoutManager.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Assert.AreEqual(new Size(100, 180), measuredSize);
            }

            {
                var measuredSize = layoutManager.Measure(new Size(50, 50));
                Assert.AreEqual(new Size(50, 50), measuredSize);
            }
        }
    }
}
