using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Utilities
{
    [TestClass]
    public class ColumnManagerTests
    {
        private const string ERROR_MESSAGE = "This is a error message.";

        private static string GetErrorMessage(Column column, DataRow dataRow)
        {
            return ERROR_MESSAGE;
        }

        [TestMethod]
        public void ColumnManager_GetMessageGetter()
        {
            var messageGetter = typeof(ColumnManagerTests).GetMessageGetter(nameof(GetErrorMessage));
            Assert.AreEqual(ERROR_MESSAGE, messageGetter(null, null));
        }
    }
}
