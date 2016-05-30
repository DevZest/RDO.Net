using DevZest.Data.Windows.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRowItemRect(block[0], rowItem));

            block = layoutManager.BlockViews[1];
            Assert.AreEqual(new Rect(0, 10, 100, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowItemRect(block[0], rowItem));

            block = layoutManager.BlockViews[2];
            Assert.AreEqual(new Rect(0, 30, 100, 30), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRowItemRect(block[0], rowItem));

            block = layoutManager.BlockViews[3];
            Assert.AreEqual(new Rect(0, 60, 100, 40), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowItemRect(block[0], rowItem));

            // Do another measure to detect bugs related to recycled elements
            measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(3, layoutManager.BlockViews.Last.Ordinal);

            rowItem = layoutManager.Template.RowItemGroups[0][0];

            block = layoutManager.BlockViews[0];
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRowItemRect(block[0], rowItem));

            block = layoutManager.BlockViews[1];
            Assert.AreEqual(new Rect(0, 10, 100, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowItemRect(block[0], rowItem));

            block = layoutManager.BlockViews[2];
            Assert.AreEqual(new Rect(0, 30, 100, 30), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRowItemRect(block[0], rowItem));

            block = layoutManager.BlockViews[3];
            Assert.AreEqual(new Rect(0, 60, 100, 40), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowItemRect(block[0], rowItem));
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
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowItemRect(block[0], rowItem));

            block = layoutManager.BlockViews[1];
            Assert.AreEqual(new Rect(0, 20, 200, 40), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(100, 0, 100, 40), layoutManager.GetRowRect(block, 1));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowItemRect(block[0], rowItem));

            block = layoutManager.BlockViews[2];
            Assert.AreEqual(new Rect(0, 60, 200, 60), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 60), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(100, 0, 100, 60), layoutManager.GetRowRect(block, 1));
            Assert.AreEqual(new Rect(0, 0, 100, 60), layoutManager.GetRowItemRect(block[0], rowItem));
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
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blockViews[0]));
            Assert.AreEqual(new Rect(0, 40, 100, 20), layoutManager.GetBlockRect(blockViews[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blockViews[1]));
            Assert.AreEqual(new Rect(0, 60, 100, 20), layoutManager.GetBlockRect(blockViews[2]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blockViews[2]));
            Assert.AreEqual(new Rect(0, 200, 100, 10), layoutManager.GetScalarItemRect(scalarItems[2], 0));
            Assert.AreEqual(new Thickness(0, 0, 0, double.PositiveInfinity), layoutManager.GetScalarItemClip(scalarItems[2], 0));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetScalarItemRect(scalarItems[3], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[3], 0));

            //==============================
            layoutManager.ScrollOffsetY = 5;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(3, layoutManager.BlockViews.Last.Ordinal);

            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetScalarItemRect(scalarItems[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[0], 0));
            Assert.AreEqual(new Rect(0, 5, 100, 10), layoutManager.GetScalarItemRect(scalarItems[1], 0));
            Assert.AreEqual(new Thickness(0, 5, 0, 0), layoutManager.GetScalarItemClip(scalarItems[1], 0));
            Assert.AreEqual(new Rect(0, 15, 100, 20), layoutManager.GetBlockRect(blockViews[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blockViews[0]));
            Assert.AreEqual(new Rect(0, 35, 100, 20), layoutManager.GetBlockRect(blockViews[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blockViews[1]));
            Assert.AreEqual(new Rect(0, 55, 100, 20), layoutManager.GetBlockRect(blockViews[2]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blockViews[2]));
            Assert.AreEqual(new Rect(0, 75, 100, 20), layoutManager.GetBlockRect(blockViews[3]));
            Assert.AreEqual(new Thickness(0, 0, 0, 5), layoutManager.GetBlockClip(blockViews[3]));
            Assert.AreEqual(new Rect(0, 195, 100, 10), layoutManager.GetScalarItemRect(scalarItems[2], 0));
            Assert.AreEqual(new Thickness(0, 0, 0, double.PositiveInfinity), layoutManager.GetScalarItemClip(scalarItems[2], 0));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetScalarItemRect(scalarItems[3], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[3], 0));

            //==============================
            layoutManager.ScrollOffsetY = 15;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(4, layoutManager.BlockViews.Last.Ordinal);

            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetScalarItemRect(scalarItems[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[0], 0));
            Assert.AreEqual(new Rect(0, -5, 100, 10), layoutManager.GetScalarItemRect(scalarItems[1], 0));
            Assert.AreEqual(new Thickness(0, double.PositiveInfinity, 0, 0), layoutManager.GetScalarItemClip(scalarItems[1], 0));
            Assert.AreEqual(new Rect(0, 5, 100, 20), layoutManager.GetBlockRect(blockViews[0]));
            Assert.AreEqual(new Thickness(0, 5, 0, 0), layoutManager.GetBlockClip(blockViews[0]));
            Assert.AreEqual(new Rect(0, 25, 100, 20), layoutManager.GetBlockRect(blockViews[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blockViews[1]));
            Assert.AreEqual(new Rect(0, 45, 100, 20), layoutManager.GetBlockRect(blockViews[2]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blockViews[2]));
            Assert.AreEqual(new Rect(0, 65, 100, 20), layoutManager.GetBlockRect(blockViews[3]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blockViews[3]));
            Assert.AreEqual(new Rect(0, 85, 100, 20), layoutManager.GetBlockRect(blockViews[4]));
            Assert.AreEqual(new Thickness(0, 0, 0, 15), layoutManager.GetBlockClip(blockViews[4]));
            Assert.AreEqual(new Rect(0, 185, 100, 10), layoutManager.GetScalarItemRect(scalarItems[2], 0));
            Assert.AreEqual(new Thickness(0, 0, 0, double.PositiveInfinity), layoutManager.GetScalarItemClip(scalarItems[2], 0));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetScalarItemRect(scalarItems[3], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[3], 0));

            //==============================
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
        public void LayoutXYManager_FrozenCross_ScalarItem()
        {
            var dataSet = MockProductCategories(0, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("10", "50", "50", "10")
                    .GridRows("10", "20")
                    .Layout(Orientation.Vertical)
                    .FrozenLeft(1)
                    .FrozenRight(1)
                    .ScalarItem().At(1, 0)
                    .ScalarItem().At(2, 0)
                    .ScalarItem().At(3, 0)
                    .RowItem().At(0, 1);
            });

            var scalarItems = layoutManager.Template.ScalarItems;

            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(120, layoutManager.ExtentX);
            Assert.AreEqual(100, layoutManager.ExtentY);
            Assert.AreEqual(100, layoutManager.ViewportX);
            Assert.AreEqual(100, layoutManager.ViewportY);
            Assert.AreEqual(0, layoutManager.ScrollOffsetX);
            Assert.AreEqual(0, layoutManager.ScrollOffsetY);
            Assert.AreEqual(new Rect(10, 0, 50, 10), layoutManager.GetScalarItemRect(scalarItems[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[0], 0));
            Assert.AreEqual(new Rect(60, 0, 50, 10), layoutManager.GetScalarItemRect(scalarItems[1], 0));
            Assert.AreEqual(new Thickness(0, 0, 20, 0), layoutManager.GetScalarItemClip(scalarItems[1], 0));
            Assert.AreEqual(new Rect(90, 0, 10, 10), layoutManager.GetScalarItemRect(scalarItems[2], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[2], 0));

            layoutManager.ScrollOffsetX = 10;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(new Rect(0, 0, 50, 10), layoutManager.GetScalarItemRect(scalarItems[0], 0));
            Assert.AreEqual(new Thickness(10, 0, 0, 0), layoutManager.GetScalarItemClip(scalarItems[0], 0));
            Assert.AreEqual(new Rect(50, 0, 50, 10), layoutManager.GetScalarItemRect(scalarItems[1], 0));
            Assert.AreEqual(new Thickness(0, 0, 10, 0), layoutManager.GetScalarItemClip(scalarItems[1], 0));
            Assert.AreEqual(new Rect(90, 0, 10, 10), layoutManager.GetScalarItemRect(scalarItems[2], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetScalarItemClip(scalarItems[2], 0));
        }

        [TestMethod]
        public void LayoutXYManager_Stretches()
        {
            var dataSet = MockProductCategories(0, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("100")
                    .GridRows("10", "20", "10")
                    .Layout(Orientation.Vertical)
                    .FrozenBottom(1)
                    .Stretch(1)
                    .RowItem().At(0, 1)
                    .ScalarItem().At(0, 2);
            });

            var scalarItems = layoutManager.Template.ScalarItems;

            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetScalarItemRect(scalarItems[0], 0));
        }

        [TestMethod]
        public void LayoutXYManager_FrozenCross_Block()
        {
            var dataSet = MockProductCategories(2, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("10", "10", "100", "10", "10")
                    .GridRows("20")
                    .Layout(Orientation.Vertical, 2)
                    .FrozenLeft(1)
                    .FrozenRight(1)
                    .BlockItem().At(1, 0)
                    .RowItem().At(2, 0)
                    .BlockItem().At(3, 0);
            });

            var blocks = layoutManager.BlockViews;
            var blockItems = layoutManager.Template.BlockItems;
            var rowItem = layoutManager.Template.RowItemGroups[0][0];

            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, blocks.First.Ordinal);
            Assert.AreEqual(0, blocks.Last.Ordinal);
            Assert.AreEqual(240, layoutManager.ExtentX);
            Assert.AreEqual(100, layoutManager.ExtentY);
            Assert.AreEqual(100, layoutManager.ViewportX);
            Assert.AreEqual(100, layoutManager.ViewportY);
            Assert.AreEqual(0, layoutManager.ScrollOffsetX);
            Assert.AreEqual(0, layoutManager.ScrollOffsetY);

            Assert.AreEqual(new Rect(10, 0, 220, 20), layoutManager.GetBlockRect(blocks[0]));
            Assert.AreEqual(new Thickness(0, 0, 140, 0), layoutManager.GetBlockClip(blocks[0]));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetBlockItemRect(blocks[0], blockItems[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockItemClip(blocks[0], blockItems[0]));
            Assert.AreEqual(new Rect(10, 0, 100, 20), layoutManager.GetRowRect(blocks[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowClip(0));
            Assert.AreEqual(new Rect(110, 0, 100, 20), layoutManager.GetRowRect(blocks[0], 1));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowClip(1));
            Assert.AreEqual(new Rect(210, 0, 10, 20), layoutManager.GetBlockItemRect(blocks[0], blockItems[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockItemClip(blocks[0], blockItems[1]));

            layoutManager.ScrollOffsetX = 5;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(new Rect(5, 0, 220, 20), layoutManager.GetBlockRect(blocks[0]));
            Assert.AreEqual(new Thickness(5, 0, 135, 0), layoutManager.GetBlockClip(blocks[0]));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetBlockItemRect(blocks[0], blockItems[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockItemClip(blocks[0], blockItems[0]));
            Assert.AreEqual(new Rect(10, 0, 100, 20), layoutManager.GetRowRect(blocks[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowClip(0));
            Assert.AreEqual(new Rect(110, 0, 100, 20), layoutManager.GetRowRect(blocks[0], 1));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowClip(1));
            Assert.AreEqual(new Rect(210, 0, 10, 20), layoutManager.GetBlockItemRect(blocks[0], blockItems[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockItemClip(blocks[0], blockItems[1]));
        }

        [TestMethod]
        public void LayoutXYManager_FrozenCross_BlockItem()
        {
            var dataSet = MockProductCategories(2, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("10", "10", "100", "10", "10")
                    .GridRows("20")
                    .Layout(Orientation.Vertical, 2)
                    .FrozenLeft(1)
                    .FrozenRight(1)
                    .BlockItem().At(0, 0)
                    .BlockItem().At(1, 0)
                    .RowItem().At(2, 0)
                    .BlockItem().At(3, 0)
                    .BlockItem().At(4, 0);
            });

            var blocks = layoutManager.BlockViews;
            var blockItems = layoutManager.Template.BlockItems;

            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, blocks.First.Ordinal);
            Assert.AreEqual(0, blocks.Last.Ordinal);
            Assert.AreEqual(240, layoutManager.ExtentX);
            Assert.AreEqual(100, layoutManager.ExtentY);
            Assert.AreEqual(100, layoutManager.ViewportX);
            Assert.AreEqual(100, layoutManager.ViewportY);
            Assert.AreEqual(0, layoutManager.ScrollOffsetX);
            Assert.AreEqual(0, layoutManager.ScrollOffsetY);

            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetBlockRect(blocks[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blocks[0]));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetBlockItemRect(blocks[0], blockItems[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockItemClip(blocks[0], blockItems[0]));
            Assert.AreEqual(new Rect(10, 0, 10, 20), layoutManager.GetBlockItemRect(blocks[0], blockItems[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockItemClip(blocks[0], blockItems[1]));
            Assert.AreEqual(new Rect(20, 0, 100, 20), layoutManager.GetRowRect(blocks[0], 0));
            Assert.AreEqual(new Thickness(0, 0, 30, 0), layoutManager.GetRowClip(0));
            Assert.AreEqual(new Rect(120, 0, 100, 20), layoutManager.GetRowRect(blocks[0], 1));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetRowClip(1));
            Assert.AreEqual(new Rect(220, 0, 10, 20), layoutManager.GetBlockItemRect(blocks[0], blockItems[2]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetBlockItemClip(blocks[0], blockItems[2]));
            Assert.AreEqual(new Rect(90, 0, 10, 20), layoutManager.GetBlockItemRect(blocks[0], blockItems[3]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockItemClip(blocks[0], blockItems[3]));

            layoutManager.ScrollOffsetX = 5;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetBlockRect(blocks[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blocks[0]));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetBlockItemRect(blocks[0], blockItems[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockItemClip(blocks[0], blockItems[0]));
            Assert.AreEqual(new Rect(5, 0, 10, 20), layoutManager.GetBlockItemRect(blocks[0], blockItems[1]));
            Assert.AreEqual(new Thickness(5, 0, 0, 0), layoutManager.GetBlockItemClip(blocks[0], blockItems[1]));
            Assert.AreEqual(new Rect(15, 0, 100, 20), layoutManager.GetRowRect(blocks[0], 0));
            Assert.AreEqual(new Thickness(0, 0, 25, 0), layoutManager.GetRowClip(0));
            Assert.AreEqual(new Rect(115, 0, 100, 20), layoutManager.GetRowRect(blocks[0], 1));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetRowClip(1));
            Assert.AreEqual(new Rect(215, 0, 10, 20), layoutManager.GetBlockItemRect(blocks[0], blockItems[2]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetBlockItemClip(blocks[0], blockItems[2]));
            Assert.AreEqual(new Rect(90, 0, 10, 20), layoutManager.GetBlockItemRect(blocks[0], blockItems[3]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockItemClip(blocks[0], blockItems[3]));
        }

        [TestMethod]
        public void LayoutXYManager_FrozenCross_RowItem()
        {
            var dataSet = MockProductCategories(2, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("10", "10", "100", "10", "10")
                    .GridRows("20")
                    .Layout(Orientation.Vertical, 2)
                    .FrozenLeft(1)
                    .FrozenRight(1)
                    .RowItem().At(0, 0)
                    .RowItem().At(1, 0)
                    .RowItem().At(2, 0)
                    .RowItem().At(3, 0)
                    .RowItem().At(4, 0);
            });

            var blocks = layoutManager.BlockViews;
            var rowItems = layoutManager.Template.RowItemGroups[0];

            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, blocks.First.Ordinal);
            Assert.AreEqual(0, blocks.Last.Ordinal);
            Assert.AreEqual(280, layoutManager.ExtentX);
            Assert.AreEqual(100, layoutManager.ExtentY);
            Assert.AreEqual(100, layoutManager.ViewportX);
            Assert.AreEqual(100, layoutManager.ViewportY);
            Assert.AreEqual(0, layoutManager.ScrollOffsetX);
            Assert.AreEqual(0, layoutManager.ScrollOffsetY);

            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetBlockRect(blocks[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blocks[0]));
            Assert.AreEqual(new Rect(0, 0, 140, 20), layoutManager.GetRowRect(blocks[0], 0));
            Assert.AreEqual(new Thickness(0, 0, 50, 0), layoutManager.GetRowClip(0));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][0], rowItems[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowItemClip(blocks[0][0], rowItems[0]));
            Assert.AreEqual(new Rect(10, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][0], rowItems[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowItemClip(blocks[0][0], rowItems[1]));
            Assert.AreEqual(new Rect(20, 0, 100, 20), layoutManager.GetRowItemRect(blocks[0][0], rowItems[2]));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowItemClip(blocks[0][0], rowItems[2]));
            Assert.AreEqual(new Rect(120, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][0], rowItems[3]));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowItemClip(blocks[0][0], rowItems[3]));
            Assert.AreEqual(new Rect(130, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][0], rowItems[4]));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowItemClip(blocks[0][0], rowItems[4]));
            Assert.AreEqual(new Rect(90, 0, 10, 20), layoutManager.GetRowRect(blocks[0], 1));
            Assert.AreEqual(new Thickness(0, 0, 0, 0), layoutManager.GetRowClip(1));
            Assert.AreEqual(new Rect(50, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][1], rowItems[0]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetRowItemClip(blocks[0][1], rowItems[0]));
            Assert.AreEqual(new Rect(60, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][1], rowItems[1]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetRowItemClip(blocks[0][1], rowItems[1]));
            Assert.AreEqual(new Rect(70, 0, 100, 20), layoutManager.GetRowItemRect(blocks[0][1], rowItems[2]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetRowItemClip(blocks[0][1], rowItems[2]));
            Assert.AreEqual(new Rect(170, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][1], rowItems[3]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetRowItemClip(blocks[0][1], rowItems[3]));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][1], rowItems[4]));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowItemClip(blocks[0][1], rowItems[4]));

            layoutManager.ScrollOffsetX = 5;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, blocks.First.Ordinal);
            Assert.AreEqual(0, blocks.Last.Ordinal);
            Assert.AreEqual(280, layoutManager.ExtentX);
            Assert.AreEqual(100, layoutManager.ExtentY);
            Assert.AreEqual(100, layoutManager.ViewportX);
            Assert.AreEqual(100, layoutManager.ViewportY);
            Assert.AreEqual(5, layoutManager.ScrollOffsetX);
            Assert.AreEqual(0, layoutManager.ScrollOffsetY);

            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetBlockRect(blocks[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetBlockClip(blocks[0]));
            Assert.AreEqual(new Rect(0, 0, 135, 20), layoutManager.GetRowRect(blocks[0], 0));
            Assert.AreEqual(new Thickness(0, 0, 45, 0), layoutManager.GetRowClip(0));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][0], rowItems[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowItemClip(blocks[0][0], rowItems[0]));
            Assert.AreEqual(new Rect(5, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][0], rowItems[1]));
            Assert.AreEqual(new Thickness(5, 0, 0, 0), layoutManager.GetRowItemClip(blocks[0][0], rowItems[1]));
            Assert.AreEqual(new Rect(15, 0, 100, 20), layoutManager.GetRowItemRect(blocks[0][0], rowItems[2]));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowItemClip(blocks[0][0], rowItems[2]));
            Assert.AreEqual(new Rect(115, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][0], rowItems[3]));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowItemClip(blocks[0][0], rowItems[3]));
            Assert.AreEqual(new Rect(125, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][0], rowItems[4]));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowItemClip(blocks[0][0], rowItems[4]));
            Assert.AreEqual(new Rect(90, 0, 10, 20), layoutManager.GetRowRect(blocks[0], 1));
            Assert.AreEqual(new Thickness(0, 0, 0, 0), layoutManager.GetRowClip(1));
            Assert.AreEqual(new Rect(45, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][1], rowItems[0]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetRowItemClip(blocks[0][1], rowItems[0]));
            Assert.AreEqual(new Rect(55, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][1], rowItems[1]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetRowItemClip(blocks[0][1], rowItems[1]));
            Assert.AreEqual(new Rect(65, 0, 100, 20), layoutManager.GetRowItemRect(blocks[0][1], rowItems[2]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetRowItemClip(blocks[0][1], rowItems[2]));
            Assert.AreEqual(new Rect(165, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][1], rowItems[3]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetRowItemClip(blocks[0][1], rowItems[3]));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetRowItemRect(blocks[0][1], rowItems[4]));
            Assert.AreEqual(new Thickness(), layoutManager.GetRowItemClip(blocks[0][1], rowItems[4]));
        }

        [TestMethod]
        public void LayoutXYManager_GetLineFiguresMain()
        {
            var dataSet = MockProductCategories(6, false);
            var pen = new Pen();
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("10", "10", "10", "10", "10")
                    .GridRows("10", "10", "20", "10", "10")
                    .Layout(Orientation.Vertical)
                    .FrozenTop(1).FrozenBottom(1).Stretch(1)
                    .RowItem().At(0, 2, 4, 2)
                    .GridLineY(new GridPoint(1, 0), 5, pen)
                    .GridLineY(new GridPoint(2, 1), 3, pen)
                    .GridLineY(new GridPoint(3, 1), 2, pen)
                    .GridLineY(new GridPoint(4, 2), 1, pen);
            });

            layoutManager.Measure(new Size(50, 100));
            var gridLineFigures = layoutManager.GridLineFigures.ToArray();
            Assert.AreEqual(4, gridLineFigures.Length);
            Assert.AreEqual(new Point(10, 0), gridLineFigures[0].StartPoint);
            Assert.AreEqual(new Point(10, 100), gridLineFigures[0].EndPoint);
            Assert.AreEqual(new Point(20, 10), gridLineFigures[1].StartPoint);
            Assert.AreEqual(new Point(20, 90), gridLineFigures[1].EndPoint);
            Assert.AreEqual(new Point(30, 10), gridLineFigures[2].StartPoint);
            Assert.AreEqual(new Point(30, 90), gridLineFigures[2].EndPoint);
            Assert.AreEqual(new Point(40, 20), gridLineFigures[3].StartPoint);
            Assert.AreEqual(new Point(40, 90), gridLineFigures[3].EndPoint);

            layoutManager.Measure(new Size(50, 200));   // Stretched
            gridLineFigures = layoutManager.GridLineFigures.ToArray();
            Assert.AreEqual(5, gridLineFigures.Length);
            Assert.AreEqual(new Point(10, 0), gridLineFigures[0].StartPoint);
            Assert.AreEqual(new Point(10, 150), gridLineFigures[0].EndPoint);
            Assert.AreEqual(new Point(10, 190), gridLineFigures[1].StartPoint);
            Assert.AreEqual(new Point(10, 200), gridLineFigures[1].EndPoint);
            Assert.AreEqual(new Point(20, 10), gridLineFigures[2].StartPoint);
            Assert.AreEqual(new Point(20, 150), gridLineFigures[2].EndPoint);
            Assert.AreEqual(new Point(30, 10), gridLineFigures[3].StartPoint);
            Assert.AreEqual(new Point(30, 140), gridLineFigures[3].EndPoint);
            Assert.AreEqual(new Point(40, 20), gridLineFigures[4].StartPoint);
            Assert.AreEqual(new Point(40, 140), gridLineFigures[4].EndPoint);
        }
    }
}
