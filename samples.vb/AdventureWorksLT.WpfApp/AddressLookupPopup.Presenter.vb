Imports System.Threading
Imports DevZest.Data
Imports DevZest.Data.Presenters
Imports DevZest.Data.Views

Partial Class AddressLookupPopup
    Public NotInheritable Class Commands
        Public Shared ReadOnly Property SelectCurrent() As RoutedUICommand
            Get
                Return ApplicationCommands.Open
            End Get
        End Property
    End Class

    Private NotInheritable Class Presenter
        Inherits DataPresenter(Of Address)
        Implements RowView.ICommandService
        Public Sub New(dataView As DataView, currentAddressID As System.Nullable(Of Integer), customerID As Integer)
            Me.CurrentAddressID = currentAddressID
            Me.CustomerID = customerID
            AddHandler dataView.Loaded, AddressOf OnDataViewLoaded
        End Sub

        Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
            builder.GridColumns("120") _
                .GridRows("Auto") _
                .RowView(Of RowView)(RowView.Styles.Selectable) _
                .Layout(Orientation.Vertical) _
                .WithSelectionMode(SelectionMode.Single) _
                .AddBinding(0, 0, Entity.BindToAddressBox())
        End Sub

        Private Function LoadDataAsync(ct As CancellationToken) As Task(Of DataSet(Of Address))
            Return App.ExecuteAsync(Function(db) db.GetAddressLookupAsync(CustomerID, ct))
        End Function

        Public Property CurrentAddressID() As System.Nullable(Of Integer)

        Public Property CustomerID() As Integer

        Private Sub OnDataViewLoaded(sender As Object, e As RoutedEventArgs)
            Dim dataView As DataView = CType(sender, DataView)
            RemoveHandler dataView.Loaded, AddressOf OnDataViewLoaded
            ShowAsync(dataView)
        End Sub

        Private Overloads Async Sub ShowAsync(dataView As DataView)
            Await ShowAsync(dataView, AddressOf LoadDataAsync)
            SelectCurrent()
        End Sub

        Private Sub SelectCurrent()
            [Select](CurrentAddressID)
        End Sub

        Private Overloads Sub [Select](currentAddressID As Integer?)
            If Not currentAddressID.HasValue Then
                Return
            End If

            Dim current = GetRow(currentAddressID.Value)
            If current IsNot Nothing Then
                View.UpdateLayout()
                [Select](current, SelectionMode.[Single])
            End If
        End Sub

        Private Function GetRow(currentAddressID As Integer) As RowPresenter
            Return Match(New Address.PK(currentAddressID))
        End Function

        Public Overloads Async Sub RefreshAsync()
            Await RefreshAsync(AddressOf LoadDataAsync)
            SelectCurrent()
        End Sub

        Iterator Function GetCommandEntries(rowView As RowView) As IEnumerable(Of CommandEntry) Implements RowView.ICommandService.GetCommandEntries
            Dim baseService = Me.GetRegisteredService(Of RowView.ICommandService)()
            For Each entry In baseService.GetCommandEntries(rowView)
                Yield entry
            Next
            Yield Commands.SelectCurrent.Bind(New KeyGesture(System.Windows.Input.Key.Enter), New MouseGesture(MouseAction.LeftDoubleClick))
        End Function
    End Class
End Class
