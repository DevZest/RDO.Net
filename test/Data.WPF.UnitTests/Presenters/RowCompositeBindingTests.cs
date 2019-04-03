using DevZest.Data.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    [TestClass]
    public class RowCompositeBindingTests
    {
        [TestMethod]
        public void RowCompositeBinding_XAML()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            RowBinding<Label> label = null;
            RowBinding<TextBlock> textBlock = null;
            RowCompositeBinding<XamlPane> pane = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                textBlock = _.Name.BindToTextBlock();
                label = _.Name.BindToLabel(textBlock);
                pane = new RowCompositeBinding<XamlPane>().AddChild(label, v => v.Label).AddChild(textBlock, v => v.TextBlock);
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
    }
}
