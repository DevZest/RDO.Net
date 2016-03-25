using DevZest.Data.Windows.Primitives;
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
    public class DataViewTests
    {
        [TestMethod]
        public void DataView_properly_initialized()
        {
            var dataView = new DataView();
            dataView.Show(DataSet<Adhoc>.New());

            dataView.RunAfterLoaded(x =>
            {
                var dataPanel = x.FindVisualChild<DataPanel>();
                Assert.IsTrue(dataPanel != null, "Failed to resolve DataSetPanel from control template.");
                Verify(dataPanel, dataView.DataPresenter.Elements);
            });
        }

        private static void Verify(DataPanel dataPanel, IReadOnlyList<UIElement> elements)
        {
            Assert.IsNotNull(dataPanel.Elements);
            Assert.IsTrue(dataPanel.Elements == elements);
            //Assert.IsTrue(((IElementCollection)elements).Parent == dataPanel);
        }
    }
}
