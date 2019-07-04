---
uid: orm_data_access_the_right_way
---

# ORM/Data Access, the Right Way

Every enterprise application is backed by a persistent data store, typically a relational database. Object-oriented programming (OOP), on the other hand, is the mainstream for enterprise application development. Great efforts of [Object-Relational Mapping (ORM, O/RM, and O/R mapping tool)](https://en.wikipedia.org/wiki/Object-relational_mapping) have been made to reconcile these two, however it's still [The Vietnam of Computer Science](http://blogs.tedneward.com/post/the-vietnam-of-computer-science/), due to the misconceptions of SQL and OOP.

## The Misconceptions

### SQL is Obsolete

The origins of the SQL take us back to the 1970s. Since then, IT world changed, projects are much more complicated, but SQL stays - more or less - the same. It works, but it's not elegant for nowadays modern application development. Most ORM, like Entity Framework, tries to encapsulate the code needed to manipulate the data, so you don't use SQL anymore. Unfortunately, this is wrongheaded and will end up with [Leaky Abstraction](https://en.wikipedia.org/wiki/Leaky_abstraction).

As coined by [Joel Spolsky](https://en.wikipedia.org/wiki/Joel_Spolsky), the Law of Leaky Abstractions states: [[1]]
> All non-trivial abstractions, to some degree, are leaky.

Apparently, RDBMS and SQL, being a fundamental of your application, is far from trivial. You can't expect to abstract it away - you have to live with it. ASP.Net web forms did similar thing, which tries to abstract server round trips away, now it's replaced with ASP.Net MVC/ASP.Net Core, which is back to the right track.

### OOP/POCO Obsession

OOP, on the other hand, is modern and the mainstream of application development. It's so widely adopted by developers that many developers subconsciously believe OOP can solve all the problems. Moreover, many framework authors has the religion that any framework, if not support POCO, is not a good framework.

In fact, like any technology, OOP has its limitations too. The biggest one, IMO, is: **OOP is limited to local process, it's not serialization/deserialization friendly.** Each and every object is accessed via its reference (the address pointer), and the reference, together with the type metadata and compiled byte code (further reference to type descriptors, vtable, etc.), is private to local process. It's just too obvious to realize this.

By nature, any serialized data is value type, which means:

1. To serialize/deserialize an object, a converter for the reference is needed, either implicitly or explicitly. JSON, the simplest data format for serialization/deserialization, is a great example.

2. As the object complexity grows, the complexity of the converter grows respectively. Particularly, the type metadata and compiled byte code, are extremely difficult for the conversion - in the end, you need virtually the whole type runtime. That's why so many applications start with [Domain Drive Design](https://martinfowler.com/tags/domain%20driven%20design.html), but end up with [Anemic Domain Model](https://martinfowler.com/bliki/AnemicDomainModel.html).

3. On the other hand, relational data is very complex by nature, compares to other data format such as JSON. This adds another complexity to the converter.

That's the real problem of object-relational impedance mismatch, if you want to map between arbitrary objects (POCO) and relational data. Unfortunately, almost all ORMs are following this path, none of them can survive from this.

## The Right Way

Once we make a step back, we can get a very simple solution. ORM/Data Access is a traditional problem, we can solve it in a traditional way.

### Rich Metadata - Relational Data Objects

Without the obsession of mapping between arbitrary objects and relational data, we can choose implementing relational schema and data into a comprehensive object model:

* Common Data <xref:DevZest.Data.Column>: <xref:DevZest.Data._Binary>, <xref:DevZest.Data._Boolean>, <xref:DevZest.Data._Binary>, <xref:DevZest.Data._Byte>, <xref:DevZest.Data._ByteEnum`1>, <xref:DevZest.Data._Char>, <xref:DevZest.Data._CharEnum`1>, <xref:DevZest.Data._DataSet`1>, <xref:DevZest.Data._DateTime>, <xref:DevZest.Data._Decimal>, <xref:DevZest.Data._Double>, <xref:DevZest.Data._Guid>, <xref:DevZest.Data._Int16>, <xref:DevZest.Data._Int16Enum`1>, <xref:DevZest.Data._Int32>, <xref:DevZest.Data._Int32Enum`1>, <xref:DevZest.Data._Int64>, <xref:DevZest.Data._Int64Enum`1>, <xref:DevZest.Data._Single>, <xref:DevZest.Data._String>, <xref:DevZest.Data.LocalColumn`1>
* SQL Server Data <xref:DevZest.Data.Column>: <xref:DevZest.Data.SqlServer._DateTimeOffset>, <xref:DevZest.Data.SqlServer._SqlXml>, <xref:DevZest.Data.SqlServer._TimeSpan>
* Entity Classes: <xref:DevZest.Data.Model>, <xref:DevZest.Data.Model`1>, <xref:DevZest.Data.Projection>, <xref:DevZest.Data.Key`1>, <xref:DevZest.Data.Ref`1>
* Local DataSet: <xref:DevZest.Data.DataSet`1>
* Database: <xref:DevZest.Data.Primitives.DbSession>(<xref:DevZest.Data.SqlServer.SqlSession>, <xref:DevZest.Data.MySql.MySqlSession>, etc), <xref:DevZest.Data.DbTable`1>, <xref:DevZest.Data.DbQuery`1>, <xref:DevZest.Data.DbMock`1>

Database and dataset schema is realized as concrete metadata objects, with rich set of properties, methods and events. SQL, serialization/deserialization are explicitly implemented in these objects. In the end:

* You're Writing native SQL using C#/VB.Net, 100% strongly typed - unbeatable for both code maintainability and performance!
* JSON serialization/deserialization is a first class citizen - unbeatable performance because no reflection required.
* Database testing and deployment is a first class citizen. You can easily mock part of database with testing data, generate database from clean C#/VB.Net code, or generate C#/VB.Net code from table data in database, right in Visual Studio.
* The rich metadata can be consumed conveniently by other layer of your application such as data presentation.

[1]: https://en.wikipedia.org/wiki/Leaky_abstraction#cite_note-1
