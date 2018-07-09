using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Analyzers.VisualBasic
{
    [TestClass]
    public class MounterRegistrationAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new MounterRegistrationAnalyzer();
        }

        [TestMethod]
        public void EmptySource_generates_no_diagnostics()
        {
            var test = @"";

            VerifyBasicDiagnostic(test);
        }

        [TestMethod]
        public void RegisterColumn_with_no_error()
        {
            var test = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

    Protected Shared ReadOnly _Column1 As Mounter(Of _Int32) = RegisterColumn(Function(x As SimpleModel) x.Column1)
    Protected Shared ReadOnly _Column2 As Mounter(Of _Int32)

    Shared Sub New()
        _Column2 = RegisterColumn(Function(x As SimpleModel) x.Column2)
        RegisterColumn(Function(x As SimpleModel) x.Column3)
    End Sub

    Private m_Column1 As _Int32
    Public Property Column1 As _Int32
        Get
            Return m_Column1
        End Get
        Private Set
            m_Column1 = Value
        End Set
    End Property

    Private m_Column2 As _Int32
    Public Property Column2 As _Int32
        Get
            Return m_Column2
        End Get
        Private Set
            m_Column2 = Value
        End Set
    End Property

    Private m_Column3 As _Int32
    Public Property Column3 As _Int32
        Get
            Return m_Column3
        End Get
        Private Set
            m_Column3 = Value
        End Set
    End Property
End Class";

            VerifyBasicDiagnostic(test);
        }
    }
}