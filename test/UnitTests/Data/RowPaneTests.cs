using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Windows.Data
{
    [TestClass]
    public class RowPaneTests
    {
        [TestMethod]
        public void RowPane_XAML()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            RowBinding<Label> label = null;
            RowBinding<TextBlock> textBlock = null;
            RowPane<XamlPane> pane = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                textBlock = _.Name.TextBlock();
                label = _.Name.Label(textBlock);
                pane = new RowPane<XamlPane>().AddChild(label, XamlPane.NAME_LEFT).AddChild(textBlock, XamlPane.NAME_RIGHT);
                builder.GridRows("100").GridColumns("100").AddBinding(0, 0, pane);
            });

            Assert.IsNull(label.SettingUpElement);
            Assert.IsNull(textBlock.SettingUpElement);
            Assert.IsNull(pane.GetSettingUpElement());

            var currentRow = elementManager.Rows[0];
            Assert.AreEqual(pane[currentRow].Children[0], label[currentRow]);
            Assert.AreEqual(pane[currentRow].Children[1], textBlock[currentRow]);
            Assert.AreEqual(_.Name.DisplayName, label[currentRow].Content);
            Assert.AreEqual(currentRow.GetValue(_.Name), textBlock[currentRow].Text);
            Assert.AreEqual(textBlock[currentRow], label[currentRow].Target);
        }

        [TestMethod]
        public void RowPane_code()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            RowBinding<Label> label = null;
            RowBinding<TextBlock> textBlock = null;
            RowPane<CodePane> pane = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                textBlock = _.Name.TextBlock();
                label = _.Name.Label(textBlock);
                pane = new RowPane<CodePane>().AddChild(label, XamlPane.NAME_LEFT).AddChild(textBlock, XamlPane.NAME_RIGHT);
                builder.GridRows("100").GridColumns("100").AddBinding(0, 0, pane);
            });

            var currentRow = elementManager.Rows[0];
            Assert.AreEqual(pane[currentRow].Children[0], label[currentRow]);
            Assert.AreEqual(pane[currentRow].Children[1], textBlock[currentRow]);
            Assert.AreEqual(_.Name.DisplayName, label[currentRow].Content);
            Assert.AreEqual(currentRow.GetValue(_.Name), textBlock[currentRow].Text);
            Assert.AreEqual(textBlock[currentRow], label[currentRow].Target);
        }
    }
}
