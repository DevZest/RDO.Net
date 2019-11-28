using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class DbInitMapperTests
    {
        private static readonly MetadataReference DbInitReference = MetadataReference.CreateFromFile(typeof(DbInit.InputAttribute).Assembly.Location);

        [TestMethod]
        public void DbInitMapper_GetDbInitInput()
        {
            var src =
@"using DevZest.Data.DbInit;

private class TestObject
{
    [Input(IsPassword = true, Order = 2)]
    public string Password { get; set; }

    [Input(Title = ""User Name"", EnvironmentVariableName = ""DbInit_UserName"",  Order = 1)]
    public string UserName { get; set; }
}";

            var document = src.CreateDocument(DbInitReference);
            var codeContext = CodeContext.Create(document, new TextSpan(65, 0));
            var type = codeContext.GetCurrentType("System.Object");
            Assert.IsNotNull(type);

            var result = type.GetDbInitInput(codeContext.Compilation);
            Assert.AreEqual(2, result.Count);
            var _ = result._;
            Assert.AreEqual("User Name", _.Title[0]);
            Assert.AreEqual("DbInit_UserName", _.EnvironmentVariableName[0]);
            Assert.AreEqual(false, _.IsPassword[0]);
            Assert.AreEqual(1, _.Order[0]);
            Assert.AreEqual("Password", _.Title[1]);
            Assert.AreEqual("Password", _.EnvironmentVariableName[1]);
            Assert.AreEqual(true, _.IsPassword[1]);
            Assert.AreEqual(2, _.Order[1]);
        }
    }
}
