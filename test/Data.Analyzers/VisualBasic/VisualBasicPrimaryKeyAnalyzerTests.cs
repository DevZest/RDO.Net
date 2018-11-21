using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis.VisualBasic
{
    [TestClass]
    public class VisualBasicPrimaryKeyAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new VisualBasicPrimaryKeyAnalyzer();
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

        [TestMethod]
        public void InvalidConstructorParam()
        {
            var test = @"
Imports DevZest.Data

Public NotInheritable Class PK
    Inherits PrimaryKey

    Public Sub New(id As Int32)
        MyBase.New()
    End Sub
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyInvalidConstructorParam,
                Message = string.Format(Resources.PrimaryKeyInvalidConstructorParam_Message, "id"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 7, 20) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void SortAttributeConflict()
        {
            var test = @"
Imports DevZest.Data
Imports DevZest.Data.Annotations

Public NotInheritable Class PK
    Inherits PrimaryKey

    Public Sub New(<Asc(), Desc()>id As _Int32)
        MyBase.New(id)
    End Sub
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeySortAttributeConflict,
                Message = string.Format(Resources.PrimaryKeySortAttributeConflict_Message),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 8, 28) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void MissingBaseConstructor()
        {
            var test = @"
Imports DevZest.Data

Public NotInheritable Class PK
    Inherits PrimaryKey

    Public Sub New(id As _Int32)
    End Sub
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyMissingBaseConstructor,
                Message = string.Format(Resources.PrimaryKeyMissingBaseConstructor_Message),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 7, 16) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void MismatchBaseConstructor()
        {
            var test = @"
Imports DevZest.Data

Public NotInheritable Class PK
    Inherits PrimaryKey

    Public Sub New(id As _Int32)
        MyBase.New()
    End Sub
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyMismatchBaseConstructor,
                Message = string.Format(Resources.PrimaryKeyMismatchBaseConstructor_Message, 1),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 8, 9) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void MismatchBaseConstructorArgument()
        {
            var test = @"
Imports DevZest.Data

Public NotInheritable Class PK
    Inherits PrimaryKey

    Public Sub New(id1 As _Int32, id2 As _Int32)
        MyBase.New(id1, id1)
    End Sub
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyMismatchBaseConstructorArgument,
                Message = string.Format(Resources.PrimaryKeyMismatchBaseConstructorArgument_Message, "id2"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 8, 25) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void MismatchSortAttribute()
        {
            var test = @"
Imports DevZest.Data

Public NotInheritable Class PK
    Inherits PrimaryKey

    Public Sub New(id As _Int32)
        MyBase.New(id.Asc())
    End Sub
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyMismatchSortAttribute,
                Message = string.Format(Resources.PrimaryKeyMismatchSortAttribute_Message, "Unspecified", "Ascending"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 7, 20) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void InvalidArgument()
        {
            var test = @"
Imports DevZest.Data

Public Class PrimaryKeyInvalidArgument
    Inherits Model(Of PrimaryKeyInvalidArgument.PK)

    Public NotInheritable Class PK
        Inherits PrimaryKey
        Public Sub New(id As _Int32)
            MyBase.New(id)
        End Sub
    End Class

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(1)
    End Function

    Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As PrimaryKeyInvalidArgument) x.ID)

    Private m_ID As _Int32
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
                Id = DiagnosticIds.PrimaryKeyInvalidArgument,
                Message = string.Format(Resources.PrimaryKeyInvalidArgument_Message),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 15, 23) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void ArgumentNaming()
        {
            var test = @"
Imports DevZest.Data
Imports DevZest.Data.Annotations

Public Class PrimaryKeyArgumentNaming
    Inherits Model(Of PrimaryKeyArgumentNaming.PK)

    Public NotInheritable Class PK
        Inherits PrimaryKey
        Public Sub New(id2 As _Int32)
            MyBase.New(id2)
        End Sub
    End Class

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(ID)
    End Function

    Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As PrimaryKeyArgumentNaming) x.ID)

    Private m_ID As _Int32
    <PkColumn>
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
                Id = DiagnosticIds.PrimaryKeyArgumentNaming,
                Message = string.Format(Resources.PrimaryKeyArgumentNaming_Message, "ID", "id2"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 16, 23) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void PkColumnAttributeMissing()
        {
            var test = @"
Imports DevZest.Data

Public Class PrimaryKeyArgumentMissing
    Inherits Model(Of PrimaryKeyArgumentMissing.PK)

    Public NotInheritable Class PK
        Inherits PrimaryKey
        Public Sub New(id As _Int32)
            MyBase.New(id)
        End Sub
    End Class

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(ID)
    End Function

    Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As PrimaryKeyArgumentMissing) x.ID)

    Private m_ID As _Int32
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
                Id = DiagnosticIds.PkColumnAttributeMissing,
                Message = string.Format(Resources.PkColumnAttributeMissing_Message, 0),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 15, 23) }
            };

            VerifyBasicDiagnostic(test, expected);
        }
    }
}
