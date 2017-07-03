using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
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
            ScalarBinding<TextBlock> textBlock = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                textBlock = _.Name.AsScalarTextBlock();
                label = _.Name.AsScalarLabel(textBlock);
                builder.GridColumns("100", "100", "100").GridRows("100").RowRange(2, 0, 2, 0)
                    .AddBinding(0, 0, label)
                    .AddBinding(1, 0, textBlock);
            });

            Assert.IsNull(label.SettingUpElement);
            Assert.IsNull(textBlock.SettingUpElement);

            Assert.AreEqual(1, label.FlowRepeatCount);
            Assert.AreEqual(_.Name.DisplayName, label[0].Content);
            Assert.AreEqual(1, textBlock.FlowRepeatCount);
            Assert.AreEqual(_.Name.DisplayName, textBlock[0].Text);
            Assert.AreEqual(textBlock[0], label[0].Target);
        }

        [TestMethod]
        public void ScalarBinding_flowable()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            ScalarBinding<Label> label = null;
            ScalarBinding<TextBlock> textBlock = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                textBlock = _.Name.AsScalarTextBlock().WithFlowRepeatable(true);
                label = _.Name.AsFlowRepeatableScalarLabel(textBlock);
                builder.Layout(Orientation.Vertical, 0)
                    .GridColumns("100", "100").GridRows("100", "100")
                    .AddBinding(0, 0, label)
                    .AddBinding(0, 1, 1, 1, _.Name.AsTextBlock())
                    .AddBinding(1, 0, textBlock);
            });

            elementManager.FlowRepeatCount = 2;

            Assert.IsNull(label.SettingUpElement);
            Assert.IsNull(textBlock.SettingUpElement);

            Assert.AreEqual(2, label.FlowRepeatCount);
            Assert.AreEqual("0. " + _.Name.DisplayName, label[0].Content);
            Assert.AreEqual("1. " + _.Name.DisplayName, label[1].Content);
            Assert.AreEqual(0, label[0].GetScalarFlowIndex());
            Assert.AreEqual(1, label[1].GetScalarFlowIndex());
            Assert.AreEqual(2, textBlock.FlowRepeatCount);
            Assert.AreEqual(_.Name.DisplayName, textBlock[0].Text);
            Assert.AreEqual(_.Name.DisplayName, textBlock[1].Text);
            Assert.AreEqual(0, textBlock[0].GetScalarFlowIndex());
            Assert.AreEqual(1, textBlock[1].GetScalarFlowIndex());
            Assert.AreEqual(textBlock[0], label[0].Target);
            Assert.AreEqual(textBlock[1], label[1].Target);

            elementManager.FlowRepeatCount = 3;
            Assert.AreEqual(3, label.FlowRepeatCount);
            Assert.AreEqual("0. " + _.Name.DisplayName, label[0].Content);
            Assert.AreEqual("1. " + _.Name.DisplayName, label[1].Content);
            Assert.AreEqual("2. " + _.Name.DisplayName, label[2].Content);
            Assert.AreEqual(0, label[0].GetScalarFlowIndex());
            Assert.AreEqual(1, label[1].GetScalarFlowIndex());
            Assert.AreEqual(2, label[2].GetScalarFlowIndex());
            Assert.AreEqual(3, textBlock.FlowRepeatCount);
            Assert.AreEqual(_.Name.DisplayName, textBlock[0].Text);
            Assert.AreEqual(_.Name.DisplayName, textBlock[1].Text);
            Assert.AreEqual(_.Name.DisplayName, textBlock[2].Text);
            Assert.AreEqual(0, textBlock[0].GetScalarFlowIndex());
            Assert.AreEqual(1, textBlock[1].GetScalarFlowIndex());
            Assert.AreEqual(2, textBlock[2].GetScalarFlowIndex());
            Assert.AreEqual(textBlock[0], label[0].Target);
            Assert.AreEqual(textBlock[1], label[1].Target);
            Assert.AreEqual(textBlock[2], label[2].Target);

            elementManager.FlowRepeatCount = 2;
            Assert.AreEqual(2, label.FlowRepeatCount);
            Assert.AreEqual("0. " + _.Name.DisplayName, label[0].Content);
            Assert.AreEqual("1. " + _.Name.DisplayName, label[1].Content);
            Assert.AreEqual(0, label[0].GetScalarFlowIndex());
            Assert.AreEqual(1, label[1].GetScalarFlowIndex());
            Assert.AreEqual(2, textBlock.FlowRepeatCount);
            Assert.AreEqual(_.Name.DisplayName, textBlock[0].Text);
            Assert.AreEqual(_.Name.DisplayName, textBlock[1].Text);
            Assert.AreEqual(0, textBlock[0].GetScalarFlowIndex());
            Assert.AreEqual(1, textBlock[1].GetScalarFlowIndex());
            Assert.AreEqual(textBlock[0], label[0].Target);
            Assert.AreEqual(textBlock[1], label[1].Target);
        }
    }
}
