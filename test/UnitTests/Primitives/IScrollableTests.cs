using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class IScrollableTests
    {
        [TestMethod]
        public void IScrollable_get_extent_and_position()
        {
            var dataSet = DataSetMock.ProductCategories(9, false);
            var _ = dataSet._;
            var layoutManager = (LayoutManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("100", "50", "150", "100")
                    .GridRows("50", "50", "50")
                    .Layout(Orientation.Vertical, 3)
                    .AddBinding(1, 1, _.BlockPlaceholder())
                    .AddBinding(2, 1, _.RowPlaceholder());
            });

            layoutManager.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var scrollable = (IScrollable)layoutManager;

            Assert.AreEqual(6, scrollable.MaxGridExtentX);
            Assert.AreEqual(0, scrollable.GetExtentX(0));
            Assert.AreEqual(100, scrollable.GetExtentX(1));
            Assert.AreEqual(150, scrollable.GetExtentX(2));
            Assert.AreEqual(300, scrollable.GetExtentX(3));
            Assert.AreEqual(450, scrollable.GetExtentX(4));
            Assert.AreEqual(600, scrollable.GetExtentX(5));
            Assert.AreEqual(700, scrollable.GetExtentX(6));
            Assert.AreEqual(0, scrollable.GetPositionX(0, GridPlacement.Head));
            Assert.AreEqual(100, scrollable.GetPositionX(1, GridPlacement.Tail));
            Assert.AreEqual(150, scrollable.GetPositionX(2, GridPlacement.Tail));
            Assert.AreEqual(300, scrollable.GetPositionX(3, GridPlacement.Tail));
            Assert.AreEqual(450, scrollable.GetPositionX(4, GridPlacement.Tail));
            Assert.AreEqual(600, scrollable.GetPositionX(5, GridPlacement.Tail));
            Assert.AreEqual(700, scrollable.GetPositionX(6, GridPlacement.Tail));

            Assert.AreEqual(5, scrollable.MaxGridExtentY);
            Assert.AreEqual(0, scrollable.GetExtentY(0));
            Assert.AreEqual(50, scrollable.GetExtentY(1));
            Assert.AreEqual(100, scrollable.GetExtentY(2));
            Assert.AreEqual(150, scrollable.GetExtentY(3));
            Assert.AreEqual(200, scrollable.GetExtentY(4));
            Assert.AreEqual(250, scrollable.GetExtentY(5));
            Assert.AreEqual(0, scrollable.GetPositionY(0, GridPlacement.Head));
            Assert.AreEqual(50, scrollable.GetPositionY(1, GridPlacement.Tail));
            Assert.AreEqual(100, scrollable.GetPositionY(2, GridPlacement.Tail));
            Assert.AreEqual(150, scrollable.GetPositionY(3, GridPlacement.Tail));
            Assert.AreEqual(200, scrollable.GetPositionY(4, GridPlacement.Tail));
            Assert.AreEqual(250, scrollable.GetPositionY(5, GridPlacement.Tail));
        }
    }
}
