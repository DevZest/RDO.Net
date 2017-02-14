using DevZest.Data.Windows.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class InputManagerTests
    {
        #region Helpers

        private sealed class ConcreteInputManager : InputManager
        {
            public ConcreteInputManager(Template template, DataSet dataSet, _Boolean where = null, ColumnSort[] orderBy = null, bool emptyBlockViewList = false)
                : base(template, dataSet, where, orderBy, emptyBlockViewList)
            {
            }
        }

        private static InputManager CreateInputManager<T>(DataSet<T> dataSet, Action<TemplateBuilder, T> buildTemplateAction)
            where T : Model, new()
        {
            var template = new Template();
            using (var templateBuilder = new TemplateBuilder(template, dataSet.Model))
            {
                buildTemplateAction(templateBuilder, dataSet._);
                templateBuilder.BlockView<AutoInitBlockView>()
                    .RowView<AutoInitRowView>();
            }
            var result = new ConcreteInputManager(template, dataSet);
            result.InitializeElements(null);
            return result;
        }

        #endregion

        [TestMethod]
        public void InputManager_RowInput()
        {
            var dataSet = ProductCategoryDataSet.Mock(3, false);
            RowBinding<TextBox> textBox = null;
            var inputManager = CreateInputManager(dataSet, (builder, _) =>
            {
                textBox = _.ParentProductCategoryID.TextBox(UpdateSourceTrigger.PropertyChanged);
                builder.GridColumns("100").GridRows("100").AddBinding(0, 0, textBox);
            });

            Assert.IsTrue(string.IsNullOrEmpty(textBox[inputManager.CurrentRow].Text));
            Assert.IsNull(inputManager.GetRowInputError(textBox[inputManager.CurrentRow]));

            textBox[inputManager.CurrentRow].Text = "A";
            Assert.IsNotNull(inputManager.GetRowInputError(textBox[inputManager.CurrentRow]));

            textBox[inputManager.CurrentRow].Text = "100";
            Assert.IsNull(inputManager.GetRowInputError(textBox[inputManager.CurrentRow]));
            Assert.AreEqual(100, dataSet._.ParentProductCategoryID[inputManager.CurrentRow.DataRow]);
        }

        [TestMethod]
        public void InputManager_ScalarInput()
        {
            var dataSet = ProductCategoryDataSet.Mock(3, false);
            Scalar<Int32> scalar = new Scalar<int>();
            ScalarBinding<TextBox> textBox = null;
            RowBinding<TextBlock> textBlock = null;
            var inputManager = CreateInputManager(dataSet, (builder, _) =>
            {
                textBox = scalar.TextBox(UpdateSourceTrigger.PropertyChanged);
                textBlock = _.Name.TextBlock(); // to avoid empty RowRange
                builder.GridColumns("100").GridRows("100", "100").AddBinding(0, 0, textBox).AddBinding(0, 1, textBlock);
            });

            Assert.AreEqual("0", textBox[0].Text);
            Assert.IsNull(inputManager.GetScalarInputError(textBox[0]));

            textBox[0].Text = "A";
            Assert.IsNotNull(inputManager.GetScalarInputError(textBox[0]));

            textBox[0].Text = "100";
            Assert.IsNull(inputManager.GetScalarInputError(textBox[0]));
            Assert.AreEqual(100, scalar.Value);

        }
    }
}
