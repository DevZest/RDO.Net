using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations.Primitives
{
    [TestClass]
    public class GeneralValidationColumnAttributeTests
    {
        private const string ERROR_MESSAGE = "This is a error message.";

        private static string GetErrorMessage(Column column, DataRow dataRow)
        {
            return ERROR_MESSAGE;
        }

        [TestMethod]
        public void GeneralValidationColumnAttribute_ResourceType()
        {
            var messageGetter = GeneralValidationColumnAttribute.GetMessageGetter(typeof(GeneralValidationColumnAttributeTests), nameof(GetErrorMessage));
            Assert.AreEqual(ERROR_MESSAGE, messageGetter(null, null));
        }
    }
}
