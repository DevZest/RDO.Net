using DevZest.Data.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DevZest.Data.Presenters
{
    [TestClass]
    public class ValidationViewTests
    {
        [TestMethod]
        public void ValidationView_RefreshStatus()
        {
            var asyncValidator = (new Mock<RowAsyncValidator>() { CallBase = true }).Object;
            Assert.AreEqual(1, ((IRowAsyncValidators)asyncValidator).Count);
            var validationView = new ValidationView();

            validationView.AsyncValidators = asyncValidator;
            Assert.AreEqual(1, validationView.AsyncValidators.Count);
            Assert.AreEqual(RowAsyncValidators.Empty, validationView.RunningAsyncValidators);
            Assert.AreEqual(RowAsyncValidators.Empty, validationView.CompletedAsyncValidators);
            Assert.AreEqual(RowAsyncValidators.Empty, validationView.FaultedAsyncValidators);

            asyncValidator.Status = AsyncValidatorStatus.Running;
            validationView.RefreshStatus();
            Assert.AreEqual(asyncValidator, validationView.RunningAsyncValidators);
            Assert.AreEqual(RowAsyncValidators.Empty, validationView.CompletedAsyncValidators);
            Assert.AreEqual(RowAsyncValidators.Empty, validationView.FaultedAsyncValidators);

            asyncValidator.Status = AsyncValidatorStatus.Completed;
            validationView.RefreshStatus();
            Assert.AreEqual(RowAsyncValidators.Empty, validationView.RunningAsyncValidators);
            Assert.AreEqual(asyncValidator, validationView.CompletedAsyncValidators);
            Assert.AreEqual(RowAsyncValidators.Empty, validationView.FaultedAsyncValidators);

            asyncValidator.Status = AsyncValidatorStatus.Faulted;
            validationView.RefreshStatus();
            Assert.AreEqual(RowAsyncValidators.Empty, validationView.RunningAsyncValidators);
            Assert.AreEqual(RowAsyncValidators.Empty, validationView.CompletedAsyncValidators);
            Assert.AreEqual(asyncValidator, validationView.FaultedAsyncValidators);

        }
    }
}
