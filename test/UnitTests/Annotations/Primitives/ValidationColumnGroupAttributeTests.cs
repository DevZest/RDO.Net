using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DevZest.Data.Annotations.Primitives
{
    [TestClass]
    public class ValidationColumnGroupAttributeTests
    {
        private const string ERROR_MESSAGE = "This is a error message.";

        private static string GetErrorMessage(IReadOnlyList<Column> columns, DataRow dataRow)
        {
            return ERROR_MESSAGE;
        }

        [TestMethod]
        public void ValidationColumnGroupAttribute_GetMessageGetter()
        {
            var messageGetter = ValidationColumnGroupAttribute.GetMessageGetter(typeof(ValidationColumnGroupAttributeTests), nameof(GetErrorMessage));
            Assert.AreEqual(ERROR_MESSAGE, messageGetter(null, null));
        }

    }
}
