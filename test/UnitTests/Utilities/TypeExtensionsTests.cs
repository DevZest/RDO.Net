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
            Assert.AreSame(ErrorMessage, func(null).Eval());
        }

        private const string ErrorMessage = "This is a error message.";

        private static _String GetErrorMessage(Column column)
        {
            return ErrorMessage;
        }
    }
}
