Imports DevZest.Data.Views

Public Class AddressLookupPopup
    Public Sub New()
        InitializeComponent()
    End Sub

    Private _presenter As Presenter
    Private _foreignKeyBox As ForeignKeyBox
    Public ReadOnly Property FK() As Address.PK
        Get
            Return If(_foreignKeyBox Is Nothing, Nothing, CType(_foreignKeyBox.ForeignKey, Address.PK))
        End Get
    End Property
    Private ReadOnly Property Lookup() As Address.Lookup
        Get
            Return CType(_foreignKeyBox.Lookup, Address.Lookup)
        End Get
    End Property

    Public Sub Show(foreignKeyBox As ForeignKeyBox, currentAddressID As System.Nullable(Of Integer), customerID As Integer)
        If IsOpen Then
            IsOpen = False
        End If
        PlacementTarget = foreignKeyBox
        _foreignKeyBox = foreignKeyBox
        _foreignKeyBox.IsEnabled = False
        _presenter = New Presenter(_dataView, currentAddressID, customerID)
        InitializeCommandBindings()
        IsOpen = True
    End Sub

    Private Sub Popup_Closed(sender As Object, e As System.EventArgs)
        _foreignKeyBox.IsEnabled = True
        PlacementTarget = Nothing
        _foreignKeyBox = Nothing
        _presenter.DetachView()
        _presenter = Nothing
        CommandBindings.Clear()
    End Sub

    Private Sub InitializeCommandBindings()
        CommandBindings.Add(New CommandBinding(Commands.SelectCurrent, AddressOf SelectCurrent, AddressOf CanSelectCurrent))
        CommandBindings.Add(New CommandBinding(NavigationCommands.Refresh, AddressOf Refresh, AddressOf CanRefresh))
    End Sub

    Private Sub SelectCurrent(sender As Object, e As ExecutedRoutedEventArgs)
        _foreignKeyBox.EndLookup(_presenter.CurrentRow.MakeValueBag(FK, Lookup))
        IsOpen = False
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
End Class
