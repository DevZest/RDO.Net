using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Utilities
{
    [TestClass]
    public class TypeExtensionsTests
    {
        private const string ERROR_MESSAGE = "This is a error message.";

        private static string ErrorMessage
        {
            get { return ERROR_MESSAGE; }
        }

        [TestMethod]
        public void TypeExtensions_ResolveStringGetter()
        {
            var stringGetter = typeof(TypeExtensionsTests).ResolveStringGetter(nameof(ErrorMessage));
            Assert.AreEqual(ERROR_MESSAGE, stringGetter());
        }
    }
}
