Imports System.Threading
Imports DevZest.Data
Imports DevZest.Data.Presenters
Imports DevZest.Data.Views

Partial Class SalesOrderWindow
    Public NotInheritable Class Styles
        Public Shared ReadOnly DataSheet As New StyleId(GetType(SalesOrderWindow))
    End Class

    Private Class Presenter
        Inherits DataPresenter(Of SalesOrderInfo)
        Implements ForeignKeyBox.ILookupService
        Public Sub New(ownerWindow As Window, addressLookupPopup As AddressLookupPopup)
            _ownerWindow = ownerWindow
            _addressLookupPopup = addressLookupPopup
        End Sub

        Private ReadOnly _ownerWindow As Window
        Private _addressLookupPopup As AddressLookupPopup
        Private _shipToAddressBinding As RowBinding(Of ForeignKeyBox)
        Private _billToAddressBinding As RowBinding(Of ForeignKeyBox)
        Private _subFormBinding As RowBinding(Of DataView)

        Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
            _subFormBinding = Entity.SalesOrderDetails.BindToDataView(Function() New DetailPresenter(_ownerWindow)).WithStyle(Styles.DataSheet)
            Dim lineCountValidation = Entity.LineCount.BindToValidationPlaceholder(_subFormBinding)
            Dim e = Entity
            builder.GridRows("Auto", "*", "Auto") _
                .GridColumns("580") _
                .AddBinding(0, 0, e.BindToSalesOrderHeaderBox(IsNew, _shipToAddressBinding, _billToAddressBinding)) _
                .AddBinding(0, 1, lineCountValidation) _
                .AddBinding(0, 1, _subFormBinding) _
                .AddBinding(0, 2, e.BindToSalesOrderFooterBox())
        End Sub

        Public ReadOnly Property CurrentRowDetailPresenter() As DetailPresenter
            Get
                Return CType(_subFormBinding(CurrentRow).DataPresenter, DetailPresenter)
            End Get
        End Property

        Function CanLookup(foreignKey As CandidateKey) As Boolean Implements ForeignKeyBox.ILookupService.CanLookup
            If foreignKey Is Entity.FK_Customer Then
                Return True
            ElseIf foreignKey Is Entity.FK_BillToAddress Then
                Return True
            ElseIf foreignKey Is Entity.FK_ShipToAddress Then
                Return True
            Else
                Return False
            End If
        End Function

        Sub BeginLookup(foreignKeyBox As ForeignKeyBox) Implements ForeignKeyBox.ILookupService.BeginLookup
            If foreignKeyBox.ForeignKey Is Entity.FK_Customer Then
                Dim dialogWindow As New CustomerLookupWindow()
                dialogWindow.Show(_ownerWindow, foreignKeyBox, CurrentRow.GetValue(Entity.CustomerID), _shipToAddressBinding(CurrentRow), _billToAddressBinding(CurrentRow))
            ElseIf foreignKeyBox.ForeignKey Is Entity.FK_ShipToAddress OrElse foreignKeyBox.ForeignKey Is Entity.FK_BillToAddress Then
                BeginLookupAddress(foreignKeyBox)
            Else
                Throw New NotSupportedException()
            End If
        End Sub

        Private Sub BeginLookupAddress(foreignKeyBox As ForeignKeyBox)
            Dim foreignKey As Address.PK = CType(foreignKeyBox.ForeignKey, Address.PK)
            If _addressLookupPopup.FK Is foreignKey Then
                _addressLookupPopup.IsOpen = False
            Else
                Dim customerID = CurrentRow.GetValue(Entity.CustomerID)
                If customerID.HasValue Then
                    Dim addressID = If(foreignKeyBox.ForeignKey Is Entity.FK_ShipToAddress, Entity.ShipToAddressID, Entity.BillToAddressID)
                    _addressLookupPopup.Show(foreignKeyBox, CurrentRow.GetValue(addressID), customerID.Value)
                End If
            End If
        End Sub

        Public ReadOnly Property SalesOrderId() As Integer
            Get
                Return Entity.SalesOrderID(0).Value
            End Get
        End Property

        Public ReadOnly Property IsNew() As Boolean
            Get
                Return SalesOrderId < 1
            End Get
        End Property

        Public Overrides Function SubmitInput(Optional focusToErrorInput As Boolean = True) As Boolean
            If Not MyBase.SubmitInput(focusToErrorInput) Then
                Return False
            End If

            Dim detailsPresenter = _subFormBinding(CurrentRow).DataPresenter
            Return detailsPresenter.SubmitInput(focusToErrorInput)
        End Function

        Public Async Function SaveToDb(ct As CancellationToken) As Task(Of System.Nullable(Of Integer))
            If IsNew Then
                Return Await App.ExecuteAsync(Function(db) db.CreateSalesOrderAsync(DataSet, ct))
            Else
                Await App.ExecuteAsync(Function(db) db.UpdateSalesOrderAsync(DataSet, ct))
                Return Nothing
            End If
        End Function
    End Class
End Class
