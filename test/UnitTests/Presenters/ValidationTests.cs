using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Presenters
{
    [TestClass]
    public class ValidationTests
    {
        [TestMethod]
        public void Validation_Templates_not_null()
        {
            Assert.IsNotNull(Validation.Templates.Failed);
            Assert.IsNotNull(Validation.Templates.Validating);
            Assert.IsNotNull(Validation.Templates.Failed);
        }
    }
}
