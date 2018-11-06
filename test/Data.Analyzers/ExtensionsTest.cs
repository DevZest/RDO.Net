using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class ExtensionsTest
    {
        [TestMethod]
        public void GetAttributeTargets()
        {
            var project = string.Empty.CreateProject();
            var compilation = project.GetCompilationAsync().Result;

            var dbColumnAttribute = compilation.GetTypeByMetadataName("DevZest.Data.Annotations.DbColumnAttribute");
            Assert.AreEqual(System.AttributeTargets.Property, dbColumnAttribute.GetAttributeTargets(compilation));
        }
    }
}
