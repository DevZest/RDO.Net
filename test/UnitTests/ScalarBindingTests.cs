using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class ScalarBindingTests
    {
        [TestMethod]
        public void ScalarBinding()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            ScalarBinding<Label> label = null;
            ScalarBinding<ColumnHeader> columnHeader = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                columnHeader = _.Name.ColumnHeader();
                label = _.Name.ScalarLabel(columnHeader);
                builder.Layout(Orientation.Vertical, 2)
                    .GridColumns("100", "100", "100").GridRows("100")
                    .AddBinding(0, 0, label)
                    .AddBinding(1, 0, columnHeader)
                    .AddBinding(2, 0, _.Name.TextBlock());
            });

            Assert.IsNull(label.SettingUpElement);
            Assert.IsNull(columnHeader.SettingUpElement);

            var currentRow = elementManager.Rows[0];
            Assert.AreEqual(_.Name.DisplayName, label[0].Content);
            Assert.AreEqual(_.Name, columnHeader[0].Column);
            Assert.AreEqual(columnHeader[0], label[0].Target);
        }
    }
}
