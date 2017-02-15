using System;
using System.Threading.Tasks;
using DevZest.Data.Windows.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class ValidationViewTests
    {
        private sealed class MockAsyncValidator : AsyncValidator
        {
            public override IRowInput RowInput
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override IColumnSet SourceColumns
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override ValidationScope ValidationScope
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            internal override InputManager InputManager
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            protected override Task<IValidationDictionary> ValidateCoreAsync()
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void ValidationView_RefreshStatus()
        {
            AsyncValidator asyncValidator = new MockAsyncValidator();
            Assert.AreEqual(1, ((IAsyncValidatorGroup)asyncValidator).Count);
            var validationView = new ValidationView();

            validationView.AsyncValidators = asyncValidator;
            Assert.AreEqual(1, validationView.AsyncValidators.Count);
            Assert.AreEqual(AsyncValidatorGroup.Empty, validationView.RunningAsyncValidators);
            Assert.AreEqual(AsyncValidatorGroup.Empty, validationView.CompletedAsyncValidators);
            Assert.AreEqual(AsyncValidatorGroup.Empty, validationView.FaultedAsyncValidators);

            asyncValidator.Status = AsyncValidatorStatus.Running;
            validationView.RefreshStatus();
            Assert.AreEqual(asyncValidator, validationView.RunningAsyncValidators);
            Assert.AreEqual(AsyncValidatorGroup.Empty, validationView.CompletedAsyncValidators);
            Assert.AreEqual(AsyncValidatorGroup.Empty, validationView.FaultedAsyncValidators);

            asyncValidator.Status = AsyncValidatorStatus.Completed;
            validationView.RefreshStatus();
            Assert.AreEqual(AsyncValidatorGroup.Empty, validationView.RunningAsyncValidators);
            Assert.AreEqual(asyncValidator, validationView.CompletedAsyncValidators);
            Assert.AreEqual(AsyncValidatorGroup.Empty, validationView.FaultedAsyncValidators);

            asyncValidator.Status = AsyncValidatorStatus.Faulted;
            validationView.RefreshStatus();
            Assert.AreEqual(AsyncValidatorGroup.Empty, validationView.RunningAsyncValidators);
            Assert.AreEqual(AsyncValidatorGroup.Empty, validationView.CompletedAsyncValidators);
            Assert.AreEqual(asyncValidator, validationView.FaultedAsyncValidators);

        }
    }
}
