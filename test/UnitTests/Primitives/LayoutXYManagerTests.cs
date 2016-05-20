using DevZest.Data.Windows.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class LayoutXYManagerTests : LayoutManagerTestsBase
    {
        [TestMethod]
        public void LayoutXYManager_ScrollInfo()
        {
            var dataSet = MockProductCategories(9, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("100")
                    .GridRows("20")
                    .Layout(Orientation.Vertical)
                    .RowItem().At(0, 0);
            });

            {
                var measuredSize = layoutManager.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
                Assert.AreEqual(8, layoutManager.BlockViews.Last.Ordinal);
                Assert.AreEqual(new Size(100, 180), measuredSize);
                Assert.AreEqual(100, layoutManager.ExtentX);
                Assert.AreEqual(180, layoutManager.ExtentY);
                Assert.AreEqual(100, layoutManager.ViewportX);
                Assert.AreEqual(180, layoutManager.ViewportY);
                Assert.AreEqual(0, layoutManager.ScrollOffsetX);
                Assert.AreEqual(0, layoutManager.ScrollOffsetY);
            }

            {
                var measuredSize = layoutManager.Measure(new Size(50, 30));
                Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
                Assert.AreEqual(1, layoutManager.BlockViews.Last.Ordinal);
                Assert.AreEqual(new Size(50, 30), measuredSize);
                Assert.AreEqual(100, layoutManager.ExtentX);
                Assert.AreEqual(180, layoutManager.ExtentY);
                Assert.AreEqual(50, layoutManager.ViewportX);
                Assert.AreEqual(30, layoutManager.ViewportY);
                Assert.AreEqual(0, layoutManager.ScrollOffsetX);
                Assert.AreEqual(0, layoutManager.ScrollOffsetY);
            }

            {
                layoutManager.ScrollOffsetX = 20;
                layoutManager.ScrollOffsetY = 30;
                var measuredSize = layoutManager.Measure(new Size(50, 30));
                Assert.AreEqual(1, layoutManager.BlockViews.First.Ordinal);
                Assert.AreEqual(2, layoutManager.BlockViews.Last.Ordinal);
                Assert.AreEqual(new Size(50, 30), measuredSize);
                Assert.AreEqual(100, layoutManager.ExtentX);
                Assert.AreEqual(180, layoutManager.ExtentY);
                Assert.AreEqual(50, layoutManager.ViewportX);
                Assert.AreEqual(30, layoutManager.ViewportY);
                Assert.AreEqual(20, layoutManager.ScrollOffsetX);
                Assert.AreEqual(30, layoutManager.ScrollOffsetY);
            }

            {
                layoutManager.ScrollOffsetY = 0;
                var measuredSize = layoutManager.Measure(new Size(50, 30));
                Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
                Assert.AreEqual(1, layoutManager.BlockViews.Last.Ordinal);
                Assert.AreEqual(new Size(50, 30), measuredSize);
                Assert.AreEqual(100, layoutManager.ExtentX);
                Assert.AreEqual(180, layoutManager.ExtentY);
                Assert.AreEqual(50, layoutManager.ViewportX);
                Assert.AreEqual(30, layoutManager.ViewportY);
                Assert.AreEqual(20, layoutManager.ScrollOffsetX);
                Assert.AreEqual(0, layoutManager.ScrollOffsetY);
            }
        }

        [TestMethod]
        public void LayoutXYManager_RowItem()
        {
            var dataSet = MockProductCategories(9, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("100")
                    .GridRows("Auto")
                    .Layout(Orientation.Vertical)
                    .RowItem().Bind((r, e) => { e.DesiredHeight = 10 * (r.Ordinal + 1); }).At(0, 0);
            });

            var measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(3, layoutManager.BlockViews.Last.Ordinal);

            var rowItem = layoutManager.Template.RowItemGroups[0][0];
            BlockView block;
                
            block = layoutManager.BlockViews[0];
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[1];
            Assert.AreEqual(new Rect(0, 10, 100, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[2];
            Assert.AreEqual(new Rect(0, 30, 100, 30), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[3];
            Assert.AreEqual(new Rect(0, 60, 100, 40), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowItemRect(block, rowItem));

            // Do another measure to detect bugs related to recycled elements
            measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(3, layoutManager.BlockViews.Last.Ordinal);

            rowItem = layoutManager.Template.RowItemGroups[0][0];

            block = layoutManager.BlockViews[0];
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[1];
            Assert.AreEqual(new Rect(0, 10, 100, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[2];
            Assert.AreEqual(new Rect(0, 30, 100, 30), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[3];
            Assert.AreEqual(new Rect(0, 60, 100, 40), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowItemRect(block, rowItem));
        }

        [TestMethod]
        public void LayoutXYManager_RowItem_multidimensional()
        {
            var dataSet = MockProductCategories(9, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("100")
                    .GridRows("Auto")
                    .Layout(Orientation.Vertical, 2)
                    .RowItem().Bind((r, e) => { e.DesiredHeight = 10 * (r.Ordinal + 1); }).At(0, 0);
            });

            var measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(2, layoutManager.BlockViews.Last.Ordinal);

            var rowItem = layoutManager.Template.RowItemGroups[0][0];
            BlockView block;

            block = layoutManager.BlockViews[0];
            Assert.AreEqual(new Rect(0, 0, 200, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(100, 0, 100, 20), layoutManager.GetRowRect(block, 1));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[1];
            Assert.AreEqual(new Rect(0, 20, 200, 40), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(100, 0, 100, 40), layoutManager.GetRowRect(block, 1));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[2];
            Assert.AreEqual(new Rect(0, 60, 200, 60), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 60), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(100, 0, 100, 60), layoutManager.GetRowRect(block, 1));
            Assert.AreEqual(new Rect(0, 0, 100, 60), layoutManager.GetRowItemRect(block, rowItem));
        }

        [TestMethod]
        public void LayoutXYManager_BlockItem()
        {
            var dataSet = MockProductCategories(9, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("20", "100", "20")
                    .GridRows("20")
                    .Layout(Orientation.Vertical, 2)
                    .BlockItem().At(0, 0)
                    .RowItem().At(1, 0)
                    .BlockItem().At(2, 0);
            });

            var measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(4, layoutManager.BlockViews.Last.Ordinal);

            BlockView block;
            var blockItem0 = layoutManager.Template.BlockItems[0];
            var blockItem1 = layoutManager.Template.BlockItems[1];

            block = layoutManager.BlockViews[0];
            Assert.AreEqual(new Rect(0, 0, 240, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 20, 20), layoutManager.GetBlockItemRect(block, blockItem0));
            Assert.AreEqual(new Rect(220, 0, 20, 20), layoutManager.GetBlockItemRect(block, blockItem1));

            block = layoutManager.BlockViews[1];
            Assert.AreEqual(new Rect(0, 20, 240, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 20, 20), layoutManager.GetBlockItemRect(block, blockItem0));
            Assert.AreEqual(new Rect(220, 0, 20, 20), layoutManager.GetBlockItemRect(block, blockItem1));

            block = layoutManager.BlockViews[2];
            Assert.AreEqual(new Rect(0, 40, 240, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 20, 20), layoutManager.GetBlockItemRect(block, blockItem0));
            Assert.AreEqual(new Rect(220, 0, 20, 20), layoutManager.GetBlockItemRect(block, blockItem1));

            block = layoutManager.BlockViews[3];
            Assert.AreEqual(new Rect(0, 60, 240, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 20, 20), layoutManager.GetBlockItemRect(block, blockItem0));
            Assert.AreEqual(new Rect(220, 0, 20, 20), layoutManager.GetBlockItemRect(block, blockItem1));

            block = layoutManager.BlockViews[4];
            Assert.AreEqual(new Rect(0, 80, 240, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 20, 20), layoutManager.GetBlockItemRect(block, blockItem0));
            Assert.AreEqual(new Rect(220, 0, 20, 20), layoutManager.GetBlockItemRect(block, blockItem1));
        }

        [TestMethod]
        public void LayoutXYManager_ScalarItem()
        {
            var dataSet = MockProductCategories(9, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("20", "100")
                    .GridRows("20", "20", "20")
                    .Layout(Orientation.Vertical, 2)
                    .ScalarItem().At(0, 0)
                    .ScalarItem().At(1, 0)
                    .ScalarItem().At(0, 1)
                    .RowItem().At(1, 1)
                    .ScalarItem(true).At(1, 2);
            });

            var measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(2, layoutManager.BlockViews.Last.Ordinal);

            var scalarItems = layoutManager.Template.ScalarItems;
            Assert.AreEqual(new Rect(0, 0, 20, 20), layoutManager.GetScalarItemRect(scalarItems[0], 0));
            Assert.AreEqual(new Rect(20, 0, 200, 20), layoutManager.GetScalarItemRect(scalarItems[1], 0));
            Assert.AreEqual(new Rect(0, 20, 20, 100), layoutManager.GetScalarItemRect(scalarItems[2], 0));
            Assert.AreEqual(new Rect(20, 120, 100, 20), layoutManager.GetScalarItemRect(scalarItems[3], 0));
            Assert.AreEqual(new Rect(120, 120, 100, 20), layoutManager.GetScalarItemRect(scalarItems[3], 1));
        }

        [TestMethod]
        public void LayoutXYManager_FrozenMain()
        {
            var dataSet = MockProductCategories(9, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("100")
                    .GridRows("10", "10", "20", "10", "10")
                    .Layout(Orientation.Vertical)
                    .FrozenTop(1)
                    .FrozenBottom(1)
                    .ScalarItem().At(0, 0)
                    .ScalarItem().At(0, 1)
                    .RowItem().At(0, 2)
                    .ScalarItem().At(0, 3)
                    .ScalarItem().At(0, 4);
            });

            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(2, layoutManager.BlockViews.Last.Ordinal);

            var scalarItems = layoutManager.Template.ScalarItems;
            var blockViews = layoutManager.BlockViews;

            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetScalarItemRect(scalarItems[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[0], 0));
            Assert.AreEqual(new Rect(0, 10, 100, 10), layoutManager.GetScalarItemRect(scalarItems[1], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[1], 0));
            Assert.AreEqual(new Rect(0, 20, 100, 20), layoutManager.GetBlockRect(blockViews[0]));
            Assert.AreEqual(new Rect(0, 40, 100, 20), layoutManager.GetBlockRect(blockViews[1]));
            Assert.AreEqual(new Rect(0, 60, 100, 20), layoutManager.GetBlockRect(blockViews[2]));
            Assert.AreEqual(new Rect(0, 200, 100, 10), layoutManager.GetScalarItemRect(scalarItems[2], 0));
            Assert.AreEqual(new Thickness(0, 0, 0, double.PositiveInfinity), layoutManager.GetScalarItemClip(scalarItems[2], 0));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetScalarItemRect(scalarItems[3], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[3], 0));

            layoutManager.ScrollOffsetY = 5;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(3, layoutManager.BlockViews.Last.Ordinal);

            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetScalarItemRect(scalarItems[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[0], 0));
            Assert.AreEqual(new Rect(0, 5, 100, 10), layoutManager.GetScalarItemRect(scalarItems[1], 0));
            Assert.AreEqual(new Thickness(0, 5, 0, 0), layoutManager.GetScalarItemClip(scalarItems[1], 0));
            Assert.AreEqual(new Rect(0, 15, 100, 20), layoutManager.GetBlockRect(blockViews[0]));
            Assert.AreEqual(new Rect(0, 35, 100, 20), layoutManager.GetBlockRect(blockViews[1]));
            Assert.AreEqual(new Rect(0, 55, 100, 20), layoutManager.GetBlockRect(blockViews[2]));
            Assert.AreEqual(new Rect(0, 75, 100, 20), layoutManager.GetBlockRect(blockViews[3]));
            Assert.AreEqual(new Rect(0, 195, 100, 10), layoutManager.GetScalarItemRect(scalarItems[2], 0));
            Assert.AreEqual(new Thickness(0, 0, 0, double.PositiveInfinity), layoutManager.GetScalarItemClip(scalarItems[2], 0));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetScalarItemRect(scalarItems[3], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[3], 0));

            layoutManager.ScrollOffsetY = 15;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(4, layoutManager.BlockViews.Last.Ordinal);

            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetScalarItemRect(scalarItems[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[0], 0));
            Assert.AreEqual(new Rect(0, -5, 100, 10), layoutManager.GetScalarItemRect(scalarItems[1], 0));
            Assert.AreEqual(new Thickness(0, double.PositiveInfinity, 0, 0), layoutManager.GetScalarItemClip(scalarItems[1], 0));
            Assert.AreEqual(new Rect(0, 5, 100, 20), layoutManager.GetBlockRect(blockViews[0]));
            Assert.AreEqual(new Rect(0, 25, 100, 20), layoutManager.GetBlockRect(blockViews[1]));
            Assert.AreEqual(new Rect(0, 45, 100, 20), layoutManager.GetBlockRect(blockViews[2]));
            Assert.AreEqual(new Rect(0, 65, 100, 20), layoutManager.GetBlockRect(blockViews[3]));
            Assert.AreEqual(new Rect(0, 85, 100, 20), layoutManager.GetBlockRect(blockViews[4]));
            Assert.AreEqual(new Rect(0, 185, 100, 10), layoutManager.GetScalarItemRect(scalarItems[2], 0));
            Assert.AreEqual(new Thickness(0, 0, 0, double.PositiveInfinity), layoutManager.GetScalarItemClip(scalarItems[2], 0));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetScalarItemRect(scalarItems[3], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[3], 0));

            layoutManager.ScrollOffsetY = 45;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(1, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(5, layoutManager.BlockViews.Last.Ordinal);

            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetScalarItemRect(scalarItems[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[0], 0));
            Assert.AreEqual(new Rect(0, -35, 100, 10), layoutManager.GetScalarItemRect(scalarItems[1], 0));
            Assert.AreEqual(new Thickness(0, double.PositiveInfinity, 0, 0), layoutManager.GetScalarItemClip(scalarItems[1], 0));
            Assert.AreEqual(new Rect(0, -5, 100, 20), layoutManager.GetBlockRect(blockViews[0]));
            Assert.AreEqual(new Rect(0, 15, 100, 20), layoutManager.GetBlockRect(blockViews[1]));
            Assert.AreEqual(new Rect(0, 35, 100, 20), layoutManager.GetBlockRect(blockViews[2]));
            Assert.AreEqual(new Rect(0, 55, 100, 20), layoutManager.GetBlockRect(blockViews[3]));
            Assert.AreEqual(new Rect(0, 75, 100, 20), layoutManager.GetBlockRect(blockViews[4]));
            Assert.AreEqual(new Rect(0, 155, 100, 10), layoutManager.GetScalarItemRect(scalarItems[2], 0));
            Assert.AreEqual(new Thickness(0, 0, 0, double.PositiveInfinity), layoutManager.GetScalarItemClip(scalarItems[2], 0));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetScalarItemRect(scalarItems[3], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[3], 0));
        }

        [TestMethod]
        public void LayoutXYManager_FrozenCross()
        {
            //throw new NotImplementedException();
        }

        [TestMethod]
        public void LayoutXYManager_Stretches()
        {

        }
    }
}
