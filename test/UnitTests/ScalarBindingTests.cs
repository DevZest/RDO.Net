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
            ScalarBinding<ColumnHeader> columnHeader = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                columnHeader = _.Name.AsColumnHeader();
                label = _.Name.ScalarLabel(columnHeader);
                builder.GridColumns("100", "100", "100").GridRows("100").RowRange(2, 0, 2, 0)
                    .AddBinding(0, 0, label)
                    .AddBinding(1, 0, columnHeader);
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
        public void ScalarBinding_flowable()
        {
            var dataSet = DataSetMock.ProductCategories(1);
            var _ = dataSet._;
            ScalarBinding<Label> label = null;
            ScalarBinding<ColumnHeader> columnHeader = null;
            var elementManager = dataSet.CreateElementManager(builder =>
            {
                columnHeader = _.Name.AsColumnHeader().WithFlowable(true);
                label = _.Name.FlowableLabel(columnHeader);
                builder.Layout(Orientation.Vertical, 0)
                    .GridColumns("100", "100").GridRows("100", "100")
                    .AddBinding(0, 0, label)
                    .AddBinding(0, 1, 1, 1, _.Name.AsTextBlock())
                    .AddBinding(1, 0, columnHeader);
            });

            elementManager.FlowCount = 2;

            Assert.IsNull(label.SettingUpElement);
            Assert.IsNull(columnHeader.SettingUpElement);

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

            elementManager.FlowCount = 3;
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

            elementManager.FlowCount = 2;
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
        }
    }
}
