# [Database Agnostic](#tab/DbAgnostic)

Data type mappings and local storage:

| Data Column | CLR Type |
|-------------|----------|
| <xref:DevZest.Data._Binary> | <xref:DevZest.Data.Binary> |
| <xref:DevZest.Data._Boolean> | System.Boolean? |
| <xref:DevZest.Data._Byte> | System.Byte? |
| <xref:DevZest.Data._ByteEnum`1> | System.Enum? as Byte? |
| <xref:DevZest.Data._Char> | System.Char? |
| <xref:DevZest.Data._CharEnum`1> | System.Enum? as Char? |
| <xref:DevZest.Data._DataSet`1> | <xref:DevZest.Data.DataSet`1> |
| <xref:DevZest.Data._DateTime> | System.DateTime? |
| <xref:DevZest.Data._Decimal> | System.Decimal? |
| <xref:DevZest.Data._Double> | System.Double? |
| <xref:DevZest.Data._Guid> | System.Guid? |
| <xref:DevZest.Data._Int16> | System.Int16? |
| <xref:DevZest.Data._Int16Enum`1> | System.Enum? as Int16? |
| <xref:DevZest.Data._Int32> | System.Int32? |
| <xref:DevZest.Data._Int32Enum`1> | System.Enum? as Int32? |
| <xref:DevZest.Data._Int64> | System.Int64? |
| <xref:DevZest.Data._Int64Enum`1> | System.Enum? as Int64 |
| <xref:DevZest.Data._Single> | System.Single? |
| <xref:DevZest.Data._String> | System.String? |
| <xref:DevZest.Data.LocalColumn`1> | Arbitrary CLR types for local use |

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

| Data Column | CLR Type |
|-------------|----------|
| <xref:DevZest.Data.SqlServer._DateTimeOffset> | System.DateTimeOffset? |
| <xref:DevZest.Data.SqlServer._SqlXml> | System.Data.SqlTypes.SqlXml |
| <xref:DevZest.Data.SqlServer._TimeSpan> | System.TimeSpan? |

And:

* <xref:DevZest.Data.SqlServer.SqlFunctions>: SQL Server functions implemented as extension methods
* <xref:DevZest.Data.SqlServer.SqlSession>: SQL Server database session

# [MySql](#tab/MySql)

* <xref:DevZest.Data.MySql.MySqlSession>: MySQL database session

***
