using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class LayoutScrollableManagerTests : LayoutManagerTestsBase
    {
        [TestMethod]
        public void LayoutScrollableManager_ScrollInfo()
        {
            var dataSet = DataSetMock.ProductCategories(9, false);
            var _ = dataSet._;
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("100")
                    .GridRows("20")
                    .Layout(Orientation.Vertical)
                    .AddBinding(0, 0, _.RowPlaceholder());
            });

            {
                var measuredSize = layoutManager.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Assert.AreEqual(0, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
                Assert.AreEqual(8, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);
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
                Assert.AreEqual(0, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
                Assert.AreEqual(1, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);
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
                Assert.AreEqual(1, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
                Assert.AreEqual(2, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);
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
                Assert.AreEqual(0, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
                Assert.AreEqual(1, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);
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
        public void LayoutScrollableManager_RowBinding()
        {
            var dataSet = DataSetMock.ProductCategories(9, false);
            var _ = dataSet._;
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("100")
                    .GridRows("Auto")
                    .Layout(Orientation.Vertical)
                    .AddBinding(0, 0, _.RowPlaceholder((v, p) => v.DesiredHeight = 10 * (p.Index + 1)));
            });

            var measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
            Assert.AreEqual(3, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);

            var rowBinding = layoutManager.Template.RowBindings[0];
            ContainerView containerView;

            containerView = layoutManager.InternalContainerViewList[0];
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRect(containerView));
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRect((RowView)containerView, rowBinding));

            containerView = layoutManager.InternalContainerViewList[1];
            Assert.AreEqual(new Rect(0, 10, 100, 20), layoutManager.GetRect(containerView));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRect((RowView)containerView, rowBinding));

            containerView = layoutManager.InternalContainerViewList[2];
            Assert.AreEqual(new Rect(0, 30, 100, 30), layoutManager.GetRect(containerView));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRect((RowView)containerView, rowBinding));

            containerView = layoutManager.InternalContainerViewList[3];
            Assert.AreEqual(new Rect(0, 60, 100, 40), layoutManager.GetRect(containerView));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRect((RowView)containerView, rowBinding));

            // Do another measure to detect bugs related to recycled elements
            measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
            Assert.AreEqual(3, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);

            rowBinding = layoutManager.Template.RowBindings[0];

            containerView = layoutManager.InternalContainerViewList[0];
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRect(containerView));
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRect((RowView)containerView, rowBinding));

            containerView = layoutManager.InternalContainerViewList[1];
            Assert.AreEqual(new Rect(0, 10, 100, 20), layoutManager.GetRect(containerView));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRect((RowView)containerView, rowBinding));

            containerView = layoutManager.InternalContainerViewList[2];
            Assert.AreEqual(new Rect(0, 30, 100, 30), layoutManager.GetRect(containerView));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRect((RowView)containerView, rowBinding));

            containerView = layoutManager.InternalContainerViewList[3];
            Assert.AreEqual(new Rect(0, 60, 100, 40), layoutManager.GetRect(containerView));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRect((RowView)containerView, rowBinding));
        }

        [TestMethod]
        public void LayoutScrollableManager_RowBinding_flowable()
        {
            var dataSet = DataSetMock.ProductCategories(9, false);
            var _ = dataSet._;
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("100")
                    .GridRows("Auto")
                    .Layout(Orientation.Vertical, 2)
                    .AddBinding(0, 0, _.RowPlaceholder((v, p) => v.DesiredHeight = 10 * (p.Index + 1)));
            });

            var measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
            Assert.AreEqual(2, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);

            var rowBinding = layoutManager.Template.RowBindings[0];
            BlockView block;

            block = (BlockView)layoutManager.InternalContainerViewList[0];
            Assert.AreEqual(new Rect(0, 0, 200, 20), layoutManager.GetRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRect(block, 0));
            Assert.AreEqual(new Rect(100, 0, 100, 20), layoutManager.GetRect(block, 1));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRect(block[0].View, rowBinding));

            block = (BlockView)layoutManager.InternalContainerViewList[1];
            Assert.AreEqual(new Rect(0, 20, 200, 40), layoutManager.GetRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRect(block, 0));
            Assert.AreEqual(new Rect(100, 0, 100, 40), layoutManager.GetRect(block, 1));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRect(block[0].View, rowBinding));

            block = (BlockView)layoutManager.InternalContainerViewList[2];
            Assert.AreEqual(new Rect(0, 60, 200, 60), layoutManager.GetRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 60), layoutManager.GetRect(block, 0));
            Assert.AreEqual(new Rect(100, 0, 100, 60), layoutManager.GetRect(block, 1));
            Assert.AreEqual(new Rect(0, 0, 100, 60), layoutManager.GetRect(block[0].View, rowBinding));
        }

        [TestMethod]
        public void LayoutScrollableManager_BlockBinding()
        {
            var dataSet = DataSetMock.ProductCategories(9, false);
            var _ = dataSet._;
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("20", "100", "20")
                    .GridRows("20")
                    .Layout(Orientation.Vertical, 2)
                    .AddBinding(0, 0, _.BlockPlaceholder())
                    .AddBinding(1, 0, _.RowPlaceholder())
                    .AddBinding(2, 0, _.BlockPlaceholder());
            });

            var measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
            Assert.AreEqual(4, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);

            BlockView block;
            var blockBinding0 = layoutManager.Template.BlockBindings[0];
            var blockBinding1 = layoutManager.Template.BlockBindings[1];

            block = (BlockView)layoutManager.InternalContainerViewList[0];
            Assert.AreEqual(new Rect(0, 0, 240, 20), layoutManager.GetRect(block));
            Assert.AreEqual(new Rect(0, 0, 20, 20), layoutManager.GetRect(block, blockBinding0));
            Assert.AreEqual(new Rect(220, 0, 20, 20), layoutManager.GetRect(block, blockBinding1));

            block = (BlockView)layoutManager.InternalContainerViewList[1];
            Assert.AreEqual(new Rect(0, 20, 240, 20), layoutManager.GetRect(block));
            Assert.AreEqual(new Rect(0, 0, 20, 20), layoutManager.GetRect(block, blockBinding0));
            Assert.AreEqual(new Rect(220, 0, 20, 20), layoutManager.GetRect(block, blockBinding1));

            block = (BlockView)layoutManager.InternalContainerViewList[2];
            Assert.AreEqual(new Rect(0, 40, 240, 20), layoutManager.GetRect(block));
            Assert.AreEqual(new Rect(0, 0, 20, 20), layoutManager.GetRect(block, blockBinding0));
            Assert.AreEqual(new Rect(220, 0, 20, 20), layoutManager.GetRect(block, blockBinding1));

            block = (BlockView)layoutManager.InternalContainerViewList[3];
            Assert.AreEqual(new Rect(0, 60, 240, 20), layoutManager.GetRect(block));
            Assert.AreEqual(new Rect(0, 0, 20, 20), layoutManager.GetRect(block, blockBinding0));
            Assert.AreEqual(new Rect(220, 0, 20, 20), layoutManager.GetRect(block, blockBinding1));

            block = (BlockView)layoutManager.InternalContainerViewList[4];
            Assert.AreEqual(new Rect(0, 80, 240, 20), layoutManager.GetRect(block));
            Assert.AreEqual(new Rect(0, 0, 20, 20), layoutManager.GetRect(block, blockBinding0));
            Assert.AreEqual(new Rect(220, 0, 20, 20), layoutManager.GetRect(block, blockBinding1));
        }

        [TestMethod]
        public void LayoutScrollableManager_ScalarBinding()
        {
            var dataSet = DataSetMock.ProductCategories(9, false);
            var _ = dataSet._;
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("20", "100")
                    .GridRows("20", "20", "20")
                    .Layout(Orientation.Vertical, 2)
                    .AddBinding(0, 0, _.ScalarPlaceholder())
                    .AddBinding(1, 0, _.ScalarPlaceholder())
                    .AddBinding(0, 1, _.ScalarPlaceholder())
                    .AddBinding(1, 1, _.RowPlaceholder())
                    .AddBinding(1, 2, _.ScalarPlaceholder().WithFlowable(true));
            });

            var measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
            Assert.AreEqual(3, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);

            var scalarBindings = layoutManager.Template.ScalarBindings;
            Assert.AreEqual(new Rect(0, 0, 20, 20), layoutManager.GetRect(scalarBindings[0], 0));
            Assert.AreEqual(new Rect(20, 0, 200, 20), layoutManager.GetRect(scalarBindings[1], 0));
            Assert.AreEqual(new Rect(0, 20, 20, 100), layoutManager.GetRect(scalarBindings[2], 0));
            Assert.AreEqual(new Rect(20, 120, 100, 20), layoutManager.GetRect(scalarBindings[3], 0));
            Assert.AreEqual(new Rect(120, 120, 100, 20), layoutManager.GetRect(scalarBindings[3], 1));
        }

        [TestMethod]
        public void LayoutScrollableManager_FrozenMain()
        {
            var dataSet = DataSetMock.ProductCategories(9, false);
            var _ = dataSet._;
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("100")
                    .GridRows("10", "10", "20", "10", "10")
                    .Layout(Orientation.Vertical)
                    .WithFrozenTop(1)
                    .WithFrozenBottom(1)
                    .AddBinding(0, 0, _.ScalarPlaceholder())
                    .AddBinding(0, 1, _.ScalarPlaceholder())
                    .AddBinding(0, 2, _.RowPlaceholder())
                    .AddBinding(0, 3, _.ScalarPlaceholder())
                    .AddBinding(0, 4, _.ScalarPlaceholder());
            });

            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
            Assert.AreEqual(3, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);

            var scalarBindings = layoutManager.Template.ScalarBindings;
            var blockViews = layoutManager.InternalContainerViewList;

            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRect(scalarBindings[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(scalarBindings[0], 0));
            Assert.AreEqual(new Rect(0, 10, 100, 10), layoutManager.GetRect(scalarBindings[1], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(scalarBindings[1], 0));
            Assert.AreEqual(new Rect(0, 20, 100, 20), layoutManager.GetRect(blockViews[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blockViews[0]));
            Assert.AreEqual(new Rect(0, 40, 100, 20), layoutManager.GetRect(blockViews[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blockViews[1]));
            Assert.AreEqual(new Rect(0, 60, 100, 20), layoutManager.GetRect(blockViews[2]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blockViews[2]));
            Assert.AreEqual(new Rect(0, 80, 100, 20), layoutManager.GetRect(blockViews[3]));
            Assert.AreEqual(new Thickness(0, 0, 0, 10), layoutManager.GetClip(blockViews[3]));
            Assert.AreEqual(new Rect(0, 200, 100, 10), layoutManager.GetRect(scalarBindings[2], 0));
            Assert.AreEqual(new Thickness(0, 0, 0, double.PositiveInfinity), layoutManager.GetClip(scalarBindings[2], 0));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetRect(scalarBindings[3], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(scalarBindings[3], 0));

            //==============================
            layoutManager.ScrollOffsetY = 5;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
            Assert.AreEqual(3, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);

            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRect(scalarBindings[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(scalarBindings[0], 0));
            Assert.AreEqual(new Rect(0, 5, 100, 10), layoutManager.GetRect(scalarBindings[1], 0));
            Assert.AreEqual(new Thickness(0, 5, 0, 0), layoutManager.GetClip(scalarBindings[1], 0));
            Assert.AreEqual(new Rect(0, 15, 100, 20), layoutManager.GetRect(blockViews[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blockViews[0]));
            Assert.AreEqual(new Rect(0, 35, 100, 20), layoutManager.GetRect(blockViews[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blockViews[1]));
            Assert.AreEqual(new Rect(0, 55, 100, 20), layoutManager.GetRect(blockViews[2]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blockViews[2]));
            Assert.AreEqual(new Rect(0, 75, 100, 20), layoutManager.GetRect(blockViews[3]));
            Assert.AreEqual(new Thickness(0, 0, 0, 5), layoutManager.GetClip(blockViews[3]));
            Assert.AreEqual(new Rect(0, 195, 100, 10), layoutManager.GetRect(scalarBindings[2], 0));
            Assert.AreEqual(new Thickness(0, 0, 0, double.PositiveInfinity), layoutManager.GetClip(scalarBindings[2], 0));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetRect(scalarBindings[3], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(scalarBindings[3], 0));

            //==============================
            layoutManager.ScrollOffsetY = 15;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
            Assert.AreEqual(4, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);

            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRect(scalarBindings[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(scalarBindings[0], 0));
            Assert.AreEqual(new Rect(0, -5, 100, 10), layoutManager.GetRect(scalarBindings[1], 0));
            Assert.AreEqual(new Thickness(0, double.PositiveInfinity, 0, 0), layoutManager.GetClip(scalarBindings[1], 0));
            Assert.AreEqual(new Rect(0, 5, 100, 20), layoutManager.GetRect(blockViews[0]));
            Assert.AreEqual(new Thickness(0, 5, 0, 0), layoutManager.GetClip(blockViews[0]));
            Assert.AreEqual(new Rect(0, 25, 100, 20), layoutManager.GetRect(blockViews[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blockViews[1]));
            Assert.AreEqual(new Rect(0, 45, 100, 20), layoutManager.GetRect(blockViews[2]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blockViews[2]));
            Assert.AreEqual(new Rect(0, 65, 100, 20), layoutManager.GetRect(blockViews[3]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blockViews[3]));
            Assert.AreEqual(new Rect(0, 85, 100, 20), layoutManager.GetRect(blockViews[4]));
            Assert.AreEqual(new Thickness(0, 0, 0, 15), layoutManager.GetClip(blockViews[4]));
            Assert.AreEqual(new Rect(0, 185, 100, 10), layoutManager.GetRect(scalarBindings[2], 0));
            Assert.AreEqual(new Thickness(0, 0, 0, double.PositiveInfinity), layoutManager.GetClip(scalarBindings[2], 0));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetRect(scalarBindings[3], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(scalarBindings[3], 0));

            //==============================
            layoutManager.ScrollOffsetY = 45;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(1, layoutManager.InternalContainerViewList.First.ContainerOrdinal);
            Assert.AreEqual(5, layoutManager.InternalContainerViewList.Last.ContainerOrdinal);

            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRect(scalarBindings[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(scalarBindings[0], 0));
            Assert.AreEqual(new Rect(0, -35, 100, 10), layoutManager.GetRect(scalarBindings[1], 0));
            Assert.AreEqual(new Thickness(0, double.PositiveInfinity, 0, 0), layoutManager.GetClip(scalarBindings[1], 0));
            Assert.AreEqual(new Rect(0, -5, 100, 20), layoutManager.GetRect(blockViews[0]));
            Assert.AreEqual(new Rect(0, 15, 100, 20), layoutManager.GetRect(blockViews[1]));
            Assert.AreEqual(new Rect(0, 35, 100, 20), layoutManager.GetRect(blockViews[2]));
            Assert.AreEqual(new Rect(0, 55, 100, 20), layoutManager.GetRect(blockViews[3]));
            Assert.AreEqual(new Rect(0, 75, 100, 20), layoutManager.GetRect(blockViews[4]));
            Assert.AreEqual(new Rect(0, 155, 100, 10), layoutManager.GetRect(scalarBindings[2], 0));
            Assert.AreEqual(new Thickness(0, 0, 0, double.PositiveInfinity), layoutManager.GetClip(scalarBindings[2], 0));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetRect(scalarBindings[3], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(scalarBindings[3], 0));
        }

        [TestMethod]
        public void LayoutScrollableManager_FrozenCross_ScalarBinding()
        {
            var dataSet = DataSetMock.ProductCategories(0, false);
            var _ = dataSet._;
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("10", "50", "50", "10")
                    .GridRows("10", "20")
                    .Layout(Orientation.Vertical)
                    .WithFrozenLeft(1)
                    .WithFrozenRight(1)
                    .AddBinding(1, 0, _.ScalarPlaceholder())
                    .AddBinding(2, 0, _.ScalarPlaceholder())
                    .AddBinding(3, 0, _.ScalarPlaceholder())
                    .AddBinding(0, 1, _.RowPlaceholder());
            });

            var scalarBindings = layoutManager.Template.ScalarBindings;

            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(120, layoutManager.ExtentX);
            Assert.AreEqual(100, layoutManager.ExtentY);
            Assert.AreEqual(100, layoutManager.ViewportX);
            Assert.AreEqual(100, layoutManager.ViewportY);
            Assert.AreEqual(0, layoutManager.ScrollOffsetX);
            Assert.AreEqual(0, layoutManager.ScrollOffsetY);
            Assert.AreEqual(new Rect(10, 0, 50, 10), layoutManager.GetRect(scalarBindings[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(scalarBindings[0], 0));
            Assert.AreEqual(new Rect(60, 0, 50, 10), layoutManager.GetRect(scalarBindings[1], 0));
            Assert.AreEqual(new Thickness(0, 0, 20, 0), layoutManager.GetClip(scalarBindings[1], 0));
            Assert.AreEqual(new Rect(90, 0, 10, 10), layoutManager.GetRect(scalarBindings[2], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(scalarBindings[2], 0));

            layoutManager.ScrollOffsetX = 10;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(new Rect(0, 0, 50, 10), layoutManager.GetRect(scalarBindings[0], 0));
            Assert.AreEqual(new Thickness(10, 0, 0, 0), layoutManager.GetClip(scalarBindings[0], 0));
            Assert.AreEqual(new Rect(50, 0, 50, 10), layoutManager.GetRect(scalarBindings[1], 0));
            Assert.AreEqual(new Thickness(0, 0, 10, 0), layoutManager.GetClip(scalarBindings[1], 0));
            Assert.AreEqual(new Rect(90, 0, 10, 10), layoutManager.GetRect(scalarBindings[2], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(scalarBindings[2], 0));
        }

        [TestMethod]
        public void LayoutScrollableManager_Stretches()
        {
            var dataSet = DataSetMock.ProductCategories(0, false);
            var _ = dataSet._;
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("100")
                    .GridRows("10", "20", "10")
                    .Layout(Orientation.Vertical)
                    .WithFrozenBottom(1)
                    .WithStretches(1)
                    .AddBinding(0, 1, _.RowPlaceholder())
                    .AddBinding(0, 2, _.ScalarPlaceholder());
            });

            var scalarBindings = layoutManager.Template.ScalarBindings;

            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(new Rect(0, 90, 100, 10), layoutManager.GetRect(scalarBindings[0], 0));
        }

        [TestMethod]
        public void LayoutScrollableManager_FrozenCross_Block()
        {
            var dataSet = DataSetMock.ProductCategories(2, false);
            var _ = dataSet._;
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("10", "10", "100", "10", "10")
                    .GridRows("20")
                    .Layout(Orientation.Vertical, 2)
                    .WithFrozenLeft(1)
                    .WithFrozenRight(1)
                    .AddBinding(1, 0, _.BlockPlaceholder())
                    .AddBinding(2, 0, _.RowPlaceholder())
                    .AddBinding(3, 0, _.BlockPlaceholder());
            });

            var blocks = layoutManager.InternalContainerViewList;
            var blockBindings = layoutManager.Template.BlockBindings;
            var rowBinding = layoutManager.Template.RowBindings[0];

            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, blocks.First.ContainerOrdinal);
            Assert.AreEqual(0, blocks.Last.ContainerOrdinal);
            Assert.AreEqual(240, layoutManager.ExtentX);
            Assert.AreEqual(100, layoutManager.ExtentY);
            Assert.AreEqual(100, layoutManager.ViewportX);
            Assert.AreEqual(100, layoutManager.ViewportY);
            Assert.AreEqual(0, layoutManager.ScrollOffsetX);
            Assert.AreEqual(0, layoutManager.ScrollOffsetY);

            Assert.AreEqual(new Rect(10, 0, 220, 20), layoutManager.GetRect(blocks[0]));
            Assert.AreEqual(new Thickness(0, 0, 140, 0), layoutManager.GetClip(blocks[0]));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], blockBindings[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip((BlockView)blocks[0], blockBindings[0]));
            Assert.AreEqual(new Rect(10, 0, 100, 20), layoutManager.GetRect((BlockView)blocks[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(0));
            Assert.AreEqual(new Rect(110, 0, 100, 20), layoutManager.GetRect((BlockView)blocks[0], 1));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(1));
            Assert.AreEqual(new Rect(210, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], blockBindings[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip((BlockView)blocks[0], blockBindings[1]));

            layoutManager.ScrollOffsetX = 5;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(new Rect(5, 0, 220, 20), layoutManager.GetRect(blocks[0]));
            Assert.AreEqual(new Thickness(5, 0, 135, 0), layoutManager.GetClip(blocks[0]));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], blockBindings[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip((BlockView)blocks[0], blockBindings[0]));
            Assert.AreEqual(new Rect(10, 0, 100, 20), layoutManager.GetRect((BlockView)blocks[0], 0));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(0));
            Assert.AreEqual(new Rect(110, 0, 100, 20), layoutManager.GetRect((BlockView)blocks[0], 1));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(1));
            Assert.AreEqual(new Rect(210, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], blockBindings[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip((BlockView)blocks[0], blockBindings[1]));
        }

        [TestMethod]
        public void LayoutScrollableManager_FrozenCross_BlockBinding()
        {
            var dataSet = DataSetMock.ProductCategories(2, false);
            var _ = dataSet._;
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("10", "10", "100", "10", "10")
                    .GridRows("20")
                    .Layout(Orientation.Vertical, 2)
                    .WithFrozenLeft(1)
                    .WithFrozenRight(1)
                    .AddBinding(0, 0, _.BlockPlaceholder())
                    .AddBinding(1, 0, _.BlockPlaceholder())
                    .AddBinding(2, 0, _.RowPlaceholder())
                    .AddBinding(3, 0, _.BlockPlaceholder())
                    .AddBinding(4, 0, _.BlockPlaceholder());
            });

            var blocks = layoutManager.InternalContainerViewList;
            var blockBindings = layoutManager.Template.BlockBindings;

            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, blocks.First.ContainerOrdinal);
            Assert.AreEqual(0, blocks.Last.ContainerOrdinal);
            Assert.AreEqual(240, layoutManager.ExtentX);
            Assert.AreEqual(100, layoutManager.ExtentY);
            Assert.AreEqual(100, layoutManager.ViewportX);
            Assert.AreEqual(100, layoutManager.ViewportY);
            Assert.AreEqual(0, layoutManager.ScrollOffsetX);
            Assert.AreEqual(0, layoutManager.ScrollOffsetY);

            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRect(blocks[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blocks[0]));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], blockBindings[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip((BlockView)blocks[0], blockBindings[0]));
            Assert.AreEqual(new Rect(10, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], blockBindings[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip((BlockView)blocks[0], blockBindings[1]));
            Assert.AreEqual(new Rect(20, 0, 100, 20), layoutManager.GetRect((BlockView)blocks[0], 0));
            Assert.AreEqual(new Thickness(0, 0, 30, 0), layoutManager.GetClip(0));
            Assert.AreEqual(new Rect(120, 0, 100, 20), layoutManager.GetRect((BlockView)blocks[0], 1));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetClip(1));
            Assert.AreEqual(new Rect(220, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], blockBindings[2]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetClip((BlockView)blocks[0], blockBindings[2]));
            Assert.AreEqual(new Rect(90, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], blockBindings[3]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip((BlockView)blocks[0], blockBindings[3]));

            layoutManager.ScrollOffsetX = 5;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRect(blocks[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blocks[0]));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], blockBindings[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip((BlockView)blocks[0], blockBindings[0]));
            Assert.AreEqual(new Rect(5, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], blockBindings[1]));
            Assert.AreEqual(new Thickness(5, 0, 0, 0), layoutManager.GetClip((BlockView)blocks[0], blockBindings[1]));
            Assert.AreEqual(new Rect(15, 0, 100, 20), layoutManager.GetRect((BlockView)blocks[0], 0));
            Assert.AreEqual(new Thickness(0, 0, 25, 0), layoutManager.GetClip(0));
            Assert.AreEqual(new Rect(115, 0, 100, 20), layoutManager.GetRect((BlockView)blocks[0], 1));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetClip(1));
            Assert.AreEqual(new Rect(215, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], blockBindings[2]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetClip((BlockView)blocks[0], blockBindings[2]));
            Assert.AreEqual(new Rect(90, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], blockBindings[3]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip((BlockView)blocks[0], blockBindings[3]));
        }

        [TestMethod]
        public void LayoutScrollableManager_FrozenCross_RowBinding()
        {
            var dataSet = DataSetMock.ProductCategories(2, false);
            var _ = dataSet._;
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("10", "10", "100", "10", "10")
                    .GridRows("20")
                    .Layout(Orientation.Vertical, 2)
                    .WithFrozenLeft(1)
                    .WithFrozenRight(1)
                    .AddBinding(0, 0, _.RowPlaceholder())
                    .AddBinding(1, 0, _.RowPlaceholder())
                    .AddBinding(2, 0, _.RowPlaceholder())
                    .AddBinding(3, 0, _.RowPlaceholder())
                    .AddBinding(4, 0, _.RowPlaceholder());
            });

            var blocks = layoutManager.InternalContainerViewList;
            var rowBindings = layoutManager.Template.RowBindings;

            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, blocks.First.ContainerOrdinal);
            Assert.AreEqual(0, blocks.Last.ContainerOrdinal);
            Assert.AreEqual(280, layoutManager.ExtentX);
            Assert.AreEqual(100, layoutManager.ExtentY);
            Assert.AreEqual(100, layoutManager.ViewportX);
            Assert.AreEqual(100, layoutManager.ViewportY);
            Assert.AreEqual(0, layoutManager.ScrollOffsetX);
            Assert.AreEqual(0, layoutManager.ScrollOffsetY);

            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRect(blocks[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blocks[0]));
            Assert.AreEqual(new Rect(0, 0, 140, 20), layoutManager.GetRect((BlockView)blocks[0], 0));
            Assert.AreEqual(new Thickness(0, 0, 50, 0), layoutManager.GetClip(0));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[0].View, rowBindings[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(((BlockView)blocks[0])[0].View, rowBindings[0]));
            Assert.AreEqual(new Rect(10, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[0].View, rowBindings[1]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(((BlockView)blocks[0])[0].View, rowBindings[1]));
            Assert.AreEqual(new Rect(20, 0, 100, 20), layoutManager.GetRect(((BlockView)blocks[0])[0].View, rowBindings[2]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(((BlockView)blocks[0])[0].View, rowBindings[2]));
            Assert.AreEqual(new Rect(120, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[0].View, rowBindings[3]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(((BlockView)blocks[0])[0].View, rowBindings[3]));
            Assert.AreEqual(new Rect(130, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[0].View, rowBindings[4]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(((BlockView)blocks[0])[0].View, rowBindings[4]));
            Assert.AreEqual(new Rect(90, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], 1));
            Assert.AreEqual(new Thickness(0, 0, 0, 0), layoutManager.GetClip(1));
            Assert.AreEqual(new Rect(50, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[1].View, rowBindings[0]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetClip(((BlockView)blocks[0])[1].View, rowBindings[0]));
            Assert.AreEqual(new Rect(60, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[1].View, rowBindings[1]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetClip(((BlockView)blocks[0])[1].View, rowBindings[1]));
            Assert.AreEqual(new Rect(70, 0, 100, 20), layoutManager.GetRect(((BlockView)blocks[0])[1].View, rowBindings[2]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetClip(((BlockView)blocks[0])[1].View, rowBindings[2]));
            Assert.AreEqual(new Rect(170, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[1].View, rowBindings[3]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetClip(((BlockView)blocks[0])[1].View, rowBindings[3]));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[1].View, rowBindings[4]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(((BlockView)blocks[0])[1].View, rowBindings[4]));

            layoutManager.ScrollOffsetX = 5;
            layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, blocks.First.ContainerOrdinal);
            Assert.AreEqual(0, blocks.Last.ContainerOrdinal);
            Assert.AreEqual(280, layoutManager.ExtentX);
            Assert.AreEqual(100, layoutManager.ExtentY);
            Assert.AreEqual(100, layoutManager.ViewportX);
            Assert.AreEqual(100, layoutManager.ViewportY);
            Assert.AreEqual(5, layoutManager.ScrollOffsetX);
            Assert.AreEqual(0, layoutManager.ScrollOffsetY);

            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRect(blocks[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(blocks[0]));
            Assert.AreEqual(new Rect(0, 0, 135, 20), layoutManager.GetRect((BlockView)blocks[0], 0));
            Assert.AreEqual(new Thickness(0, 0, 45, 0), layoutManager.GetClip(0));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[0].View, rowBindings[0]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(((BlockView)blocks[0])[0].View, rowBindings[0]));
            Assert.AreEqual(new Rect(5, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[0].View, rowBindings[1]));
            Assert.AreEqual(new Thickness(5, 0, 0, 0), layoutManager.GetClip(((BlockView)blocks[0])[0].View, rowBindings[1]));
            Assert.AreEqual(new Rect(15, 0, 100, 20), layoutManager.GetRect(((BlockView)blocks[0])[0].View, rowBindings[2]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(((BlockView)blocks[0])[0].View, rowBindings[2]));
            Assert.AreEqual(new Rect(115, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[0].View, rowBindings[3]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(((BlockView)blocks[0])[0].View, rowBindings[3]));
            Assert.AreEqual(new Rect(125, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[0].View, rowBindings[4]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(((BlockView)blocks[0])[0].View, rowBindings[4]));
            Assert.AreEqual(new Rect(90, 0, 10, 20), layoutManager.GetRect((BlockView)blocks[0], 1));
            Assert.AreEqual(new Thickness(0, 0, 0, 0), layoutManager.GetClip(1));
            Assert.AreEqual(new Rect(45, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[1].View, rowBindings[0]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetClip(((BlockView)blocks[0])[1].View, rowBindings[0]));
            Assert.AreEqual(new Rect(55, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[1].View, rowBindings[1]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetClip(((BlockView)blocks[0])[1].View, rowBindings[1]));
            Assert.AreEqual(new Rect(65, 0, 100, 20), layoutManager.GetRect(((BlockView)blocks[0])[1].View, rowBindings[2]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetClip(((BlockView)blocks[0])[1].View, rowBindings[2]));
            Assert.AreEqual(new Rect(165, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[1].View, rowBindings[3]));
            Assert.AreEqual(new Thickness(0, 0, double.PositiveInfinity, 0), layoutManager.GetClip(((BlockView)blocks[0])[1].View, rowBindings[3]));
            Assert.AreEqual(new Rect(0, 0, 10, 20), layoutManager.GetRect(((BlockView)blocks[0])[1].View, rowBindings[4]));
            Assert.AreEqual(new Thickness(), layoutManager.GetClip(((BlockView)blocks[0])[1].View, rowBindings[4]));
        }

        [TestMethod]
        public void LayoutScrollableManager_GetLineFiguresMain_Spans()
        {
            var dataSet = DataSetMock.ProductCategories(6, false);
            var _ = dataSet._;
            var pen = new Pen();
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("10", "10", "10", "10", "10")
                    .GridRows("10", "10", "20", "10", "10")
                    .Layout(Orientation.Vertical)
                    .WithFrozenTop(1).WithFrozenBottom(1).WithStretches(1)
                    .AddBinding(0, 2, 4, 2, _.RowPlaceholder())
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

        [TestMethod]
        public void LayoutScrollableManager_GetLineFiguresMain_Locations()
        {
            var dataSet = DataSetMock.ProductCategories(3, false);
            var _ = dataSet._;
            var pen = new Pen();
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("10", "20", "10")
                    .GridRows("10")
                    .Layout(Orientation.Vertical, 3)
                    .WithFrozenLeft(1)
                    .AddBinding(1, 0, _.RowPlaceholder())
                    .GridLineY(new GridPoint(1, 0), 1, pen, GridLinePosition.PreviousTrack)
                    .GridLineY(new GridPoint(2, 0), 1, pen);
            });

            layoutManager.Measure(new Size(100, 100));
            var gridLineFigures = layoutManager.GridLineFigures.ToArray();
            Assert.AreEqual(4, gridLineFigures.Length);
            Assert.AreEqual(10, gridLineFigures[0].StartPoint.X);
            Assert.AreEqual(30, gridLineFigures[1].StartPoint.X);
            Assert.AreEqual(50, gridLineFigures[2].StartPoint.X);
            Assert.AreEqual(70, gridLineFigures[3].StartPoint.X);

            layoutManager.Measure(new Size(70, 100));
            layoutManager.ScrollOffsetX = 10;
            layoutManager.Measure(new Size(70, 100));
            Assert.AreEqual(10, layoutManager.ScrollOffsetX);
            gridLineFigures = layoutManager.GridLineFigures.ToArray();
            Assert.AreEqual(4, gridLineFigures.Length);
            Assert.AreEqual(10, gridLineFigures[0].StartPoint.X);
            Assert.AreEqual(20, gridLineFigures[1].StartPoint.X);
            Assert.AreEqual(40, gridLineFigures[2].StartPoint.X);
            Assert.AreEqual(60, gridLineFigures[3].StartPoint.X);

            layoutManager.Measure(new Size(40, 100));
            layoutManager.ScrollOffsetX = 40;
            layoutManager.Measure(new Size(40, 100));
            Assert.AreEqual(40, layoutManager.ScrollOffsetX);
            gridLineFigures = layoutManager.GridLineFigures.ToArray();
            Assert.AreEqual(2, gridLineFigures.Length);
            Assert.AreEqual(10, gridLineFigures[0].StartPoint.X);
            Assert.AreEqual(30, gridLineFigures[1].StartPoint.X);
        }

        [TestMethod]
        public void LayoutScrollableManager_GetLineFiguresCross_Span()
        {
            var dataSet = DataSetMock.ProductCategories(3, false);
            var _ = dataSet._;
            var pen = new Pen();
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("10", "20", "10")
                    .GridRows("10")
                    .Layout(Orientation.Vertical, 3)
                    .WithFrozenLeft(1)
                    .AddBinding(1, 0, _.RowPlaceholder())
                    .GridLineX(new GridPoint(0, 0), 3, pen)
                    .GridLineX(new GridPoint(1, 1), 2, pen);
            });

            layoutManager.Measure(new Size(100, 100));
            var gridLineFigures = layoutManager.GridLineFigures.ToArray();
            Assert.AreEqual(2, gridLineFigures.Length);
            Assert.AreEqual(new Point(0, 0), gridLineFigures[0].StartPoint);
            Assert.AreEqual(new Point(80, 0), gridLineFigures[0].EndPoint);
            Assert.AreEqual(new Point(10, 10), gridLineFigures[1].StartPoint);
            Assert.AreEqual(new Point(80, 10), gridLineFigures[1].EndPoint);

            layoutManager.Measure(new Size(70, 100));
            layoutManager.ScrollOffsetX = 10;
            layoutManager.Measure(new Size(70, 100));
            Assert.AreEqual(10, layoutManager.ScrollOffsetX);
            gridLineFigures = layoutManager.GridLineFigures.ToArray();
            Assert.AreEqual(2, gridLineFigures.Length);
            Assert.AreEqual(new Point(0, 0), gridLineFigures[0].StartPoint);
            Assert.AreEqual(new Point(70, 0), gridLineFigures[0].EndPoint);
            Assert.AreEqual(new Point(10, 10), gridLineFigures[1].StartPoint);
            Assert.AreEqual(new Point(70, 10), gridLineFigures[1].EndPoint);
        }

        [TestMethod]
        public void LayoutScrollableManager_GetLineFiguresCross_Locations()
        {
            var dataSet = DataSetMock.ProductCategories(9, false);
            var _ = dataSet._;
            var pen = new Pen();
            var layoutManager = (LayoutScrollableManager)dataSet.CreateLayoutManager((builder) =>
            {
                builder.GridColumns("100")
                    .GridRows("10", "10", "20", "10", "10")
                    .Layout(Orientation.Vertical)
                    .WithFrozenTop(1)
                    .WithFrozenBottom(1)
                    .WithStretches(1)
                    .AddBinding(0, 2, _.RowPlaceholder())
                    .GridLineX(new GridPoint(0, 1), 1, pen)
                    .GridLineX(new GridPoint(0, 2), 1, pen, GridLinePosition.PreviousTrack)
                    .GridLineX(new GridPoint(0, 3), 1, pen)
                    .GridLineX(new GridPoint(0, 4), 1, pen);
            });

            layoutManager.Measure(new Size(100, 100));
            var gridLineFigures = layoutManager.GridLineFigures.ToArray();
            Assert.AreEqual(6, gridLineFigures.Length);
            Assert.AreEqual(10, gridLineFigures[0].StartPoint.Y);
            Assert.AreEqual(20, gridLineFigures[1].StartPoint.Y);
            Assert.AreEqual(40, gridLineFigures[2].StartPoint.Y);
            Assert.AreEqual(60, gridLineFigures[3].StartPoint.Y);
            Assert.AreEqual(80, gridLineFigures[4].StartPoint.Y);
            Assert.AreEqual(90, gridLineFigures[5].StartPoint.Y);

            layoutManager.ScrollOffsetY = 15;
            layoutManager.Measure(new Size(100, 100));
            gridLineFigures = layoutManager.GridLineFigures.ToArray();
            Assert.AreEqual(6, gridLineFigures.Length);
            Assert.AreEqual(10, gridLineFigures[0].StartPoint.Y);
            Assert.AreEqual(25, gridLineFigures[1].StartPoint.Y);
            Assert.AreEqual(45, gridLineFigures[2].StartPoint.Y);
            Assert.AreEqual(65, gridLineFigures[3].StartPoint.Y);
            Assert.AreEqual(85, gridLineFigures[4].StartPoint.Y);
            Assert.AreEqual(90, gridLineFigures[5].StartPoint.Y);

            layoutManager.ScrollOffsetY = 0;
            layoutManager.Measure(new Size(100, 300));  // Stretched
            Assert.AreEqual(0, layoutManager.ScrollOffsetY);
            Assert.AreEqual(9, layoutManager.InternalContainerViewList.Count);
            gridLineFigures = layoutManager.GridLineFigures.ToArray();
            Assert.AreEqual(13, gridLineFigures.Length);
            Assert.AreEqual(10, gridLineFigures[0].StartPoint.Y);
            Assert.AreEqual(20, gridLineFigures[1].StartPoint.Y);
            Assert.AreEqual(40, gridLineFigures[2].StartPoint.Y);   // end of block1
            Assert.AreEqual(60, gridLineFigures[3].StartPoint.Y);   // end of block2
            Assert.AreEqual(80, gridLineFigures[4].StartPoint.Y);   // end of block3
            Assert.AreEqual(100, gridLineFigures[5].StartPoint.Y);  // end of block4
            Assert.AreEqual(120, gridLineFigures[6].StartPoint.Y);  // end of block5
            Assert.AreEqual(140, gridLineFigures[7].StartPoint.Y);  // end of block6
            Assert.AreEqual(160, gridLineFigures[8].StartPoint.Y);  // end of block7
            Assert.AreEqual(180, gridLineFigures[9].StartPoint.Y);  // end of block8
            Assert.AreEqual(200, gridLineFigures[10].StartPoint.Y); // end of block9
            Assert.AreEqual(210, gridLineFigures[11].StartPoint.Y);
            Assert.AreEqual(290, gridLineFigures[12].StartPoint.Y);
        }
    }
}