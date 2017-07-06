using DevZest.Data;
using DevZest.Data.Views;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace DevZest.Data.Presenters.Primitives
{
    [TestClass]
    public class InputManagerTests
    {
        [TestMethod]
        public void InputManager_RowInput()
        {
            var dataSet = DataSetMock.ProductCategories(3, false);
            var _ = dataSet._;
            RowBinding<TextBox> textBox = null;
            var inputManager = dataSet.CreateInputManager((builder) =>
            {
                textBox = _.ParentProductCategoryID.AsTextBox(UpdateSourceTrigger.PropertyChanged);
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
            var dataSet = DataSetMock.ProductCategories(3, false);
            var _ = dataSet._;
            Scalar<Int32> scalar = new Scalar<int>(valueValidator: x =>
            {
                return x > 5 ? new InputError("ERR-01", "Value cannot be greater than 5.") : InputError.Empty;
            });
            ScalarBinding<TextBox> textBox = null;
            RowBinding<TextBlock> textBlock = null;
            var inputManager = dataSet.CreateInputManager((builder) =>
            {
                textBox = scalar.AsTextBox(UpdateSourceTrigger.PropertyChanged);
                textBlock = _.Name.AsTextBlock(); // to avoid empty RowRange
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

        [TestMethod]
        public void InputManager_Validate_Progress()
        {
            var dataSet = DataSet<ProductCategory>.New();
            var _ = dataSet._;
            dataSet.Add(new DataRow());

            RowBinding<TextBox> textBox = null;
            var inputManager = dataSet.CreateInputManager(builder =>
            {
                textBox = _.Name.AsTextBox(UpdateSourceTrigger.PropertyChanged);
                builder.GridColumns("100").GridRows("100").AddBinding(0, 0, textBox);
            });

            var currentRow = inputManager.CurrentRow;
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(0, errors.Count);
            }

            textBox[currentRow].Text = "some name";
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(0, errors.Count);
            }

            textBox[currentRow].Text = null;
            Assert.AreEqual(string.Empty, textBox[currentRow].Text);
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(1, errors.Count);
                var errorMessage = (ValidationMessage)errors[0].ErrorContent;
                Assert.AreEqual("DevZest.Data.Required", errorMessage.Id);
                Assert.AreEqual(_.Name, errorMessage.Source);
            }

            textBox[currentRow].Text = "some other name";
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(0, errors.Count);
            }
        }

        [TestMethod]
        public void InputManager_Validate_Implicit()
        {
            var dataSet = DataSet<ProductCategory>.New();
            var _ = dataSet._;
            dataSet.Add(new DataRow());

            RowBinding<TextBox> textBox = null;
            var inputManager = dataSet.CreateInputManager(builder =>
            {
                textBox = _.Name.AsTextBox(UpdateSourceTrigger.PropertyChanged);
                builder.GridColumns("100").GridRows("100").AddBinding(0, 0, textBox).WithValidationMode(ValidationMode.Implicit);
            });

            var currentRow = inputManager.CurrentRow;
            Assert.IsNull(_.Name[0]);
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(1, errors.Count);
                var errorMessage = (ValidationMessage)errors[0].ErrorContent;
                Assert.AreEqual("DevZest.Data.Required", errorMessage.Id);
                Assert.AreEqual(_.Name, errorMessage.Source);
            }

            textBox[currentRow].Text = "some name";
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(0, errors.Count);
            }
        }

        private const string BAD_NAME = "Bad Name";

        [TestMethod]
        public void InputManager_AsyncValidators_input_error_to_invalid_to_valid()
        {
            RunInWpfSyncContext(InputManager_AsyncValidators_input_error_to_invalid_to_valid_Async);
        }

        private async Task InputManager_AsyncValidators_input_error_to_invalid_to_valid_Async()
        {
            var dataSet = DataSet<ProductCategory>.New();
            var _ = dataSet._;
            dataSet.Add(new DataRow());

            RowBinding<TextBox> textBox = null;
            RowBinding<ValidationView> validationView = null;
            var inputManager = dataSet.CreateInputManager(builder =>
            {
                textBox = _.Name.AsTextBox(UpdateSourceTrigger.PropertyChanged);
                validationView = textBox.Input.AsValidationView();

                builder.GridColumns("100").GridRows("100")
                    .AddBinding(0, 0, textBox).WithValidationMode(ValidationMode.Implicit)
                    .AddBinding(0, 0, validationView);

                textBox.Input.AddAsyncValidator(() => ValidateBadNameAsync(_.Name, 0));
            });

            var currentRow = inputManager.CurrentRow;
            var asyncValidator = validationView[currentRow].AsyncValidators[0];
            Assert.AreEqual(asyncValidator, validationView[currentRow].AsyncValidators);

            Assert.AreEqual(AsyncValidatorStatus.Created, asyncValidator.Status);
            Assert.AreEqual(0, validationView[currentRow].RunningAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].CompletedAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].FaultedAsyncValidators.Count);
            Assert.AreEqual(1, validationView[currentRow].Errors.Count);

            textBox[currentRow].Text = BAD_NAME;
            Assert.AreEqual(AsyncValidatorStatus.Running, asyncValidator.Status);
            Assert.AreEqual(asyncValidator, validationView[currentRow].RunningAsyncValidators);
            Assert.AreEqual(0, validationView[currentRow].CompletedAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].FaultedAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].Errors.Count);

            await asyncValidator.LastRunningTask;
            Assert.AreEqual(AsyncValidatorStatus.Completed, asyncValidator.Status);
            Assert.AreEqual(0, validationView[currentRow].RunningAsyncValidators.Count);
            Assert.AreEqual(asyncValidator, validationView[currentRow].CompletedAsyncValidators);
            Assert.AreEqual(0, validationView[currentRow].FaultedAsyncValidators.Count);
            Assert.AreEqual(1, validationView[currentRow].Errors.Count);

            textBox[currentRow].Text = "Good Name";
            Assert.AreEqual(AsyncValidatorStatus.Running, asyncValidator.Status);
            Assert.AreEqual(asyncValidator, validationView[currentRow].RunningAsyncValidators);
            Assert.AreEqual(0, validationView[currentRow].CompletedAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].FaultedAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].Errors.Count);

            await asyncValidator.LastRunningTask;
            Assert.AreEqual(AsyncValidatorStatus.Completed, asyncValidator.Status);
            Assert.AreEqual(0, validationView[currentRow].RunningAsyncValidators.Count);
            Assert.AreEqual(asyncValidator, validationView[currentRow].CompletedAsyncValidators);
            Assert.AreEqual(0, validationView[currentRow].FaultedAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].Errors.Count);
        }

        private static async Task<IValidationMessageGroup> ValidateBadNameAsync(_String nameColumn, int index)
        {
            await Task.Delay(200);
            var value = nameColumn[index];
            return value == BAD_NAME ? new ValidationMessage("ERR-01", ValidationSeverity.Error, "Bad Name", nameColumn) : null;
        }

        [TestMethod]
        public void InputManager_AsyncValidators_faulted()
        {
            RunInWpfSyncContext(InputManager_AsyncValidators_faulted_Async);
        }

        private async Task InputManager_AsyncValidators_faulted_Async()
        {
            var dataSet = DataSet<ProductCategory>.New();
            var _ = dataSet._;
            dataSet.Add(new DataRow());

            RowBinding<TextBox> textBox = null;
            RowBinding<ValidationView> validationView = null;
            var inputManager = dataSet.CreateInputManager(builder =>
            {
                textBox = _.Name.AsTextBox(UpdateSourceTrigger.PropertyChanged);
                validationView = textBox.Input.AsValidationView();

                builder.GridColumns("100").GridRows("100")
                    .AddBinding(0, 0, textBox).WithValidationMode(ValidationMode.Implicit)
                    .AddBinding(0, 0, validationView);

                textBox.Input.AddAsyncValidator(ValidateFaultedAsync);
            });

            var currentRow = inputManager.CurrentRow;
            var asyncValidator = validationView[currentRow].AsyncValidators[0];

            textBox[currentRow].Text = "Anything";
            Assert.AreEqual(AsyncValidatorStatus.Running, asyncValidator.Status);
            Assert.AreEqual(asyncValidator, validationView[currentRow].RunningAsyncValidators);
            Assert.AreEqual(0, validationView[currentRow].CompletedAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].FaultedAsyncValidators.Count);

            await asyncValidator.LastRunningTask;
            Assert.AreEqual(AsyncValidatorStatus.Faulted, asyncValidator.Status);
            Assert.AreEqual(typeof(InvalidOperationException), asyncValidator.Exception.GetType());
            Assert.AreEqual(0, validationView[currentRow].RunningAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].CompletedAsyncValidators.Count);
            Assert.AreEqual(asyncValidator, validationView[currentRow].FaultedAsyncValidators);
        }

        private static async Task<IValidationMessageGroup> ValidateFaultedAsync()
        {
            await Task.Delay(200);
            throw new InvalidOperationException("Validation failed.");
        }

        [TestMethod]
        public void InputManager_AsyncValidators_Reset()
        {
            RunInWpfSyncContext(InputManager_AsyncValidators_Reset_Async);
        }

        private async Task InputManager_AsyncValidators_Reset_Async()
        {
            var dataSet = DataSet<ProductCategory>.New();
            var _ = dataSet._;
            dataSet.Add(new DataRow());

            RowBinding<TextBox> textBox = null;
            RowBinding<ValidationView> validationView = null;
            var inputManager = dataSet.CreateInputManager(builder =>
            {
                textBox = _.Name.AsTextBox(UpdateSourceTrigger.PropertyChanged);
                validationView = textBox.Input.AsValidationView();

                builder.GridColumns("100").GridRows("100")
                    .AddBinding(0, 0, textBox).WithValidationMode(ValidationMode.Implicit)
                    .AddBinding(0, 0, validationView);

                textBox.Input.AddAsyncValidator(ValidateFaultedAsync);
            });

            var currentRow = inputManager.CurrentRow;
            var asyncValidator = validationView[currentRow].AsyncValidators[0];

            textBox[currentRow].Text = "Anything";
            Assert.AreEqual(AsyncValidatorStatus.Running, asyncValidator.Status);
            Assert.AreEqual(asyncValidator, validationView[currentRow].RunningAsyncValidators);
            Assert.AreEqual(0, validationView[currentRow].CompletedAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].FaultedAsyncValidators.Count);

            asyncValidator.Reset();
            inputManager.InvalidateView();
            Assert.AreEqual(AsyncValidatorStatus.Created, asyncValidator.Status);
            Assert.IsNull(asyncValidator.Exception);
            Assert.AreEqual(0, validationView[currentRow].RunningAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].CompletedAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].FaultedAsyncValidators.Count);

            await asyncValidator.LastRunningTask;
            inputManager.InvalidateView();
            Assert.AreEqual(AsyncValidatorStatus.Created, asyncValidator.Status);
            Assert.IsNull(asyncValidator.Exception);
            Assert.AreEqual(0, validationView[currentRow].RunningAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].CompletedAsyncValidators.Count);
            Assert.AreEqual(0, validationView[currentRow].FaultedAsyncValidators.Count);
        }

        [TestMethod]
        public void InputManager_ShowValidationResult()
        {
            var dataSet = DataSet<ProductCategory>.New();
            var _ = dataSet._;
            dataSet.Add(new DataRow());

            RowBinding<TextBox> textBox = null;
            var inputManager = dataSet.CreateInputManager(builder =>
            {
                textBox = _.Name.AsTextBox(UpdateSourceTrigger.PropertyChanged);
                builder.GridColumns("100").GridRows("100").AddBinding(0, 0, textBox).WithValidationMode(ValidationMode.Implicit);
            });

            var currentRow = inputManager.CurrentRow;
            Assert.IsNull(_.Name[0]);
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(1, errors.Count);
                var errorMessage = (ValidationMessage)errors[0].ErrorContent;
                Assert.AreEqual("DevZest.Data.Required", errorMessage.Id);
                Assert.AreEqual(_.Name, errorMessage.Source);
            }

            var validationMessage = new ValidationMessage("ERR-RESULT", ValidationSeverity.Error, "Result Error", _.Name);
            IValidationResult validationResult = DevZest.Data.ValidationResult.Empty.Add(new ValidationEntry(currentRow.DataRow, validationMessage));
            inputManager.Show(validationResult);
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(1, errors.Count);
                Assert.AreEqual(validationMessage, errors[0].ErrorContent);
            }

            textBox[currentRow].Text = "any value";
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(0, errors.Count);
            }
        }

        // http://stackoverflow.com/questions/14087257/how-to-add-synchronization-context-to-async-test-method
        private static void RunInWpfSyncContext(Func<Task> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var prevCtx = SynchronizationContext.Current;
            try
            {
                var syncCtx = new DispatcherSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                var task = func();
                if (task == null)
                    throw new InvalidOperationException();

                var frame = new DispatcherFrame();
                var t2 = task.ContinueWith(x => { frame.Continue = false; }, TaskScheduler.Default);
                Dispatcher.PushFrame(frame);   // execute all tasks until frame.Continue == false

                task.GetAwaiter().GetResult(); // rethrow exception when task has failed 
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }

    }
}
