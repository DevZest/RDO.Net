using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    [TestClass]
    public class ScalarPaneTests
    {
        [TestMethod]
        public void ScalarPane()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            ScalarBinding<Label> label = null;
            ScalarBinding<ColumnHeader> columnHeader = null;
            CompositeScalarBinding<XamlCompositeView> pane = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                columnHeader = _.Name.AsColumnHeader();
                label = _.Name.ScalarLabel(columnHeader);
                pane = new CompositeScalarBinding<XamlCompositeView>().AddChild(label, XamlCompositeView.NAME_LEFT).AddChild(columnHeader, XamlCompositeView.NAME_RIGHT);
                builder.Layout(Orientation.Vertical, 0)
                    .GridColumns("100").GridRows("100", "100")
                    .AddBinding(0, 0, pane)
                    .AddBinding(0, 1, _.Name.AsTextBlock());
            });

            Assert.IsNull(label.SettingUpElement);
            Assert.IsNull(columnHeader.SettingUpElement);

            Assert.AreEqual(1, label.FlowCount);
            Assert.AreEqual(_.Name.DisplayName, label[0].Content);
            Assert.AreEqual(1, columnHeader.FlowCount);
            Assert.AreEqual(_.Name, columnHeader[0].Column);
            Assert.AreEqual(columnHeader[0], label[0].Target);
        }

        [TestMethod]
        public void ScalarPane_flowable()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            ScalarBinding<Label> label = null;
            ScalarBinding<ColumnHeader> columnHeader = null;
            CompositeScalarBinding<XamlCompositeView> pane = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                columnHeader = _.Name.AsColumnHeader().WithFlowable(true);
                label = _.Name.FlowableLabel(columnHeader);
                pane = new CompositeScalarBinding<XamlCompositeView>().WithFlowable(true)
                    .AddChild(label, XamlCompositeView.NAME_LEFT).AddChild(columnHeader, XamlCompositeView.NAME_RIGHT);
                builder.Layout(Orientation.Vertical, 0)
                    .GridColumns("100").GridRows("100", "100")
                    .AddBinding(0, 0, pane)
                    .AddBinding(0, 1, _.Name.AsTextBlock());
            });

            elementManager.FlowCount = 2;
            Assert.AreEqual(2, pane.FlowCount);
            Assert.AreEqual(0, pane[0].GetScalarFlowIndex());
            Assert.AreEqual(1, pane[1].GetScalarFlowIndex());
            Assert.AreEqual(2, label.FlowCount);
            Assert.AreEqual("0. " + _.Name.DisplayName, label[0].Content);
            Assert.AreEqual("1. " + _.Name.DisplayName, label[1].Content);
            Assert.AreEqual(0, label[0].GetScalarFlowIndex());
            Assert.AreEqual(1, label[1].GetScalarFlowIndex());
            Assert.AreEqual(2, columnHeader.FlowCount);
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

            elementManager.FlowCount = 3;
            Assert.AreEqual(3, pane.FlowCount);
            Assert.AreEqual(0, pane[0].GetScalarFlowIndex());
            Assert.AreEqual(1, pane[1].GetScalarFlowIndex());
            Assert.AreEqual(2, pane[2].GetScalarFlowIndex());
            Assert.AreEqual(3, label.FlowCount);
            Assert.AreEqual("0. " + _.Name.DisplayName, label[0].Content);
            Assert.AreEqual("1. " + _.Name.DisplayName, label[1].Content);
            Assert.AreEqual("2. " + _.Name.DisplayName, label[2].Content);
            Assert.AreEqual(0, label[0].GetScalarFlowIndex());
            Assert.AreEqual(1, label[1].GetScalarFlowIndex());
            Assert.AreEqual(2, label[2].GetScalarFlowIndex());
            Assert.AreEqual(3, columnHeader.FlowCount);
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

            elementManager.FlowCount = 2;
            Assert.AreEqual(2, pane.FlowCount);
            Assert.AreEqual(0, pane[0].GetScalarFlowIndex());
            Assert.AreEqual(1, pane[1].GetScalarFlowIndex());
            Assert.AreEqual(2, label.FlowCount);
            Assert.AreEqual("0. " + _.Name.DisplayName, label[0].Content);
            Assert.AreEqual("1. " + _.Name.DisplayName, label[1].Content);
            Assert.AreEqual(0, label[0].GetScalarFlowIndex());
            Assert.AreEqual(1, label[1].GetScalarFlowIndex());
            Assert.AreEqual(2, columnHeader.FlowCount);
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
