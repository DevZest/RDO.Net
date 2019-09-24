Imports System.Threading
Imports DevZest.Data
Imports DevZest.Data.Presenters
Imports DevZest.Data.Views

Partial Class CustomerLookupWindow
    Private NotInheritable Class Presenter
        Inherits DataPresenter(Of Customer)
        Implements RowView.ICommandService
        Public Sub New(dataView As DataView, currentCustomerID As Integer?)
            Me.CurrentCustomerID = currentCustomerID
            AddHandler dataView.Loaded, AddressOf OnDataViewLoaded
        End Sub

        Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
            builder.GridColumns("200", "200", "120", "190") _
                .GridRows("Auto", "20") _
                .RowView(Of RowView)(RowView.Styles.Selectable) _
                .Layout(Orientation.Vertical) _
                .WithFrozenTop(1) _
                .GridLineX(New GridPoint(0, 1), 4) _
                .GridLineY(New GridPoint(1, 1), 1) _
                .GridLineY(New GridPoint(2, 1), 1) _
                .GridLineY(New GridPoint(3, 1), 1) _
                .WithSelectionMode(SelectionMode.[Single]) _
                .AddBinding(0, 0, Entity.CompanyName.BindToColumnHeader("Company Name")) _
                .AddBinding(1, 0, Entity.ContactPerson.BindToColumnHeader("Contact Person")) _
                .AddBinding(2, 0, Entity.Phone.BindToColumnHeader("Phone")) _
                .AddBinding(3, 0, Entity.EmailAddress.BindToColumnHeader("Email Address")) _
                .AddBinding(0, 1, Entity.CompanyName.BindToTextBlock()) _
                .AddBinding(1, 1, Entity.ContactPerson.BindToTextBlock()) _
                .AddBinding(2, 1, Entity.Phone.BindToTextBlock()) _
                .AddBinding(3, 1, Entity.EmailAddress.BindToTextBlock())
        End Sub

        Private Function LoadDataAsync(ct As CancellationToken) As Task(Of DataSet(Of Customer))
            Return App.ExecuteAsync(Function(db) db.Customer.ToDataSetAsync(ct))
        End Function

        Public Property CurrentCustomerID() As Integer?

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
            [Select](CurrentCustomerID)
        End Sub

        Private Overloads Sub [Select](currentCustomerID As Integer?)
            If Not currentCustomerID.HasValue Then
                Return
            End If

            Dim current = GetRow(currentCustomerID.Value)
            If current IsNot Nothing Then
                [Select](current, SelectionMode.[Single])
            End If
        End Sub

        Private Function GetRow(currentCustomerID As Integer) As RowPresenter
            Return Match(New Customer.PK(currentCustomerID))
        End Function

        Public Overloads Async Sub RefreshAsync()
            Await RefreshAsync(AddressOf LoadDataAsync)
            SelectCurrent()
        End Sub

        Private _searchText As String
        Public Property SearchText() As String
            Get
                Return _searchText
            End Get
            Set
                If _searchText = Value Then
                    Return
                End If

                _searchText = Value
                If String.IsNullOrEmpty(Value) Then
                    Where = Nothing
                Else
                    Where = Function(dataRow) Entity.CompanyName(dataRow).Contains(Value) _
                        OrElse Entity.ContactPerson(dataRow).Contains(Value) _
                        OrElse Entity.Phone(dataRow).Contains(Value) _
                        OrElse Entity.EmailAddress(dataRow).Contains(Value)
                End If
                SelectCurrent()
            End Set
        End Property

        Iterator Function GetCommandEntries(rowView As RowView) As IEnumerable(Of CommandEntry) Implements RowView.ICommandService.GetCommandEntries
            Dim baseService = Me.GetRegisteredService(Of RowView.ICommandService)()
            For Each entry In baseService.GetCommandEntries(rowView)
                Yield entry
            Next
            Yield Commands.SelectCurrent.Bind(New KeyGesture(Key.Enter), New MouseGesture(MouseAction.LeftDoubleClick))
        End Function
    End Class
End Class
