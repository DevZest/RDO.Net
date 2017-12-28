using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Views
{
    [TestClass]
    public class ValidationMessageViewTests
    {
        [TestMethod]
        public void ValidationMessageView_Templates_not_null()
        {
            Assert.IsNotNull(ValidationMessageView.Templates.ValidationError.GetOrLoad());
            Assert.IsNotNull(ValidationMessageView.Templates.ValidationWarning.GetOrLoad());
        }
    }
}
