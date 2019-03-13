<UniqueConstraint("AK_RowGuid", DbName:="AK_%_rowguid", Description:="Unique nonclustered constraint. Used to support replication samples.")>
Public MustInherit Class BaseModel(Of T As CandidateKey)
    Inherits Model(Of T)

    Shared Sub New()
        RegisterColumn(Function(x As BaseModel(Of T)) x.RowGuid)
        RegisterColumn(Function(x As BaseModel(Of T)) x.ModifiedDate)
    End Sub

    Private m_RowGuid As _Guid
    <Required>
    <AutoGuid(Name:="DF_%_rowguid", Description:="Default constraint value of NEWID()")>
    <DbColumn(Description:="ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")>
    Public Property RowGuid As _Guid
        Get
            Return m_RowGuid
        End Get
        Private Set
            m_RowGuid = Value
        End Set
    End Property

    Private _ModifiedDate As _DateTime
    <Required>
    <SqlDateTime>
    <AutoDateTime(Name:="DF_%_ModifiedDate", Description:="Default constraint value of GETDATE()")>
    <DbColumn(Description:="Date and time the record was last updated.")>
    Public Property ModifiedDate As _DateTime
        Get
            Return _ModifiedDate
        End Get
        Private Set
            _ModifiedDate = Value
        End Set
    End Property

    Public Sub ResetRowIdentifiers()
        Dim vDataSet = DataSet
        If vDataSet Is Nothing Then Return

        For i As Integer = 0 To vDataSet.Count - 1
            RowGuid(i) = Guid.NewGuid()
            ModifiedDate(i) = DateTime.Now
        Next
    End Sub

    <_UniqueConstraint>
    Private ReadOnly Property AK_RowGuid As ColumnSort()
        Get
            Return New ColumnSort() {RowGuid}
        End Get
    End Property
End Class
