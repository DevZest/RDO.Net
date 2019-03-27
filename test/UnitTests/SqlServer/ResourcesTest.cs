using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class ResourcesTest
    {
        [TestMethod]
        public void Resources_correctly_embeded()
        {
            Assert.IsNotNull(DiagnosticMessages.ColumnTypeNotSupported(typeof(_String)));
        }
    }
}
