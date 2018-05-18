Imports DevZest.Data
Imports DevZest.Data.Primitives

Public Module [Global]
    Public Function ModelOf(Of T As {Model, New})(dataSet As DataSet(Of T)) As T
        Return dataSet.GetModel()
    End Function

    Public Function ModelOf(Of T As {Model, New})(dbSet As DbSet(Of T)) As T
        Return dbSet.GetModel()
    End Function
End Module
