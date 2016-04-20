using DevZest.Data.Windows.Factories;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Controls;

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
        }
    }
}
