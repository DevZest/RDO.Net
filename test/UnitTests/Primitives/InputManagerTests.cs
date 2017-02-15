using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class InputManagerTests
    {
        [TestMethod]
        public void InputManager_RowInput()
        {
            var dataSet = ProductCategoryDataSet.Mock(3, false);
            var _ = dataSet._;
            RowBinding<TextBox> textBox = null;
            var inputManager = dataSet.CreateInputManager((builder) =>
            {
                textBox = _.ParentProductCategoryID.TextBox(UpdateSourceTrigger.PropertyChanged);
                builder.GridColumns("100").GridRows("100").AddBinding(0, 0, textBox);
            });

            var element = textBox[inputManager.CurrentRow];
            Assert.IsTrue(string.IsNullOrEmpty(element.Text));
            Assert.IsNull(inputManager.GetRowInputError(element));
            {
                var errors = System.Windows.Controls.Validation.GetErrors(element);
                Assert.AreEqual(0, errors.Count);
            }

            element.Text = "A";
            Assert.IsNotNull(inputManager.GetRowInputError(element));
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[inputManager.CurrentRow]);
                Assert.AreEqual(1, errors.Count);
                Assert.AreEqual(inputManager.GetRowInputError(element), errors[0].ErrorContent);
            }

            element.Text = "100";
            Assert.IsNull(inputManager.GetRowInputError(element));
            Assert.AreEqual(100, dataSet._.ParentProductCategoryID[inputManager.CurrentRow.DataRow]);
            {
                var errors = System.Windows.Controls.Validation.GetErrors(element);
                Assert.AreEqual(0, errors.Count);
            }
        }

        [TestMethod]
        public void InputManager_Scalar()
        {
            var oldValue = -1;
            var scalar = new Scalar<int>(0,
                onValueChanged: x =>
                {
                    oldValue = x;
                },
                valueValidator: x =>
                {
                    return x > 5 ? new InputError("ERR-01", "Value cannot be greater than 5.") : InputError.Empty;
                });

            Assert.AreEqual(-1, oldValue);

            scalar.Value = 4;
            Assert.AreEqual(0, oldValue);

            ArgumentException catchedException = null;
            try
            {
                scalar.Value = 6;
            }
            catch (ArgumentException ex)
            {
                catchedException = ex;
            }

            Assert.IsNotNull(catchedException);
        }

        [TestMethod]
        public void InputManager_ScalarInput()
        {
            var dataSet = ProductCategoryDataSet.Mock(3, false);
            var _ = dataSet._;
            Scalar<Int32> scalar = new Scalar<int>(valueValidator: x =>
            {
                return x > 5 ? new InputError("ERR-01", "Value cannot be greater than 5.") : InputError.Empty;
            });
            ScalarBinding<TextBox> textBox = null;
            RowBinding<TextBlock> textBlock = null;
            var inputManager = dataSet.CreateInputManager((builder) =>
            {
                textBox = scalar.TextBox(UpdateSourceTrigger.PropertyChanged);
                textBlock = _.Name.TextBlock(); // to avoid empty RowRange
                builder.GridColumns("100").GridRows("100", "100").AddBinding(0, 0, textBox).AddBinding(0, 1, textBlock);
            });

            Assert.AreEqual("0", textBox[0].Text);
            Assert.IsNull(inputManager.GetScalarInputError(textBox[0]));
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[0]);
                Assert.AreEqual(0, errors.Count);
            }

            textBox[0].Text = "A";
            Assert.IsNotNull(inputManager.GetScalarInputError(textBox[0]));
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[0]);
                Assert.AreEqual(1, errors.Count);
                Assert.AreEqual(inputManager.GetScalarInputError(textBox[0]), errors[0].ErrorContent);
            }

            textBox[0].Text = "4";
            Assert.IsNull(inputManager.GetScalarInputError(textBox[0]));
            Assert.AreEqual(4, scalar.Value);
            Assert.IsNull(inputManager.GetScalarInputError(textBox[0]));
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[0]);
                Assert.AreEqual(0, errors.Count);
            }

            textBox[0].Text = "6";
            Assert.AreEqual("6", textBox[0].Text);
            Assert.IsNotNull(inputManager.GetScalarValueError(textBox[0]));
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[0]);
                Assert.AreEqual(1, errors.Count);
                Assert.AreEqual(inputManager.GetScalarValueError(textBox[0]), errors[0].ErrorContent);
            }
        }
    }
}
