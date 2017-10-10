using DevZest.Data.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    [TestClass]
    public class CompositeScalarBindingTests
    {
        [TestMethod]
        public void CompositeScalarBinding()
        {
            //var dataSet = DataSetMock.ProductCategories(1);
            //var _ = dataSet._;
            //ScalarBinding<Label> label = null;
            //ScalarBinding<TextBlock> textBlock = null;
            //CompositeScalarBinding<XamlPane> pane = null;
            //var elementManager = dataSet.CreateElementManager(builder =>
            //{
            //    textBlock = _.Name.AsScalarTextBlock();
            //    label = _.Name.AsScalarLabel(textBlock);
            //    pane = new CompositeScalarBinding<XamlPane>().AddChild(label, v => v.Label).AddChild(textBlock, v => v.TextBlock);
            //    builder.Layout(Orientation.Vertical, 0)
            //        .GridColumns("100").GridRows("100", "100")
            //        .AddBinding(0, 0, pane)
            //        .AddBinding(0, 1, _.Name.AsTextBlock());
            //});

            //Assert.IsNull(label.SettingUpElement);
            //Assert.IsNull(textBlock.SettingUpElement);

            //Assert.AreEqual(1, label.FlowRepeatCount);
            //Assert.AreEqual(_.Name.DisplayName, label[0].Content);
            //Assert.AreEqual(1, textBlock.FlowRepeatCount);
            //Assert.AreEqual(_.Name.DisplayName, textBlock[0].Text);
            //Assert.AreEqual(textBlock[0], label[0].Target);
            throw new NotImplementedException();
        }

        [TestMethod]
        public void CompositeScalarBinding_flowable()
        {
            //var dataSet = DataSetMock.ProductCategories(1);
            //var _ = dataSet._;
            //ScalarBinding<Label> label = null;
            //ScalarBinding<TextBlock> textBlock = null;
            //CompositeScalarBinding<XamlPane> pane = null;
            //var elementManager = dataSet.CreateElementManager(builder =>
            //{
            //    textBlock = _.Name.AsScalarTextBlock().WithFlowRepeatable(true);
            //    label = _.Name.AsFlowRepeatableScalarLabel(textBlock);
            //    pane = new CompositeScalarBinding<XamlPane>().WithFlowRepeatable(true)
            //        .AddChild(label, XamlPane.NAME_LEFT).AddChild(textBlock, XamlPane.NAME_RIGHT);
            //    builder.Layout(Orientation.Vertical, 0)
            //        .GridColumns("100").GridRows("100", "100")
            //        .AddBinding(0, 0, pane)
            //        .AddBinding(0, 1, _.Name.AsTextBlock());
            //});

            //elementManager.FlowRepeatCount = 2;
            //Assert.AreEqual(2, pane.FlowRepeatCount);
            //Assert.AreEqual(0, pane[0].GetScalarFlowIndex());
            //Assert.AreEqual(1, pane[1].GetScalarFlowIndex());
            //Assert.AreEqual(2, label.FlowRepeatCount);
            //Assert.AreEqual("0. " + _.Name.DisplayName, label[0].Content);
            //Assert.AreEqual("1. " + _.Name.DisplayName, label[1].Content);
            //Assert.AreEqual(0, label[0].GetScalarFlowIndex());
            //Assert.AreEqual(1, label[1].GetScalarFlowIndex());
            //Assert.AreEqual(2, textBlock.FlowRepeatCount);
            //Assert.AreEqual(_.Name.DisplayName, textBlock[0].Text);
            //Assert.AreEqual(_.Name.DisplayName, textBlock[1].Text);
            //Assert.AreEqual(0, textBlock[0].GetScalarFlowIndex());
            //Assert.AreEqual(1, textBlock[1].GetScalarFlowIndex());
            //Assert.AreEqual(textBlock[0], label[0].Target);
            //Assert.AreEqual(textBlock[1], label[1].Target);
            //Assert.AreEqual(pane[0].Children[0], label[0]);
            //Assert.AreEqual(pane[0].Children[1], textBlock[0]);
            //Assert.AreEqual(pane[1].Children[0], label[1]);
            //Assert.AreEqual(pane[1].Children[1], textBlock[1]);

            //elementManager.FlowRepeatCount = 3;
            //Assert.AreEqual(3, pane.FlowRepeatCount);
            //Assert.AreEqual(0, pane[0].GetScalarFlowIndex());
            //Assert.AreEqual(1, pane[1].GetScalarFlowIndex());
            //Assert.AreEqual(2, pane[2].GetScalarFlowIndex());
            //Assert.AreEqual(3, label.FlowRepeatCount);
            //Assert.AreEqual("0. " + _.Name.DisplayName, label[0].Content);
            //Assert.AreEqual("1. " + _.Name.DisplayName, label[1].Content);
            //Assert.AreEqual("2. " + _.Name.DisplayName, label[2].Content);
            //Assert.AreEqual(0, label[0].GetScalarFlowIndex());
            //Assert.AreEqual(1, label[1].GetScalarFlowIndex());
            //Assert.AreEqual(2, label[2].GetScalarFlowIndex());
            //Assert.AreEqual(3, textBlock.FlowRepeatCount);
            //Assert.AreEqual(_.Name.DisplayName, textBlock[0].Text);
            //Assert.AreEqual(_.Name.DisplayName, textBlock[1].Text);
            //Assert.AreEqual(_.Name.DisplayName, textBlock[2].Text);
            //Assert.AreEqual(0, textBlock[0].GetScalarFlowIndex());
            //Assert.AreEqual(1, textBlock[1].GetScalarFlowIndex());
            //Assert.AreEqual(2, textBlock[2].GetScalarFlowIndex());
            //Assert.AreEqual(textBlock[0], label[0].Target);
            //Assert.AreEqual(textBlock[1], label[1].Target);
            //Assert.AreEqual(textBlock[2], label[2].Target);
            //Assert.AreEqual(pane[0].Children[0], label[0]);
            //Assert.AreEqual(pane[0].Children[1], textBlock[0]);
            //Assert.AreEqual(pane[1].Children[0], label[1]);
            //Assert.AreEqual(pane[1].Children[1], textBlock[1]);
            //Assert.AreEqual(pane[2].Children[0], label[2]);
            //Assert.AreEqual(pane[2].Children[1], textBlock[2]);

            //elementManager.FlowRepeatCount = 2;
            //Assert.AreEqual(2, pane.FlowRepeatCount);
            //Assert.AreEqual(0, pane[0].GetScalarFlowIndex());
            //Assert.AreEqual(1, pane[1].GetScalarFlowIndex());
            //Assert.AreEqual(2, label.FlowRepeatCount);
            //Assert.AreEqual("0. " + _.Name.DisplayName, label[0].Content);
            //Assert.AreEqual("1. " + _.Name.DisplayName, label[1].Content);
            //Assert.AreEqual(0, label[0].GetScalarFlowIndex());
            //Assert.AreEqual(1, label[1].GetScalarFlowIndex());
            //Assert.AreEqual(2, textBlock.FlowRepeatCount);
            //Assert.AreEqual(_.Name.DisplayName, textBlock[0].Text);
            //Assert.AreEqual(_.Name.DisplayName, textBlock[1].Text);
            //Assert.AreEqual(0, textBlock[0].GetScalarFlowIndex());
            //Assert.AreEqual(1, textBlock[1].GetScalarFlowIndex());
            //Assert.AreEqual(textBlock[0], label[0].Target);
            //Assert.AreEqual(textBlock[1], label[1].Target);
            //Assert.AreEqual(pane[0].Children[0], label[0]);
            //Assert.AreEqual(pane[0].Children[1], textBlock[0]);
            //Assert.AreEqual(pane[1].Children[0], label[1]);
            //Assert.AreEqual(pane[1].Children[1], textBlock[1]);
            throw new NotImplementedException();
        }
    }
}
