# [Database Agnostic](#tab/DbAgnostic)

Data type mappings and local storage:

[!include[RDO.Data Columns Agnostic](_rdo_data_columns_agnostic.md)]

Entity modeling:

* <xref:DevZest.Data.Model>/<xref:DevZest.Data.Model`1>: Schema of entity (without/with primary key)
* <xref:DevZest.Data.Projection>: Subset of <xref:DevZest.Data.Model>/<xref:DevZest.Data.Model`1>
* <xref:DevZest.Data.CandidateKey>/<xref:DevZest.Data.Key`1>/<xref:DevZest.Data.Ref`1>: Schema of database key
* <xref:DevZest.Data.Annotations>: Annotations apply to <xref:DevZest.Data.Model> or its members

Data access:

* <xref:DevZest.Data.Functions>: Functions implemented as extension methods
* <xref:DevZest.Data.DataSet`1> and <xref:DevZest.Data.DataRow>: Local data access
* <xref:DevZest.Data.DbTable`1>: Database table
* <xref:DevZest.Data.DbQuery`1>: Database query

Plus:

* [DevZest.Data.DbDesign](https://www.nuget.org/packages/DevZest.Data.DbDesign/): NuGet package for database mocking and deployment.
* [DevZest.Data.Tools.vsix](https://marketplace.visualstudio.com/items?itemName=DevZest.Data.Tools) (aka RDO.Tools): Visual Studio extension design time tools.

# [SQL Server](#tab/SqlServer)

Data type mappings and storage:

[!include[RDO.Data Columns SQL Server](_rdo_data_columns_sql_server.md)]

And:

* <xref:DevZest.Data.SqlServer.SqlFunctions>: SQL Server functions implemented as extension methods
* <xref:DevZest.Data.SqlServer.SqlSession>: SQL Server database session

# [MySql](#tab/MySql)

* <xref:DevZest.Data.MySql.MySqlSession>: MySQL database session

***
