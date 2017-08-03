using DevZest.Data.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    [TestClass]
    public class CompositeRowBindingTests
    {
        [TestMethod]
        public void CompositeRowBinding_XAML()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            RowBinding<Label> label = null;
            RowBinding<TextBlock> textBlock = null;
            CompositeRowBinding<XamlCompositeView> pane = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                textBlock = _.Name.AsTextBlock();
                label = _.Name.AsLabel(textBlock);
                pane = new CompositeRowBinding<XamlCompositeView>().AddChild(label, XamlCompositeView.NAME_LEFT).AddChild(textBlock, XamlCompositeView.NAME_RIGHT);
                builder.GridRows("100").GridColumns("100").AddBinding(0, 0, pane);
            });

            Assert.IsNull(label.SettingUpElement);
            Assert.IsNull(textBlock.SettingUpElement);
            Assert.IsNull(pane.GetSettingUpElement());

            var currentRow = elementManager.Rows[0];
            Assert.AreEqual(pane[currentRow].BindingDispatcher.Children[0], label[currentRow]);
            Assert.AreEqual(pane[currentRow].BindingDispatcher.Children[1], textBlock[currentRow]);
            Assert.AreEqual(_.Name.DisplayName, label[currentRow].Content);
            Assert.AreEqual(currentRow.GetValue(_.Name), textBlock[currentRow].Text);
            Assert.AreEqual(textBlock[currentRow], label[currentRow].Target);
        }

        [TestMethod]
        public void CompositeRowBinding_code()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            RowBinding<Label> label = null;
            RowBinding<TextBlock> textBlock = null;
            CompositeRowBinding<CodeCompositeView> pane = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                textBlock = _.Name.AsTextBlock();
                label = _.Name.AsLabel(textBlock);
                pane = new CompositeRowBinding<CodeCompositeView>().AddChild(label, CodeCompositeView.NAME_LEFT).AddChild(textBlock, CodeCompositeView.NAME_RIGHT);
                builder.GridRows("100").GridColumns("100").AddBinding(0, 0, pane);
            });

            var currentRow = elementManager.Rows[0];
            Assert.AreEqual(pane[currentRow].BindingDispatcher.Children[0], label[currentRow]);
            Assert.AreEqual(pane[currentRow].BindingDispatcher.Children[1], textBlock[currentRow]);
            Assert.AreEqual(_.Name.DisplayName, label[currentRow].Content);
            Assert.AreEqual(currentRow.GetValue(_.Name), textBlock[currentRow].Text);
            Assert.AreEqual(textBlock[currentRow], label[currentRow].Target);
        }
    }
}
