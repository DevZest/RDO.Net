using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations.Primitives
{
    [TestClass]
    public class GeneralValidationModelWireupAttributeTests
    {
        private class TestModel : Model
        {
        }

        private const string ERROR_MESSAGE = "This is a error message.";

        private static string GetErrorMessage(TestModel model, DataRow dataRow)
        {
            return ERROR_MESSAGE;
        }

        [TestMethod]
        public void GeneralValidationModelWireupAttribute_GetMessageGetter()
        {
            var messageGetter = GeneralValidationModelWireupAttribute.GetMessageGetter(typeof(TestModel), typeof(GeneralValidationModelWireupAttributeTests), nameof(GetErrorMessage));
            Assert.AreEqual(ERROR_MESSAGE, messageGetter(null, null));
        }
    }
}
