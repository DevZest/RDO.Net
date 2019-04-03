using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Views
{
    [TestClass]
    public class ValidationErrorsControlTests
    {
        [TestMethod]
        public void ValidationErrorsControl_Templates_not_null()
        {
            Assert.IsNotNull(ValidationErrorsControl.Templates.Failed);
        }
    }
}
