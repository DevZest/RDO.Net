Imports DevZest.Data.Presenters
Imports DevZest.Data.Views

Public Class ProductLookupWindow

    Public NotInheritable Class Commands
        Private Sub New()
        End Sub

        Public Shared ReadOnly Property SelectCurrent() As RoutedUICommand
            Get
                Return ApplicationCommands.Open
            End Get
        End Property

        Public Shared ReadOnly Property Refresh() As RoutedUICommand
            Get
                Return NavigationCommands.Refresh
            End Get
        End Property

        Public Shared ReadOnly Property Search() As RoutedUICommand
            Get
                Return SearchBox.Commands.Search
            End Get
        End Property

        Public Shared ReadOnly Property ClearSearch() As RoutedUICommand
            Get
                Return SearchBox.Commands.ClearSearch
            End Get
        End Property

        Public Shared ReadOnly Property Close() As RoutedUICommand
            Get
                Return ApplicationCommands.Close
            End Get
        End Property
    End Class

    Public Sub New()
        InitializeComponent()
    End Sub

    Private _presenter As Presenter
    Private _foreignKeyBox As ForeignKeyBox
    Private ReadOnly Property PK() As Product.PK
        Get
            Return CType(_foreignKeyBox.ForeignKey, Product.PK)
        End Get
    End Property

    Private ReadOnly Property Lookup() As Product.Lookup
        Get
            Return CType(_foreignKeyBox.Lookup, Product.Lookup)
        End Get
    End Property

    Public Overloads Sub Show(ownerWindow As Window, foreignKeyBox As ForeignKeyBox, currentProductID As System.Nullable(Of Integer))
        Debug.Assert(ownerWindow IsNot Nothing)
        Debug.Assert(foreignKeyBox IsNot Nothing)

        Owner = ownerWindow
        _foreignKeyBox = foreignKeyBox
        _presenter = New Presenter(_dataView, currentProductID)
        InitializeCommandBindings()
        ShowDialog()
    End Sub

    Private Sub InitializeCommandBindings()
        CommandBindings.Add(New CommandBinding(Commands.SelectCurrent, AddressOf SelectCurrent, AddressOf CanSelectCurrent))
        CommandBindings.Add(New CommandBinding(Commands.Refresh, AddressOf Refresh, AddressOf CanRefresh))
        CommandBindings.Add(New CommandBinding(Commands.Search, AddressOf Search, AddressOf CanRefresh))
        CommandBindings.Add(New CommandBinding(Commands.ClearSearch, AddressOf ClearSearch, AddressOf CanRefresh))
        CommandBindings.Add(New CommandBinding(ApplicationCommands.Close, AddressOf Close))
    End Sub

    Private ReadOnly Property CurrentRow() As RowPresenter
        Get
            Return _presenter.CurrentRow
        End Get
    End Property

    Private ReadOnly Property Product() As Product
        Get
            Return _presenter.Entity
        End Get
    End Property

    Private Sub SelectCurrent(sender As Object, e As ExecutedRoutedEventArgs)
        If Not Nullable.Equals(_presenter.CurrentProductID, CurrentRow.GetValue(Product.ProductID)) Then
            _foreignKeyBox.EndLookup(CurrentRow.MakeValueBag(PK, Lookup))
        End If
        Close()
    End Sub

    Private Sub CanSelectCurrent(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = _presenter.CurrentRow IsNot Nothing
    End Sub

    Private Sub Refresh(sender As Object, e As ExecutedRoutedEventArgs)
        _presenter.RefreshAsync()
    End Sub

    Private Sub CanRefresh(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = _presenter.DataSet IsNot Nothing
    End Sub

    Private Sub Search(sender As Object, e As ExecutedRoutedEventArgs)
        _presenter.SearchText = _searchBox.SearchText
    End Sub

    Private Sub ClearSearch(sender As Object, e As ExecutedRoutedEventArgs)
        _presenter.SearchText = Nothing
    End Sub

    Private Overloads Sub Close(sender As Object, e As ExecutedRoutedEventArgs)
        Me.Close()
    End Sub

End Class
