#If Not DbInit Then

Imports System.Runtime.CompilerServices
Imports DevZest.Data

Module DataSetExtensions
    <Extension()>
    Public Function AddRows(Of T As {Class, IEntity, New})(ByVal dataSet As DataSet(Of T), ByVal count As Integer) As DataSet(Of T)
        For i = 1 To count
            dataSet.AddRow()
        Next
        Return dataSet
    End Function
End Module

#End If

