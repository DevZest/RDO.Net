using DevZest.Data.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class ModelMemberAttributeAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ModelMemberAttributeAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ModelMemberAttributeAnalyzer();
        }

        [TestMethod]
        public void InvalidModelMemberAttribute_CS()
        {
            var test =
@"using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class InvalidModelMemberAttribute : Model
    {
        static InvalidModelMemberAttribute()
        {
            RegisterColumn((InvalidModelMemberAttribute _) => _.Id);
        }

        [CreditCard]
        public _Int32 Id { get; private set; }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.InvalidModelMemberAttribute,
                Message = string.Format(Resources.InvalidModelMemberAttribute_Message, typeof(CreditCardAttribute), "DevZest.Data.Column<string>", typeof(_Int32)),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void InvalidModelMemberAttribute_VB()
        {
            var test =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Public Class InvalidModelMemberAttribute
    Inherits Model

    Shared Sub New()
        RegisterColumn(Function(x As InvalidModelMemberAttribute) x.ID)
    End Sub

    Private m_ID As _Int32
    <CreditCard>
    Public Property ID As _Int32
        Get
            Return m_ID
        End Get
        Private Set
            m_ID = Value
        End Set
    End Property
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.InvalidModelMemberAttribute,
                Message = string.Format(Resources.InvalidModelMemberAttribute_Message, typeof(CreditCardAttribute), "DevZest.Data.Column(Of String)", typeof(_Int32)),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 12, 6) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void ModelMemberAttributeRequiresArgument_CS()
        {
            var test =
@"using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class ModelMemberAttributeRequiresArgument : Model
    {
        static ModelMemberAttributeRequiresArgument()
        {
            RegisterColumn((ModelMemberAttributeRequiresArgument _) => _.Id);
        }

        [DbColumn]
        public _Int32 Id { get; private set; }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.ModelMemberAttributeRequiresArgument,
                Message = string.Format(Resources.ModelMemberAttributeRequiresArgument_Message, typeof(DbColumnAttribute)),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void ModelMemberAttributeRequiresArgument_VB()
        {
            var test =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Public Class ModelMemberAttributeRequiresArgument
    Inherits Model

    Shared Sub New()
        RegisterColumn(Function(x As ModelMemberAttributeRequiresArgument) x.ID)
    End Sub

    Private m_ID As _Int32
    <DbColumn>
    Public Property ID As _Int32
        Get
            Return m_ID
        End Get
        Private Set
            m_ID = Value
        End Set
    End Property
End Class
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.ModelMemberAttributeRequiresArgument,
                Message = string.Format(Resources.ModelMemberAttributeRequiresArgument_Message, typeof(DbColumnAttribute)),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 12, 6) }
            };

            VerifyBasicDiagnostic(test, expected);
        }
    }
}

