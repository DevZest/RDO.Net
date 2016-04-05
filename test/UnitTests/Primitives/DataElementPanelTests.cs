using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class DataElementPanelTests
    {
        [TestMethod]
        public void DataElementPanel_Elements()
        {
            var dataView = new DataView();
            dataView.Show(DataSet<Adhoc>.New());

            dataView.RunAfterLoaded(x =>
            {
                var dataElementPanel = x.FindVisualChild<DataElementPanel>();
                Assert.IsTrue(dataElementPanel != null, "Failed to resolve DataElementPanel from control template. Check DataView.Template.");
                Assert.IsTrue(((IElementCollection)dataElementPanel.Elements).Parent == dataElementPanel);
                //Assert.Fail("This error proves the test completed sucessfully.");
            });
        }
    }
}
