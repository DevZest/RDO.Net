using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class KnownTypesTests
    {
        [TestMethod]
        public void KnownTypes_GetKnownType_not_null()
        {
            var project = string.Empty.CreateProject();
            var compilation = project.GetCompilationAsync().Result;

            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.AttributeUsageAttribute));

            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.Model));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.GenericModel));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.Column));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.LocalColumn));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.ColumnList));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.Projection));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.CandidateKey));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.AscAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.DescAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.PropertyRegistrationAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.CreateKeyAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.ModelDeclarationAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.ModelImplementationAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.CrossReferenceAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.ModelDeclarationSpecAttribute));

            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.ModelDesignerSpecAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.AddonAttribute));

            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.DbSession));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.DbTable));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.RelationshipAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes._RelationshipAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.KeyMapping));
        }
    }
}
