# Welcome to RDO.Net

[RDO.Net](https://rdo.devzest.com/articles/overview/about_rdo_net.html) (Relational Data Objects for .Net) is a framework to handle data in .Net platform, consists of the following libraries and tools:

![image](https://rdo.devzest.com/images/RdoNetOverview.jpg)

## License

RDO.Net is licensed separately for its runtime (blue) and design-time tools (purple), much like .Net Core and Visual Studio.

* [RDO.Net Runtime License](https://rdo.devzest.com/articles/additional_info/rdo_net_runtime_license.html): Open source in this repo under MIT license.
* [RDO.Tools License](https://rdo.devzest.com/articles/additional_info/rdo_tools_license.html)/[Pricing](https://my.devzest.com/Pricing): Free as long as you're using Visual Studio Community Edition.

## Why RDO.Net

Enterprise application, typically backed by a relational database, has decades of history. Today's enterprise applications are unnecessarily complex and heavyweight, due to the following technical constraints:

* [Object-Relational Mapping (ORM, O/RM, and O/R mapping tool)](https://en.wikipedia.org/wiki/Object-relational_mapping), as the core of enterprise applications, is still [The Vietnam of Computer Science](http://blogs.tedneward.com/post/the-vietnam-of-computer-science/). Particularly, these difficulties are referred to as the [object-relational impedance mismatch](https://en.wikipedia.org/wiki/Object-relational_impedance_mismatch).
* Data access testing, still stays on principles and guidelines. No widely practical use yet. Refactoring or changing an enterprise application is time consuming and error prone.
* Existing data presentation solutions are far from ideal. Taking [MVVM](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel) for example: it can be overkill for simple UI; in bigger cases, it can be hard to design the ViewModel up front in order to get the right amount of generality. Refactoring or changing data presentation code is time consuming and error prone.

The above challenges impose great burdens for developing and further changing an enterprise application. Many frameworks are trying to solve these problems however they are all far from ideal. RDO.Net is the only solution to these problems in an integral, not an after-thought way (strongly recommended reading through):

* [Enterprise Application, the Direction](https://rdo.devzest.com/articles/overview/enterprise_application_the_direction.html)
* [Data and Business Layer, the New Way](https://rdo.devzest.com/articles/overview/data_and_business_layer_the_new_way.html)
* [Presentation Layer, the New Way](https://rdo.devzest.com/articles/overview/presentation_layer_the_new_way.html)

In the end, your application follows your business in a no-more-no-less basis - it adapts to your business, not vice versa:

* Your application is 100% strongly typed from database to GUI, all in clean C#/VB.Net code. Refactoring or changing your code is much easier than ever before.
* Your data access is best balanced for both programmability and performance. Rich set of data objects such as `Model`, `Db`, `DbTable`, `DbQuery` and `DataSet` are provided. No more [object-relational impedance mismatch](https://en.wikipedia.org/wiki/Object-relational_impedance_mismatch).
* Data access testing is a first class citizen which can be performed easily. Data is the core of your application, now you can build much more robust data access layer.
* A one-for-all, fully customizable data presenter is provided to handle presentation logic including layout, data binding and data validation, all consumed in clean C#/VB.Net code (no XAML needed). You don't need complex controls such as `ListBox`, `TreeView`, `DataGrid` any more. You data presentation code is greatly simplified because you can reuse all the presentation logic.
* And much more with a lightweight runtime - you only need to add several dlls into your application, size ranged from tens to several hundereds KBs.

## A Taste of RDO.Net

A fully featured sample application, [AdventureWorksLT](https://github.com/DevZest/AdventureWorksLT), together with others, is provided to demonstrate the use of RDO.Net:

![image](https://rdo.devzest.com/images/samples_adventureworkslt.wpfapp.jpg)

## The Model

```csharp
using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    [Computation(nameof(ComputeLineTotal))]
    [CheckConstraint(nameof(CK_SalesOrderDetail_OrderQty), typeof(UserMessages), 
     nameof(UserMessages.CK_SalesOrderDetail_OrderQty), 
     Description = "Check constraint [OrderQty] > (0)")]
    [CheckConstraint(nameof(CK_SalesOrderDetail_UnitPrice), 
     typeof(UserMessages), nameof(UserMessages.CK_SalesOrderDetail_UnitPrice), 
     Description = "Check constraint [UnitPrice] >= (0.00)")]
    [CheckConstraint(nameof(CK_SalesOrderDetail_UnitPriceDiscount), 
     typeof(UserMessages), nameof(UserMessages.CK_SalesOrderDetail_UnitPriceDiscount), 
     Description = "Check constraint [UnitPriceDiscount] >= (0.00)")]
    [DbIndex(nameof(IX_SalesOrderDetail_ProductID), Description = "Nonclustered index.")]
    public class SalesOrderDetail : BaseModel<SalesOrderDetail.PK>
    {
        [DbPrimaryKey("PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID", 
         Description = "Clustered index created by a primary key constraint.")]
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 salesOrderID, _Int32 salesOrderDetailID)
                : base(salesOrderID, salesOrderDetailID)
            {
            }
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                Register((Key _) => _.SalesOrderID, _SalesOrderID);
                Register((Key _) => _.SalesOrderDetailID, _SalesOrderDetailID);
            }

            protected override PK CreatePrimaryKey()
            {
                return new PK(SalesOrderID, SalesOrderDetailID);
            }

            public _Int32 SalesOrderID { get; private set; }

            public _Int32 SalesOrderDetailID { get; private set; }
        }

        public static readonly Mounter<_Int32> 
          _SalesOrderID = RegisterColumn((SalesOrderDetail _) => _.SalesOrderID);
        public static readonly Mounter<_Int32> 
          _SalesOrderDetailID = RegisterColumn((SalesOrderDetail _) => _.SalesOrderDetailID);
        public static readonly Mounter<_Int16> 
          _OrderQty = RegisterColumn((SalesOrderDetail _) => _.OrderQty);
        public static readonly Mounter<_Int32> 
          _ProductID = RegisterColumn((SalesOrderDetail _) => _.ProductID);
        public static readonly Mounter<_Decimal> 
          _UnitPrice = RegisterColumn((SalesOrderDetail _) => _.UnitPrice);
        public static readonly Mounter<_Decimal> 
         _UnitPriceDiscount = RegisterColumn((SalesOrderDetail _) => _.UnitPriceDiscount);
        public static readonly Mounter<_Decimal> 
         _LineTotal = RegisterColumn((SalesOrderDetail _) => _.LineTotal);

        public SalesOrderDetail()
        {
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(SalesOrderID, SalesOrderDetailID);
        }

        private SalesOrderHeader.PK _fk_salesOrderHeader;
        public SalesOrderHeader.PK FK_SalesOrderHeader
        {
            get { return _fk_salesOrderHeader ?? 
                  (_fk_salesOrderHeader = new SalesOrderHeader.PK(SalesOrderID)); }
        }

        private Product.PK _fk_product;
        public Product.PK FK_Product
        {
            get { return _fk_product ?? (_fk_product = new Product.PK(ProductID)); }
        }

        [DbColumn(Description = "Primary key. Foreign key to SalesOrderHeader.SalesOrderID.")]
        public _Int32 SalesOrderID { get; private set; }

        [Identity]
        [DbColumn(Description = "Primary key. One incremental unique number per product sold.")]
        public _Int32 SalesOrderDetailID { get; private set; }

        [Required]
        [DbColumn(Description = "Quantity ordered per product.")]
        public _Int16 OrderQty { get; private set; }

        [Required]
        [DbColumn(Description = "Product sold to customer. Foreign key to Product.ProductID.")]
        public _Int32 ProductID { get; private set; }

        [Required]
        [SqlMoney]
        [DbColumn(Description = "Selling price of a single product.")]
        public _Decimal UnitPrice { get; private set; }

        [Required]
        [SqlMoney]
        [DefaultValue(typeof(decimal), "0", Name = "DF_SalesOrderDetail_UnitPriceDiscount")]
        [DbColumn(Description = "Discount amount.")]
        public _Decimal UnitPriceDiscount { get; private set; }

        [Required]
        [SqlMoney]
        [DbColumn(Description = "Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.")]
        public _Decimal LineTotal { get; private set; }

        [_Computation]
        private void ComputeLineTotal()
        {
            LineTotal.ComputedAs((UnitPrice * 
            (_Decimal.Const(1) - UnitPriceDiscount) * OrderQty).IfNull(_Decimal.Const(0)));
        }

        [_CheckConstraint]
        private _Boolean CK_SalesOrderDetail_OrderQty
        {
            get { return OrderQty > _Decimal.Const(0); }
        }

        [_CheckConstraint]
        private _Boolean CK_SalesOrderDetail_UnitPrice
        {
            get { return UnitPrice >= _Decimal.Const(0); }
        }

        [_CheckConstraint]
        private _Boolean CK_SalesOrderDetail_UnitPriceDiscount
        {
            get { return UnitPriceDiscount >= _Decimal.Const(0); }
        }

        [_DbIndex]
        private ColumnSort[] IX_SalesOrderDetail_ProductID => new ColumnSort[] { ProductID };
    }
}
```

The code of model can be manipulated in Model Visualizer tool window in Visual Studio:

![image](https://rdo.devzest.com/images/SalesOrderDetailModelVisualizer.jpg)

## The Database

```csharp
using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;
using System;
using System.Data.SqlClient;

namespace DevZest.Samples.AdventureWorksLT
{
    public partial class Db : SqlSession
    {
        public Db(string connectionString, Action<Db> initializer = null)
            : base(CreateSqlConnection(connectionString))
        {
            initializer?.Invoke(this);
        }

        private static SqlConnection CreateSqlConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            return new SqlConnection(connectionString);
        }

        public Db(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        private DbTable<Address> _address;
        [DbTable("[SalesLT].[Address]", 
         Description = "Street address information for customers.")]
        public DbTable<Address> Address
        {
            get { return GetTable(ref _address); }
        }

        private DbTable<Customer> _customer;
        [DbTable("[SalesLT].[Customer]", Description = "Customer information.")]
        public DbTable<Customer> Customer
        {
            get { return GetTable(ref _customer); }
        }

        private DbTable<CustomerAddress> _customerAddress;
        [DbTable("[SalesLT].[CustomerAddress]", 
         Description = "Cross-reference table mapping customers to their address(es).")]
        [Relationship(nameof(FK_CustomerAddress_Customer_CustomerID), 
         Description = "Foreign key constraint referencing Customer.CustomerID.")]
        [Relationship(nameof(FK_CustomerAddress_Address_AddressID), 
         Description = "Foreign key constraint referencing Address.AddressID.")]
        public DbTable<CustomerAddress> CustomerAddress
        {
            get { return GetTable(ref _customerAddress); }
        }

        [_Relationship]
        private KeyMapping FK_CustomerAddress_Customer_CustomerID(CustomerAddress _)
        {
            return _.FK_Customer.Join(Customer._);
        }

        [_Relationship]
        private KeyMapping FK_CustomerAddress_Address_AddressID(CustomerAddress _)
        {
            return _.FK_Address.Join(Address._);
        }

        private DbTable<ProductCategory> _productCategory;
        [DbTable("[SalesLT].[ProductCategory]", 
         Description = "High-level product categorization.")]
        [Relationship(nameof
         (FK_ProductCategory_ProductCategory_ParentProductCategoryID_ProductCategoryID), 
         Description = "Foreign key constraint referencing ProductCategory.ProductCategoryID.")]
        public DbTable<ProductCategory> ProductCategory
        {
            get { return GetTable(ref _productCategory); }
        }

        [_Relationship]
        private KeyMapping 
        FK_ProductCategory_ProductCategory_ParentProductCategoryID_ProductCategoryID
        (ProductCategory _)
        {
            return _.FK_ParentProductCategory.Join(_);
        }

        private DbTable<ProductModel> _productModel;
        [DbTable("[SalesLT].[ProductModel]")]
        public DbTable<ProductModel> ProductModel
        {
            get { return GetTable(ref _productModel); }
        }

        private DbTable<ProductDescription> _productDescription;
        [DbTable("[SalesLT].[ProductDescription]", 
            Description = "Product descriptions in several languages.")]
        public DbTable<ProductDescription> ProductDescription
        {
            get { return GetTable(ref _productDescription); }
        }

        private DbTable<ProductModelProductDescription> _productModelProductDescription;
        [DbTable("[SalesLT].[ProductModelProductDescription]", 
            Description = "Cross-reference table mapping product descriptions and the language the description is written in.")]
        [Relationship(nameof(FK_ProductModelProductDescription_ProductModel_ProductModelID), 
           Description = "Foreign key constraint referencing ProductModel.ProductModelID.")]
        [Relationship(nameof
           (FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID), 
           Description = "Foreign key constraint referencing ProductDescription.ProductDescriptionID.")]
        public DbTable<ProductModelProductDescription> ProductModelProductDescription
        {
            get { return GetTable(ref _productModelProductDescription); }
        }

        [_Relationship]
        private KeyMapping FK_ProductModelProductDescription_ProductModel_ProductModelID
                (ProductModelProductDescription _)
        {
            return _.FK_ProductModel.Join(ProductModel._);
        }

        [_Relationship]
        private KeyMapping 
          FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID
          (ProductModelProductDescription _)
        {
            return _.FK_ProductDescription.Join(ProductDescription._);
        }

        private DbTable<Product> _product;
        [DbTable("[SalesLT].[Product]", Description = "Products sold or used in the manufacturing of sold products.")]
        [Relationship(nameof(FK_Product_ProductModel_ProductModelID))]
        [Relationship(nameof(FK_Product_ProductCategory_ProductCategoryID))]
        public DbTable<Product> Product
        {
            get { return GetTable(ref _product); }
        }

        [_Relationship]
        private KeyMapping FK_Product_ProductModel_ProductModelID(Product _)
        {
            return _.FK_ProductModel.Join(ProductModel._);
        }

        [_Relationship]
        private KeyMapping FK_Product_ProductCategory_ProductCategoryID(Product _)
        {
            return _.FK_ProductCategory.Join(ProductCategory._);
        }

        private DbTable<SalesOrderHeader> _salesOrderHeader;
        [DbTable("[SalesLT].[SalesOrderHeader]", 
         Description = "General sales order information.")]
        [Relationship(nameof(FK_SalesOrderHeader_Customer_CustomerID))]
        [Relationship(nameof(FK_SalesOrderHeader_Address_BillTo_AddressID))]
        [Relationship(nameof(FK_SalesOrderHeader_Address_ShipTo_AddressID))]
        public DbTable<SalesOrderHeader> SalesOrderHeader
        {
            get { return GetTable(ref _salesOrderHeader); }
        }

        [_Relationship]
        private KeyMapping FK_SalesOrderHeader_Customer_CustomerID(SalesOrderHeader _)
        {
            return _.FK_Customer.Join(Customer._);
        }

        [_Relationship]
        private KeyMapping FK_SalesOrderHeader_Address_BillTo_AddressID(SalesOrderHeader _)
        {
            return _.FK_BillToCustomerAddress.Join(CustomerAddress._);
        }

        [_Relationship]
        private KeyMapping FK_SalesOrderHeader_Address_ShipTo_AddressID(SalesOrderHeader _)
        {
            return _.FK_ShipToCustomerAddress.Join(CustomerAddress._);
        }

        private DbTable<SalesOrderDetail> _salesOrderDetail;
        [DbTable("[SalesLT].[SalesOrderDetail]", 
         Description = "Individual products associated with a specific sales order. See SalesOrderHeader.")]
        [Relationship(nameof(FK_SalesOrderDetail_SalesOrderHeader))]
        [Relationship(nameof(FK_SalesOrderDetail_Product))]
        public DbTable<SalesOrderDetail> SalesOrderDetail
        {
            get { return GetTable(ref _salesOrderDetail); }
        }

        [_Relationship]
        private KeyMapping FK_SalesOrderDetail_SalesOrderHeader(SalesOrderDetail _)
        {
            return _.FK_SalesOrderHeader.Join(SalesOrderHeader._);
        }

        [_Relationship]
        private KeyMapping FK_SalesOrderDetail_Product(SalesOrderDetail _)
        {
            return _.FK_Product.Join(Product._);
        }
    }
}
```

The code of database can be manipulated via Db Visualizer tool window in Visual Studio:

![image](https://rdo.devzest.com/images/AdventureWorksLTDbVisualizer.jpg)

## Data Access (CRUD)

```csharp
private async Task EnsureConnectionOpenAsync(CancellationToken ct)
{
    if (Connection.State != ConnectionState.Open)
        await OpenConnectionAsync(ct);
}

public async Task<DataSet<SalesOrderInfo>> 
  GetSalesOrderInfoAsync(_Int32 salesOrderID, CancellationToken ct = default(CancellationToken))
{
    var result = CreateQuery((DbQueryBuilder builder, SalesOrderInfo _) =>
    {
        builder.From(SalesOrderHeader, out var o)
            .LeftJoin(Customer, o.FK_Customer, out var c)
            .LeftJoin(Address, o.FK_ShipToAddress, out var shipTo)
            .LeftJoin(Address, o.FK_BillToAddress, out var billTo)
            .AutoSelect()
            .AutoSelect(c, _.Customer)
            .AutoSelect(shipTo, _.ShipToAddress)
            .AutoSelect(billTo, _.BillToAddress)
            .Where(o.SalesOrderID == salesOrderID);
    });

    await result.CreateChildAsync(_ => _.SalesOrderDetails, 
                 (DbQueryBuilder builder, SalesOrderInfoDetail _) =>
    {
        builder.From(SalesOrderDetail, out var d)
            .LeftJoin(Product, d.FK_Product, out var p)
            .AutoSelect()
            .AutoSelect(p, _.Product)
            .OrderBy(d.SalesOrderDetailID);
    }, ct);

    return await result.ToDataSetAsync(ct);
}

public async Task<int?> CreateSalesOrderAsync(DataSet<SalesOrderInfo> salesOrders, 
                        CancellationToken ct)
{
    await EnsureConnectionOpenAsync(ct);
    using (var transaction = BeginTransaction())
    {
        salesOrders._.ResetRowIdentifiers();
        await SalesOrderHeader.InsertAsync(salesOrders, true, ct);
        var salesOrderDetails = salesOrders.GetChild(_ => _.SalesOrderDetails);
        salesOrderDetails._.ResetRowIdentifiers();
        await SalesOrderDetail.InsertAsync(salesOrderDetails, ct);

        await transaction.CommitAsync(ct);
        return salesOrders.Count > 0 ? salesOrders._.SalesOrderID[0] : null;
    }
}

public async Task UpdateSalesOrderAsync
       (DataSet<SalesOrderInfo> salesOrders, CancellationToken ct)
{
    await EnsureConnectionOpenAsync(ct);
    using (var transaction = BeginTransaction())
    {
        salesOrders._.ResetRowIdentifiers();
        await SalesOrderHeader.UpdateAsync(salesOrders, ct);
        await SalesOrderDetail.DeleteAsync
              (salesOrders, (s, _) => s.Match(_.FK_SalesOrderHeader), ct);
        var salesOrderDetails = salesOrders.GetChild(_ => _.SalesOrderDetails);
        salesOrderDetails._.ResetRowIdentifiers();
        await SalesOrderDetail.InsertAsync(salesOrderDetails, ct);

        await transaction.CommitAsync(ct);
    }
}

public Task<int> DeleteSalesOrderAsync(DataSet<SalesOrderHeader.Key> dataSet, 
                 CancellationToken ct)
{
    return SalesOrderHeader.DeleteAsync(dataSet, (s, _) => s.Match(_), ct);
}
```

## Data Presentation

```csharp
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using DevZest.Data;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Samples.AdventureWorksLT
{
    partial class SalesOrderWindow
    {
        private class DetailPresenter : DataPresenter<SalesOrderInfoDetail>, 
                      ForeignKeyBox.ILookupService, DataView.IPasteAppendService
        {
            public DetailPresenter(Window ownerWindow)
            {
                _ownerWindow = ownerWindow;
            }

            private readonly Window _ownerWindow;

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var product = _.Product;
                builder.GridRows("Auto", "20")
                    .GridColumns("20", "*", "*", "Auto", "Auto", "Auto", "Auto")
                    .WithFrozenTop(1)
                    .GridLineX(new GridPoint(0, 2), 7)
                    .GridLineY(new GridPoint(2, 1), 1).GridLineY
                          (new GridPoint(3, 1), 1).GridLineY(new GridPoint(4, 1), 1)
                    .GridLineY(new GridPoint(5, 1), 1).GridLineY
                          (new GridPoint(6, 1), 1).GridLineY(new GridPoint(7, 1), 1)
                    .Layout(Orientation.Vertical)
                    .WithVirtualRowPlacement(VirtualRowPlacement.Tail)
                    .AllowDelete()
                    .AddBinding(0, 0, this.BindToGridHeader())
                    .AddBinding(1, 0, product.ProductNumber.BindToColumnHeader("Product No."))
                    .AddBinding(2, 0, product.Name.BindToColumnHeader("Product"))
                    .AddBinding(3, 0, _.UnitPrice.BindToColumnHeader("Unit Price"))
                    .AddBinding(4, 0, _.UnitPriceDiscount.BindToColumnHeader("Discount"))
                    .AddBinding(5, 0, _.OrderQty.BindToColumnHeader("Qty"))
                    .AddBinding(6, 0, _.LineTotal.BindToColumnHeader("Total"))
                    .AddBinding(0, 1, _.BindTo<RowHeader>())
                    .AddBinding(1, 1, _.FK_Product.BindToForeignKeyBox
                        (product, GetProductNumber).MergeIntoGridCell
                        (product.ProductNumber.BindToTextBlock()).WithSerializableColumns
                        (_.ProductID, product.ProductNumber))
                    .AddBinding(2, 1, 
                        product.Name.BindToTextBlock().AddToGridCell().WithSerializableColumns
                        (product.Name))
                    .AddBinding(3, 1, _.UnitPrice.BindToTextBox().MergeIntoGridCell())
                    .AddBinding(4, 1, _.UnitPriceDiscount.BindToTextBox
                        (new PercentageConverter()).MergeIntoGridCell
                        (_.UnitPriceDiscount.BindToTextBlock("{0:P}")))
                    .AddBinding(5, 1, _.OrderQty.BindToTextBox().MergeIntoGridCell())
                    .AddBinding(6, 1, _.LineTotal.BindToTextBlock
                        ("{0:C}").AddToGridCell().WithSerializableColumns(_.LineTotal));
            }
            ...
        }
    }
}
```

The above code will result in the following data grid UI:

![image](https://rdo.devzest.com/images/SalesOrderDetailUI.jpg)

## Mock Database for Testing

```csharp
public sealed class MockSalesOrder : DbMock<Db>
{
    public static Task<Db> CreateAsync(Db db, IProgress<DbInitProgress> progress = null, 
           CancellationToken ct = default(CancellationToken))
    {
        return new MockSalesOrder().MockAsync(db, progress, ct);
    }

    // This method is generated by a tool
    private static DataSet<SalesOrderHeader> Headers()
    {
        DataSet<SalesOrderHeader> result = DataSet<SalesOrderHeader>.Create().AddRows(4);
        SalesOrderHeader _ = result._;
        _.SuspendIdentity();
        _.SalesOrderID[0] = 1;
        _.SalesOrderID[1] = 2;
        ...
        _.ResumeIdentity();
        return result;
    }

    // This method is generated by a tool
    private static DataSet<SalesOrderDetail> Details()
    {
        DataSet<SalesOrderDetail> result = DataSet<SalesOrderDetail>.Create().AddRows(32);
        SalesOrderDetail _ = result._;
        _.SuspendIdentity();
        ...
        _.SalesOrderDetailID[0] = 1;
        _.SalesOrderDetailID[1] = 2;
        ...
        _.ResumeIdentity();
        return result;
    }

    protected override void Initialize()
    {
        // The order of mocking table does not matter, 
        // the dependencies will be sorted out automatically.
        Mock(Db.SalesOrderDetail, Details);
        Mock(Db.SalesOrderHeader, Headers);
    }
}
```

The code for testing data is generated from database:

![image](https://www.codeproject.com/KB/dotnet/5246942/SalesOrderDetailMockDb.jpg)

## Getting Started

It's highly recommended to start with our [step by step tutorial](https://rdo.devzest.com/articles/tutorial/get_started.html).
