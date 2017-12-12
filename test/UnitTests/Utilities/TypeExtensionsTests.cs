using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DevZest.Data.Utilities
{
    [TestClass]
    public class TypeExtensionsTests
    {
        private const string ErrorMessage = "This is a error message.";

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
