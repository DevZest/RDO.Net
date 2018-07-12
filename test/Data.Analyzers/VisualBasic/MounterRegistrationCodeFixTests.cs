using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis.VisualBasic
{
    [TestClass]
    public class MounterRegistrationCodeFixTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new MounterRegistrationAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new MounterRegistrationCodeFixProvider();
        }

        [TestMethod]
        public void RegisterLocalColumn_no_static_constructor()
        {
            var test = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

    Private m_Column1 As LocalColumn(Of Int32)
    Public Property Column1 As LocalColumn(Of Int32)
        Get
            Return m_Column1
        End Get
        Private Set
            m_Column1 = Value
        End Set
    End Property
End Class";
            var expected = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

    Shared Sub New()
        RegisterLocalColumn(Function(x As SimpleModel) x.Column1)
    End Sub

    Private m_Column1 As LocalColumn(Of Int32)
    Public Property Column1 As LocalColumn(Of Int32)
        Get
            Return m_Column1
        End Get
        Private Set
            m_Column1 = Value
        End Set
    End Property
End Class";
            VerifyBasicFix(test, expected, allowNewCompilerDiagnostics: true);
        }

        [TestMethod]
        public void RegisterLocalColumn_empty_static_constructor()
        {
            var test = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

    Shared Sub New()
    End Sub

    Private m_Column1 As LocalColumn(Of Int32)
    Public Property Column1 As LocalColumn(Of Int32)
        Get
            Return m_Column1
        End Get
        Private Set
            m_Column1 = Value
        End Set
    End Property
End Class";
            var expected = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

    Shared Sub New()
        RegisterLocalColumn(Function(x As SimpleModel) x.Column1)
    End Sub

    Private m_Column1 As LocalColumn(Of Int32)
    Public Property Column1 As LocalColumn(Of Int32)
        Get
            Return m_Column1
        End Get
        Private Set
            m_Column1 = Value
        End Set
    End Property
End Class";
            VerifyBasicFix(test, expected, allowNewCompilerDiagnostics: true);
        }

        [TestMethod]
        public void RegisterColumn()
        {
            var test = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

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

            var expected = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

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
            VerifyBasicFix(test, expected);
        }

        [TestMethod]
        public void RegisterColumn_existing_mounter()
        {
            var test = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

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

    Private m_Column2 As _Int32
    Public Property Column2 As _Int32
        Get
            Return m_Column2
        End Get
        Private Set
            m_Column2 = Value
        End Set
    End Property
End Class";

            var expected = @"
Imports DevZest.Data

Class SimpleModel
    Inherits Model

    Protected Shared ReadOnly _Column1 As Mounter(Of _Int32) = RegisterColumn(Function(x As SimpleModel) x.Column1)
    Protected Shared ReadOnly _Column2 As Mounter(Of _Int32) = RegisterColumn(Function(x As SimpleModel) x.Column2)
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
End Class";
            VerifyBasicFix(test, expected);
        }

        [TestMethod]
        public void RegisterColumn_abstract_generic()
        {
            var test = @"
Imports DevZest.Data

MustInherit Class SimpleModel(Of T As PrimaryKey)
    Inherits Model(Of T)

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

            var expected = @"
Imports DevZest.Data

MustInherit Class SimpleModel(Of T As PrimaryKey)
    Inherits Model(Of T)

    Protected Shared ReadOnly _Column1 As Mounter(Of _Int32) = RegisterColumn(Function(x As SimpleModel(Of T)) x.Column1)
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
            VerifyBasicFix(test, expected);
        }

        [TestMethod]
        public void RegisterChildModel_abstract_generic_parent_model()
        {
            var test = @"
Imports DevZest.Data

Class ChildModel
    Inherits Model
End Class

MustInherit Class SimpleModel(Of T As PrimaryKey)
    Inherits Model(Of T)

    Private m_Child As ChildModel
    Public Property Child As ChildModel
        Get
            Return m_Child
        End Get
        Private Set
            m_Child = Value
        End Set
    End Property
End Class";
            var expected = @"
Imports DevZest.Data

Class ChildModel
    Inherits Model
End Class

MustInherit Class SimpleModel(Of T As PrimaryKey)
    Inherits Model(Of T)

    Shared Sub New()
        RegisterChildModel(Function(x As SimpleModel(Of T)) x.Child)
    End Sub

    Private m_Child As ChildModel
    Public Property Child As ChildModel
        Get
            Return m_Child
        End Get
        Private Set
            m_Child = Value
        End Set
    End Property
End Class";
            VerifyBasicFix(test, expected);
        }

        [TestMethod]
        public void RegisterChildModel_recursive_child()
        {
            var test = @"
Imports DevZest.Data

Public Class RecursiveModel
    Inherits Model(Of PK)

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
    End Class

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(ID)
    End Function

    Protected Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As RecursiveModel) x.ID)
    Protected Shared ReadOnly _ParentID As Mounter(Of _Int32) = RegisterColumn(Function(x As RecursiveModel) x.ParentID)

    Private m_ID As _Int32
    Public Property ID As _Int32
        Get
            Return m_ID
        End Get
        Private Set
            m_ID = Value
        End Set
    End Property

    Private m_ParentID As _Int32
    Public Property ParentID As _Int32
        Get
            Return m_ParentID
        End Get
        Private Set
            m_ParentID = Value
        End Set
    End Property

    Private m_Child As RecursiveModel
    Public Property Child As RecursiveModel
        Get
            Return m_Child
        End Get
        Set
            m_Child = Value
        End Set
    End Property

    Private m_FK_Parent As PK
    Public ReadOnly Property FK_Parent As PK
        Get
            If m_FK_Parent Is Nothing Then
                m_FK_Parent = New PK(ParentID)
            End If
            Return m_FK_Parent
        End Get
    End Property
End Class";

            var expected = @"
Imports DevZest.Data

Public Class RecursiveModel
    Inherits Model(Of PK)

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
    End Class

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(ID)
    End Function

    Protected Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As RecursiveModel) x.ID)
    Protected Shared ReadOnly _ParentID As Mounter(Of _Int32) = RegisterColumn(Function(x As RecursiveModel) x.ParentID)

    Shared Sub New()
        RegisterChildModel(Function(x As RecursiveModel) x.Child, Function(x As RecursiveModel) x.FK_Parent)
    End Sub

    Private m_ID As _Int32
    Public Property ID As _Int32
        Get
            Return m_ID
        End Get
        Private Set
            m_ID = Value
        End Set
    End Property

    Private m_ParentID As _Int32
    Public Property ParentID As _Int32
        Get
            Return m_ParentID
        End Get
        Private Set
            m_ParentID = Value
        End Set
    End Property

    Private m_Child As RecursiveModel
    Public Property Child As RecursiveModel
        Get
            Return m_Child
        End Get
        Set
            m_Child = Value
        End Set
    End Property

    Private m_FK_Parent As PK
    Public ReadOnly Property FK_Parent As PK
        Get
            If m_FK_Parent Is Nothing Then
                m_FK_Parent = New PK(ParentID)
            End If
            Return m_FK_Parent
        End Get
    End Property
End Class";
            VerifyBasicFix(test, expected);
        }
    }
}
