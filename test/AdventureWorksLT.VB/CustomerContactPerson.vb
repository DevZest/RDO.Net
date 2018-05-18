Imports DevZest.Data
Imports System.Runtime.CompilerServices

Namespace DevZest.Samples.AdventureWorksLT
    Module CustomerContactPerson
        Private NotInheritable Class Manager
            Inherits AttachedLocalColumnManager(Of Customer, String)

            Public Shared ReadOnly Singleton As Manager = New Manager()

            Private Sub New()
            End Sub

            Protected Overrides Function CreateLocalColumn(dataSetContainer As DataSetContainer, __ As Customer) As Column(Of String)
                Return dataSetContainer.CreateLocalColumn(__, __.LastName, __.FirstName, __.Title, AddressOf GetContactPerson)
            End Function
        End Class

        <Extension()>
        Function GetContactPerson(ByVal __ As Customer) As Column(Of String)
            Return Manager.Singleton.GetAttachedColumn(__)
        End Function

        Public ReadOnly Property Initializer As Action(Of Customer)
            Get
                Return Manager.Singleton.Initializer
            End Get
        End Property

        Private Function GetContactPerson(ByVal dataRow As DataRow, ByVal lastName As _String, ByVal firstName As _String, ByVal title As _String) As String
            Return GetContactPerson(lastName(dataRow), firstName(dataRow), title(dataRow))
        End Function

        Function GetContactPerson(ByVal lastName As String, ByVal firstName As String, ByVal title As String) As String
            Dim result As String = If(String.IsNullOrEmpty(lastName), String.Empty, lastName.ToUpper())

            If Not String.IsNullOrEmpty(firstName) Then
                If result.Length > 0 Then result += ", "
                result += firstName
            End If

            If Not String.IsNullOrEmpty(title) Then
                result += " ("
                result += title
                result += ")"
            End If

            Return result
        End Function
    End Module
End Namespace
