using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DevZest.Data.Utilities
{
    [TestClass]
    public class TypeExtensionsTests
    {
        [TestMethod]
        public void Type_GetMessageFunc()
        {
            var func = GetType().GetMessageFunc(nameof(GetErrorMessage));
            Assert.AreSame(ErrorMessage, func(null, null));
        }

        private const string ErrorMessage = "This is a error message.";

        private static string GetErrorMessage(Column column, DataRow dataRow)
        {
            return ErrorMessage;
        }

        [TestMethod]
        public void Type_GetColumnsErrorMessageFunc()
        {
            var func = GetType().GetColumnsMessageFunc(nameof(GetColumnsErrorMessage));
            Assert.AreSame(ErrorMessage, func(null, null, null));
        }

        private static string GetColumnsErrorMessage(string attributeName, IReadOnlyList<Column> columns, DataRow dataRow)
        {
            return ErrorMessage;
        }
    }
}
