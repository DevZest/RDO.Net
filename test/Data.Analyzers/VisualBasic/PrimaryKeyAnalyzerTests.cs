using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis.VisualBasic
{
    [TestClass]
    public class PrimaryKeyAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new PrimaryKeyAnalyzer();
        }

        //No diagnostics expected to show up
        [TestMethod]
        public void EmptySource()
        {
            var test = @"";

            VerifyBasicDiagnostic(test);
        }

        [TestMethod]
        public void NoError()
        {
            var test = @"
Imports DevZest.Data

Public NotInheritable Class PK
    Inherits PrimaryKey

    Public Sub New(id As _Int32)
        MyBase.New(id)
    End Sub

    Public ReadOnly Property ID As _Int32
        Get
            Return GetColumn(Of _Int32)(0)
        End Get
    End Property
End Class";

            VerifyBasicDiagnostic(test);
        }

        [TestMethod]
        public void NotSealed()
        {
            var test = @"
Imports DevZest.Data

Public Class PK
    Inherits PrimaryKey

    Public Sub New(id As _Int32)
        MyBase.New(id)
    End Sub

    Public ReadOnly Property ID As _Int32
        Get
            Return GetColumn(Of _Int32)(0)
        End Get
    End Property
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyNotSealed,
                Message = Resources.PrimaryKeyNotSealed_Message,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 4, 14) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void InvalidConstructors()
        {
            var test = @"
Imports DevZest.Data

Public NotInheritable Class PK
    Inherits PrimaryKey

    Public ReadOnly Property ID As _Int32
        Get
            Return GetColumn(Of _Int32)(0)
        End Get
    End Property
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyInvalidConstructors,
                Message = Resources.PrimaryKeyInvalidConstructors_Message,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 4, 29) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void ParameterlessConstructor()
        {
            var test = @"
Imports DevZest.Data

Public NotInheritable Class PK
    Inherits PrimaryKey

    Public Sub New()
        MyBase.New()
    End Sub
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyParameterlessConstructor,
                Message = Resources.PrimaryKeyParameterlessConstructor_Message,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 7, 16) }
            };

            VerifyBasicDiagnostic(test, expected);
        }
    }
}
