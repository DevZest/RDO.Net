using DevZest.Data.DbInit;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class KnownTypesTests
    {
        [TestMethod]
        public void KnownTypes_GetKnownType_not_null()
        {
            var project = string.Empty.CreateProject(additionalReference: MetadataReference.CreateFromFile(typeof(EmptyDbAttribute).Assembly.Location));
            var compilation = project.GetCompilationAsync().Result;

            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.KeyOf));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.RefOf));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.MounterOf));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.CheckConstraintAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.UniqueConstraintAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.CustomValidatorAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.DbIndexAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes._CheckConstraintAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes._UniqueConstraintAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes._CustomValidatorAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes._DbIndexAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.ColumnSort));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.MessageResourceAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes._Boolean));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.NotImplementedException));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.ComputationMode));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.ComputationAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes._ComputationAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.DataRow));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.DataValidationError));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.InvisibleToDbDesignerAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.DbTableAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.ForeignKeyRule));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.DbSessionProviderOf));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.DbInitializerOf));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.EmptyDbAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.InputAttribute));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.DataSetOf));
            Assert.IsNotNull(compilation.GetKnownType(KnownTypes.IColumns));
        }
    }
}
