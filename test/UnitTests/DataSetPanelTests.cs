using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class DataSetPanelTests
    {
        [TestMethod]
        public void DataSetPanel_properly_initialized()
        {
            var dataSetView = new DataSetView();
            dataSetView.Show(DataSetPresenter.Create(DataSet<Adhoc>.New()));

            dataSetView.RunAfterLoaded(x =>
            {
                var layoutManager = dataSetView.Presenter.LayoutManager;
                var dataSetPanel = x.FindVisualChild<DataSetPanel>();
                Assert.IsTrue(dataSetPanel != null, "Failed to resolve DataSetPanel from control template.");
                Verify(dataSetPanel, layoutManager.ScrollableElements);
                Assert.IsTrue(dataSetPanel.Child == null);
            });
        }

        [TestMethod]
        public void DataSetPanel_properly_initialized_pinned()
        {
            var dataSetView = new DataSetView();
            dataSetView.Show(DataSetPresenter.Create(DataSet<Adhoc>.New(), (c, m) =>
            {
                c.WithPinnedLeft(1);
            }));

            dataSetView.RunAfterLoaded(x =>
            {
                var layoutManager = dataSetView.Presenter.LayoutManager;
                var dataSetPanel = x.FindVisualChild<DataSetPanel>();
                Assert.IsTrue(dataSetPanel != null, "Failed to resolve DataSetPanel from control template.");
                Verify(dataSetPanel, layoutManager.PinnedElements);
                Verify(dataSetPanel.Child, layoutManager.ScrollableElements);
            });
        }

        private static void Verify(DataSetPanel dataSetPanel, IReadOnlyList<UIElement> elements)
        {
            Assert.IsTrue(dataSetPanel.Elements == elements);
            Assert.IsTrue(((IElementCollection)elements).Parent == dataSetPanel);
        }
    }
}
