![image](/images/RdoDataArchitecture2.jpg)

The following data objects are provided with rich set of properties, methods and events:

* <xref:DevZest.Data.Model>/<xref:DevZest.Data.Model`1>: Defines the meta data of model, and declarative business logic such as data constraints, automatic calculated field and validation, which can be consumed for both database and local in-memory code.
* <xref:DevZest.Data.DataSet`1>: Stores hierarchical data locally and acts as domain model of your business logic. It can be conveniently exchanged with relational database in set-based operations (CRUD), or external system via JSON.
* `Db`: Defines the database session, which contains:
  * <xref:DevZest.Data.DbTable`1>: Permanent database tables for data storage;
  * Instance methods of `Db` class to implement procedural business logic, using <xref:DevZest.Data.DataSet`1> objects as input/output. The business logic can be simple CRUD operations, or complex operation such as MRP calculation:
    * You can use <xref:DevZest.Data.DbQuery`1> objects to encapsulate data as reusable view, and/or temporary <xref:DevZest.Data.DbTable`1> objects to store intermediate result, to write stored procedure alike, set-based operations (CRUD) business logic.
    * On the other hand, <xref:DevZest.Data.DataSet`1> objects, in addition to be used as input/output of your procedural business logic, can also be used to write in-memory code to implement your business logic locally.
    * Since these objects are database agnostic, you can easily port your business logic into different relational databases.
* <xref:DevZest.Data.DbMock`1>: Easily mock the database in an isolated, known state for testing.

The following is an example of business layer implementation, to deal with sales orders in `AdventureWorksLT` sample. Please note the example is just CRUD operations for simplicity, RDO.Data is capable of doing much more than it.

# [C#](#tab/cs)

[!code-csharp[DbApi](../../../samples/AdventureWorksLT.SqlServer/AdventureWorksLT/Db.Api.cs#SalesOrderCRUD)]

# [VB.Net](#tab/vb)

[!code-vb[DbApi](../../../samples.vb/AdventureWorksLT.SqlServer/AdventureWorksLT/Db.Api.SalesOrder.vb)]

***

### RDO.Data Features, Pros and Cons

#### RDO.Data Features

* Comprehensive hierarchical data support.
* Rich declarative business logic support: constraints, automatic calculated filed, validations, etc, for both server side and client side.
* Comprehensive inter-table join/lookup support.
* Reusable view via <xref:DevZest.Data.DbQuery`1> objects.
* Intermediate result store via temporary <xref:DevZest.Data.DbTable`1> objects.
* Comprehensive JSON support, better performance because no reflection required.
* Fully customizable data types and user-defined functions.
* Built-in logging for database operations.
* Extensive support for testing.
* Rich design time tools support.
* And much more...

#### Pros

* Unified programming model for all scenarios. You have full control of your data and business layer, no magic black box.
* Your data and business layer is best balanced for both programmability and performance. Rich set of data objects are provided, no more object-relational impedance mismatch.
* Data and business layer testing is a first class citizen which can be performed easily - your application can be much more robust and adaptive to change.
* Easy to use. The APIs are clean and intuitive, with rich design time tools support.
* Rich feature and lightweight. The runtime `DevZest.Data.dll` is less than 500KB in size, whereas `DevZest.Data.SqlServer` is only 108KB in size, without any 3rd party dependency.
* The rich metadata can be consumed conveniently by other layer of your application such as the presentation layer.

#### Cons

* It's new. Although APIs are designed clean and intuitive, you or your team still need some time to get familiar with the framework. Particularly, your domain model objects are split into two parts: the <xref:DevZest.Data.Model>/<xref:DevZest.Data.Model`1> objects and <xref:DevZest.Data.DataSet`1> objects. It's not complex, but you or your team may need some time to get used to it.
* To best utilize RDO.Data, your team should be comfortable with SQL, at least to an intermediate level. This is one of those situations where you have to take into account the make up of your team - people do affect architectural decisions.
* Although data objects are lightweight, there are some overhead compares to POCO objects, especially for the simplest scenarios. In terms of performance, It may get close to, but cannot beat native stored procedure.
