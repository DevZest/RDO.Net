using Microsoft.CodeAnalysis;
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

        [TestMethod]
        public void InvalidInvocation_assigned_to_local_variable()
        {
            var test = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

    Protected Shared ReadOnly _Column1 As Mounter(Of _Int32) = RegisterColumn(Function(x As SimpleModel) x.Column1)

    Shared Sub New()
        Dim column1 = RegisterColumn(Function(x As SimpleModel) x.Column1)
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
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.InvalidRegisterMounterInvocation,
                Message = Resources.InvalidRegisterMounterInvocation_Message,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 10, 23) }
            };
            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void InvalidInvocation_in_a_separate_method()
        {
            var test = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

    Protected Shared ReadOnly _Column1 As Mounter(Of _Int32) = RegisterColumn(Function(x As SimpleModel) x.Column1)

    Private Shared Sub AnotherMethod()
        _Column1 = RegisterColumn(Function(x As SimpleModel) x.Column1)
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
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.InvalidRegisterMounterInvocation,
                Message = Resources.InvalidRegisterMounterInvocation_Message,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 10, 20) }
            };
            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void DuplicateRegistration_in_static_constructor()
        {
            var test = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

    Protected Shared ReadOnly _Column1 As Mounter(Of _Int32) = RegisterColumn(Function(x As SimpleModel) x.Column1)

    Shared Sub New()
        RegisterColumn(Function(x As SimpleModel) x.Column1)
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
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.DuplicateMounterRegistration,
                Message = string.Format(Resources.DuplicateMounterRegistration_Message, "Column1"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 10, 9) }
            };
            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void DuplicateRegistration_in_field_initializer()
        {
            var test = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

    Shared Sub New()
        RegisterColumn(Function(x As SimpleModel) x.Column1)
    End Sub

    Protected Shared ReadOnly _Column1 As Mounter(Of _Int32) = RegisterColumn(Function(x As SimpleModel) x.Column1)

    Private m_Column1 As _Int32
    Public Property Column1 As _Int32
        Get
            Return m_Column1
        End Get
        Private Set
            m_Column1 = Value
        End Set
    End Property
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.DuplicateMounterRegistration,
                Message = string.Format(Resources.DuplicateMounterRegistration_Message, "Column1"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 11, 64) }
            };
            VerifyBasicDiagnostic(test, expected);
        }
    }
}