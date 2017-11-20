using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Utilities
{
    [TestClass]
    public class TypeExtensionsTests
    {
        [TestMethod]
        public void Type_GetErrorMessageFunc()
        {
            var func = GetType().GetMessageFunc(nameof(GetErrorMessage));
            Assert.AreSame(ErrorMessage, func(null, null));
        }

        private const string ErrorMessage = "This is a error message.";

        private static string GetErrorMessage(Column column, DataRow dataRow)
        {
            return ErrorMessage;
        }
    }
}
