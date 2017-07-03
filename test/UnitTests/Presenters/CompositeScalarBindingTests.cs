using DevZest.Data.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    [TestClass]
    public class CompositeScalarBindingTests
    {
        [TestMethod]
        public void CompositeScalarBinding()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            ScalarBinding<Label> label = null;
            ScalarBinding<ColumnHeader> columnHeader = null;
            CompositeScalarBinding<XamlCompositeView> pane = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                columnHeader = _.Name.AsColumnHeader();
                label = _.Name.AsScalarLabel(columnHeader);
                pane = new CompositeScalarBinding<XamlCompositeView>().AddChild(label, XamlCompositeView.NAME_LEFT).AddChild(columnHeader, XamlCompositeView.NAME_RIGHT);
                builder.Layout(Orientation.Vertical, 0)
                    .GridColumns("100").GridRows("100", "100")
                    .AddBinding(0, 0, pane)
                    .AddBinding(0, 1, _.Name.AsTextBlock());
            });

            Assert.IsNull(label.SettingUpElement);
            Assert.IsNull(columnHeader.SettingUpElement);

            Assert.AreEqual(1, label.FlowRepeatCount);
            Assert.AreEqual(_.Name.DisplayName, label[0].Content);
            Assert.AreEqual(1, columnHeader.FlowRepeatCount);
            Assert.AreEqual(_.Name, columnHeader[0].Column);
            Assert.AreEqual(columnHeader[0], label[0].Target);
        }

        [TestMethod]
        public void CompositeScalarBinding_flowable()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            ScalarBinding<Label> label = null;
            ScalarBinding<ColumnHeader> columnHeader = null;
            CompositeScalarBinding<XamlCompositeView> pane = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                columnHeader = _.Name.AsColumnHeader().WithFlowRepeatable(true);
                label = _.Name.AsFlowRepeatableScalarLabel(columnHeader);
                pane = new CompositeScalarBinding<XamlCompositeView>().WithFlowRepeatable(true)
                    .AddChild(label, XamlCompositeView.NAME_LEFT).AddChild(columnHeader, XamlCompositeView.NAME_RIGHT);
                builder.Layout(Orientation.Vertical, 0)
                    .GridColumns("100").GridRows("100", "100")
                    .AddBinding(0, 0, pane)
                    .AddBinding(0, 1, _.Name.AsTextBlock());
            });

            elementManager.FlowRepeatCount = 2;
            Assert.AreEqual(2, pane.FlowRepeatCount);
            Assert.AreEqual(0, pane[0].GetScalarFlowIndex());
            Assert.AreEqual(1, pane[1].GetScalarFlowIndex());
            Assert.AreEqual(2, label.FlowRepeatCount);
            Assert.AreEqual("0. " + _.Name.DisplayName, label[0].Content);
            Assert.AreEqual("1. " + _.Name.DisplayName, label[1].Content);
            Assert.AreEqual(0, label[0].GetScalarFlowIndex());
            Assert.AreEqual(1, label[1].GetScalarFlowIndex());
            Assert.AreEqual(2, columnHeader.FlowRepeatCount);
            Assert.AreEqual(_.Name, columnHeader[0].Column);
            Assert.AreEqual(_.Name, columnHeader[1].Column);
            Assert.AreEqual(0, columnHeader[0].GetScalarFlowIndex());
            Assert.AreEqual(1, columnHeader[1].GetScalarFlowIndex());
            Assert.AreEqual(columnHeader[0], label[0].Target);
            Assert.AreEqual(columnHeader[1], label[1].Target);
            Assert.AreEqual(pane[0].Children[0], label[0]);
            Assert.AreEqual(pane[0].Children[1], columnHeader[0]);
            Assert.AreEqual(pane[1].Children[0], label[1]);
            Assert.AreEqual(pane[1].Children[1], columnHeader[1]);

            elementManager.FlowRepeatCount = 3;
            Assert.AreEqual(3, pane.FlowRepeatCount);
            Assert.AreEqual(0, pane[0].GetScalarFlowIndex());
            Assert.AreEqual(1, pane[1].GetScalarFlowIndex());
            Assert.AreEqual(2, pane[2].GetScalarFlowIndex());
            Assert.AreEqual(3, label.FlowRepeatCount);
            Assert.AreEqual("0. " + _.Name.DisplayName, label[0].Content);
            Assert.AreEqual("1. " + _.Name.DisplayName, label[1].Content);
            Assert.AreEqual("2. " + _.Name.DisplayName, label[2].Content);
            Assert.AreEqual(0, label[0].GetScalarFlowIndex());
            Assert.AreEqual(1, label[1].GetScalarFlowIndex());
            Assert.AreEqual(2, label[2].GetScalarFlowIndex());
            Assert.AreEqual(3, columnHeader.FlowRepeatCount);
            Assert.AreEqual(_.Name, columnHeader[0].Column);
            Assert.AreEqual(_.Name, columnHeader[1].Column);
            Assert.AreEqual(_.Name, columnHeader[2].Column);
            Assert.AreEqual(0, columnHeader[0].GetScalarFlowIndex());
            Assert.AreEqual(1, columnHeader[1].GetScalarFlowIndex());
            Assert.AreEqual(2, columnHeader[2].GetScalarFlowIndex());
            Assert.AreEqual(columnHeader[0], label[0].Target);
            Assert.AreEqual(columnHeader[1], label[1].Target);
            Assert.AreEqual(columnHeader[2], label[2].Target);
            Assert.AreEqual(pane[0].Children[0], label[0]);
            Assert.AreEqual(pane[0].Children[1], columnHeader[0]);
            Assert.AreEqual(pane[1].Children[0], label[1]);
            Assert.AreEqual(pane[1].Children[1], columnHeader[1]);
            Assert.AreEqual(pane[2].Children[0], label[2]);
            Assert.AreEqual(pane[2].Children[1], columnHeader[2]);

            elementManager.FlowRepeatCount = 2;
            Assert.AreEqual(2, pane.FlowRepeatCount);
            Assert.AreEqual(0, pane[0].GetScalarFlowIndex());
            Assert.AreEqual(1, pane[1].GetScalarFlowIndex());
            Assert.AreEqual(2, label.FlowRepeatCount);
            Assert.AreEqual("0. " + _.Name.DisplayName, label[0].Content);
            Assert.AreEqual("1. " + _.Name.DisplayName, label[1].Content);
            Assert.AreEqual(0, label[0].GetScalarFlowIndex());
            Assert.AreEqual(1, label[1].GetScalarFlowIndex());
            Assert.AreEqual(2, columnHeader.FlowRepeatCount);
            Assert.AreEqual(_.Name, columnHeader[0].Column);
            Assert.AreEqual(_.Name, columnHeader[1].Column);
            Assert.AreEqual(0, columnHeader[0].GetScalarFlowIndex());
            Assert.AreEqual(1, columnHeader[1].GetScalarFlowIndex());
            Assert.AreEqual(columnHeader[0], label[0].Target);
            Assert.AreEqual(columnHeader[1], label[1].Target);
            Assert.AreEqual(pane[0].Children[0], label[0]);
            Assert.AreEqual(pane[0].Children[1], columnHeader[0]);
            Assert.AreEqual(pane[1].Children[0], label[1]);
            Assert.AreEqual(pane[1].Children[1], columnHeader[1]);
        }
    }
}
