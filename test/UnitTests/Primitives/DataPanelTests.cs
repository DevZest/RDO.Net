using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class DataPanelTests
    {
        [TestMethod]
        public void DataPanel_Elements()
        {
            var dataView = new DataView();
            dataView.Show(DataSet<Adhoc>.New());

            dataView.RunAfterLoaded(x =>
            {
                var dataPanel = x.FindVisualChild<DataPanel>();
                Assert.IsTrue(dataPanel != null, "Failed to resolve DataPanel from control template. Check DataView.Template.");
                Assert.IsTrue(((IElementCollection)dataPanel.Elements).Parent == dataPanel);
                //Assert.Fail("This error proves the test completed sucessfully.");
            });
        }
    }
}
