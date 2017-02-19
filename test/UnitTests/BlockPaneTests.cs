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
            BlockBinding<BlockLabel> blockLabel = null;
            BlockBinding<BlockHeader> blockHeader = null;
            BlockPane<XamlPane> pane = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                blockHeader = _.BlockHeader();
                blockLabel = _.Name.BlockLabel(blockHeader);
                pane = new BlockPane<XamlPane>().AddChild(blockLabel, XamlPane.NAME_LEFT).AddChild(blockHeader, XamlPane.NAME_RIGHT);
                builder.Layout(Orientation.Vertical, 2)
                    .GridColumns("100", "100").GridRows("100").RowRange(1, 0, 1, 0)
                    .AddBinding(0, 0, pane);
            });

            Assert.IsNull(blockLabel.SettingUpElement);
            Assert.IsNull(blockHeader.SettingUpElement);
            Assert.IsNull(pane.GetSettingUpElement());

            var currentRow = elementManager.Rows[0];
            Assert.AreEqual(pane[0].Children[0], blockLabel[0]);
            Assert.AreEqual(pane[0].Children[1], blockHeader[0]);
            Assert.AreEqual(_.Name.DisplayName, blockLabel[0].Content);
            Assert.AreEqual("0", blockHeader[0].Text);
            Assert.AreEqual(blockHeader[0], blockLabel[0].Target);
        }
    }
}
