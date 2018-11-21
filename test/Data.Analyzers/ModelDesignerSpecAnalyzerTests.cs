using DevZest.Data.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class ModelDesignerSpecAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ModelDesignerSpecAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ModelDesignerSpecAnalyzer();
        }

        [TestMethod]
        public void ModelDesignerSpecInvalidType_CS()
        {
            var test =
@"using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class ModelDesignerSpecInvalidType : Model
    {
        static ModelDesignerSpecInvalidType()
        {
            RegisterColumn((ModelDesignerSpecInvalidType _) => _.Id);
        }

        [CreditCard]
        public _Int32 Id { get; private set; }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.ModelDesignerSpecInvalidType,
                Message = string.Format(Resources.ModelDesignerSpecInvalidType_Message, typeof(CreditCardAttribute), "DevZest.Data.Column<string>", typeof(_Int32)),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void ModelDesignerSpecInvalidType_VB()
        {
            var test =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Public Class ModelDesignerSpecInvalidType
    Inherits Model

    Shared Sub New()
        RegisterColumn(Function(x As ModelDesignerSpecInvalidType) x.ID)
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
                Id = DiagnosticIds.ModelDesignerSpecInvalidType,
                Message = string.Format(Resources.ModelDesignerSpecInvalidType_Message, typeof(CreditCardAttribute), "DevZest.Data.Column(Of String)", typeof(_Int32)),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 12, 6) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void ModelDesignerSpecRequiresArgument_CS()
        {
            var test =
@"using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class ModelDesignerSpecRequiresArgument : Model
    {
        static ModelDesignerSpecRequiresArgument()
        {
            RegisterColumn((ModelDesignerSpecRequiresArgument _) => _.Id);
            RegisterColumn((ModelDesignerSpecRequiresArgument _) => _.Name);
        }

        [DbColumn]
        public _Int32 Id { get; private set; }

        [DbColumn(Description=""Description""]
        public _String Name { get; private set; }        
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.ModelDesignerSpecRequiresArgument,
                Message = string.Format(Resources.ModelDesignerSpecRequiresArgument_Message, typeof(DbColumnAttribute)),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 13, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void ModelDesignerSpecRequiresArgument_VB()
        {
            var test =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Public Class ModelDesignerSpecRequiresArgument
    Inherits Model

    Shared Sub New()
        RegisterColumn(Function(x As ModelDesignerSpecRequiresArgument) x.ID)
        RegisterColumn(Function(x As ModelDesignerSpecRequiresArgument) x.Name)
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

    Private m_Name As _String
    <DbColumn(Description=""Description"")>
    Public Property Name As _String
        Get
            Return m_Name
        End Get
        Private Set
            m_Name = Value
        End Set
    End Property
End Class
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.ModelDesignerSpecRequiresArgument,
                Message = string.Format(Resources.ModelDesignerSpecRequiresArgument_Message, typeof(DbColumnAttribute)),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 13, 6) }
            };

            VerifyBasicDiagnostic(test, expected);
        }
    }
}

