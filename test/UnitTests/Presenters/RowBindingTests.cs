using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    [TestClass]
    public class RowBindingTests
    {
        [TestMethod]
        public void RowBinding()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            RowBinding<Label> label = null;
            RowBinding<TextBlock> textBlock = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                textBlock = _.Name.BindToTextBlock();
                label = _.Name.BindToLabel(textBlock);
                builder.GridColumns("100", "100").GridRows("100")
                    .AddBinding(0, 0, label)
                    .AddBinding(1, 0, textBlock);
            });

            Assert.IsNull(label.SettingUpElement);
            Assert.IsNull(textBlock.SettingUpElement);

            var currentRow = elementManager.Rows[0];
            Assert.AreEqual(_.Name.DisplayName, label[currentRow].Content);
            Assert.AreEqual(currentRow.GetValue(_.Name), textBlock[currentRow].Text);
            Assert.AreEqual(textBlock[currentRow], label[currentRow].Target);
        }

    }
}
