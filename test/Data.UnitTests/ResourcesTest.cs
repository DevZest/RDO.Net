using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class ResourcesTest
    {
        [TestMethod]
        public void Resources_correctly_embedded()
        {
            Assert.IsNotNull(DiagnosticMessages.BooleanColumn_CannotDeserialize);
            Assert.IsNotNull(UserMessages.CreditCardAttribute);
        }
    }
}
