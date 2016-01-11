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
    public class DataPanelTests
    {
        [TestMethod]
        public void DataPanel_properly_initialized()
        {
            var dataForm = new DataForm();
            dataForm.Show(DataView.Create(DataSet<Adhoc>.New()));

            dataForm.RunAfterLoaded(x =>
            {
                var layoutManager = dataForm.View.LayoutManager;
                var dataSetPanel = x.FindVisualChild<DataPanel>();
                Assert.IsTrue(dataSetPanel != null, "Failed to resolve DataSetPanel from control template.");
                Verify(dataSetPanel, layoutManager.ScrollableElements);
                Assert.IsTrue(dataSetPanel.Child == null);
            });
        }

        [TestMethod]
        public void DataPanel_properly_initialized_pinned()
        {
            var dataForm = new DataForm();
            dataForm.Show(DataView.Create(DataSet<Adhoc>.New(), (c, m) =>
            {
                c.WithPinnedLeft(1);
            }));

            dataForm.RunAfterLoaded(x =>
            {
                var layoutManager = dataForm.View.LayoutManager;
                var dataSetPanel = x.FindVisualChild<DataPanel>();
                Assert.IsTrue(dataSetPanel != null, "Failed to resolve DataSetPanel from control template.");
                Verify(dataSetPanel, layoutManager.PinnedElements);
                Verify(dataSetPanel.Child, layoutManager.ScrollableElements);
            });
        }

        private static void Verify(DataPanel dataPanel, IReadOnlyList<UIElement> elements)
        {
            Assert.IsTrue(dataPanel.Elements == elements);
            Assert.IsTrue(((IElementCollection)elements).Parent == dataPanel);
        }
    }
}
