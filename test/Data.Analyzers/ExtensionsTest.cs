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
            Assert.IsNotNull(dbColumnAttribute);
            Assert.AreEqual(System.AttributeTargets.Property, dbColumnAttribute.GetAttributeTargets(compilation));
        }

        [TestMethod]
        public void GetModelMemberAttributeSpec()
        {
            var project = string.Empty.CreateProject();
            var compilation = project.GetCompilationAsync().Result;

            var identityAttribute = compilation.GetTypeByMetadataName("DevZest.Data.Annotations.IdentityAttribute");
            Assert.IsNotNull(identityAttribute);

            var result = identityAttribute.GetModelMemberAttributeSpec(compilation).Value;

            {
                var addonTypes = result.AddonTypes;
                Assert.AreEqual(1, addonTypes.Length);
                Assert.IsNotNull(addonTypes[0]);
                Assert.AreEqual(compilation.GetTypeByMetadataName("DevZest.Data.Addons.Identity"), addonTypes[0]);
            }

            {
                var validOnTypes = result.ValidOnTypes;
                Assert.AreEqual(3, validOnTypes.Length);
                Assert.IsNotNull(validOnTypes[0]);
                Assert.AreEqual(compilation.GetTypeByMetadataName("DevZest.Data._Int32"), validOnTypes[0]);
                Assert.IsNotNull(validOnTypes[1]);
                Assert.AreEqual(compilation.GetTypeByMetadataName("DevZest.Data._Int64"), validOnTypes[1]);
                Assert.IsNotNull(validOnTypes[2]);
                Assert.AreEqual(compilation.GetTypeByMetadataName("DevZest.Data._Int16"), validOnTypes[2]);
            }
        }
    }
}
