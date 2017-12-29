using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Views
{
    [TestClass]
    public class ValidationMessagesControlTests
    {
        [TestMethod]
        public void ValidationMessagesControl_Templates_not_null()
        {
            Assert.IsNotNull(ValidationMessagesControl.Templates.ValidationError);
            Assert.IsNotNull(ValidationMessagesControl.Templates.ValidationWarning);
        }
    }
}
