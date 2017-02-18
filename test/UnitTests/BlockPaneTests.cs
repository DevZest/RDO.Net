using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class BlockPaneTests
    {
        [TestMethod]
        public void BlockPane_XAML()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            BlockBinding<Label> label = null;
            BlockBinding<TextBlock> textBlock = null;
            BlockPane<XamlPane> pane = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                textBlock = new BlockBinding<TextBlock>(
                    onRefresh: (e, i, r) =>
                    {
                        e.Text = (i + 1).ToString();
                    });
                label = new BlockBinding<Label>(
                    onSetup: (e, i, r) =>
                    {
                        e.Content = _.Name.DisplayName;
                        e.Target = textBlock.SettingUpElement;
                    },
                    onRefresh: (e, i, r) =>
                    {
                    },
                    onCleanup: (e, i, r) =>
                    {
                    });
                pane = new BlockPane<XamlPane>().AddChild(label, XamlPane.NAME_LEFT).AddChild(textBlock, XamlPane.NAME_RIGHT);
                builder.Layout(Orientation.Vertical, 2)
                    .GridColumns("100", "100").GridRows("100")
                    .AddBinding(0, 0, pane).AddBinding(1, 0, _.Name.TextBlock());
            });

            Assert.IsNull(label.SettingUpElement);
            Assert.IsNull(textBlock.SettingUpElement);
            Assert.IsNull(pane.GetSettingUpElement());

            var currentRow = elementManager.Rows[0];
            Assert.AreEqual(pane[0].Children[0], label[0]);
            Assert.AreEqual(pane[0].Children[1], textBlock[0]);
            Assert.AreEqual(_.Name.DisplayName, label[0].Content);
            Assert.AreEqual("1", textBlock[0].Text);
            Assert.AreEqual(textBlock[0], label[0].Target);
        }
    }
}
