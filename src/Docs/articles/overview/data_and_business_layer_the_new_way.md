---
uid: data_and_business_layer_the_new_way
---

# Data and Business Layer, the New Way

Every enterprise application is backed by a persistent data store, typically a relational database. Object-oriented programming (OOP), on the other hand, is the mainstream for enterprise application development. According to [Martin Fowler's post](https://www.martinfowler.com/articles/dblogic.html), currently there are 3 patterns to develop business logic:

* [Transaction Script](https://www.martinfowler.com/articles/dblogic.html#TransactionScript) and [Domain Model](https://www.martinfowler.com/articles/dblogic.html#DomainModel): The business logic is placed in-memory code and the database is used pretty much as a storage mechanism.
* [Logic in SQL](https://www.martinfowler.com/articles/dblogic.html#LogicInSql): Business logic is placed in SQL queries such as stored procedure.

Each pattern has its own pros and cons, basically it's a tradeoff between programmability and performance. Most people go with the in-memory code way for better programmability, which requires an [Object-Relational Mapping (ORM, O/RM, and O/R mapping tool)](https://en.wikipedia.org/wiki/Object-relational_mapping), such as Entity Framework. Great efforts have been made to reconcile these two, however it's still [The Vietnam of Computer Science](http://blogs.tedneward.com/post/the-vietnam-of-computer-science/), due to the misconceptions of SQL and OOP.

## The Misconceptions

### SQL is Obsolete

The origins of the SQL take us back to the 1970s. Since then, IT world changed, projects are much more complicated, but SQL stays - more or less - the same. It works, but it's not elegant for nowadays modern application development. Most ORM implementations, like Entity Framework, tries to encapsulate the code needed to manipulate the data, so you don't use SQL anymore. Unfortunately, this is wrongheaded and will end up with [Leaky Abstraction](https://en.wikipedia.org/wiki/Leaky_abstraction).

As coined by Joel Spolsky, the [Law of Leaky Abstractions](https://www.joelonsoftware.com/2002/11/11/the-law-of-leaky-abstractions/) states:
> All non-trivial abstractions, to some degree, are leaky.

Apparently, RDBMS and SQL, being a fundamental of your application, is far from trivial. You can't expect to abstract it away - you have to live with it. Most ORM implementations provide native SQL execution because of this.

### OOP/POCO Obsession

OOP, on the other hand, is modern and the mainstream of application development. It's so widely adopted by developers that many developers subconsciously believe OOP can solve all the problems. Moreover, many framework authors has the religion that any framework, if not support POCO, is not a good framework.

In fact, like any technology, OOP has its limitations too. The biggest one, IMO, is: **OOP is limited to local process, it's not serialization/deserialization friendly.** Each and every object is accessed via its reference (the address pointer), and the reference, together with the type metadata and compiled byte code (further reference to type descriptors, vtable, etc.), is private to local process. It's just too obvious to realize this.

By nature, any serialized data is value type, which means:

1. To serialize/deserialize an object, a converter for the reference is needed, either implicitly or explicitly. ORM can be considered as the converter between objects and relational data.

2. As the object complexity grows, the complexity of the converter grows respectively. Particularly, the type metadata and compiled byte code (the behavior of the object, or the logic), are difficult or maybe impossible for the conversion - in the end, you need virtually the whole type runtime. That's why so many applications start with [Domain Drive Design](https://martinfowler.com/tags/domain%20driven%20design.html), but end up with [Anemic Domain Model](https://martinfowler.com/bliki/AnemicDomainModel.html).

3. On the other hand, relational data model is very complex by nature, compares to other data format such as JSON. This adds another complexity to the converter. ORM, which is considered as the converter between objects and relational data, will sooner of later hit the wall.

That's the real problem of object-relational impedance mismatch, if you want to map between arbitrary objects (POCO) and relational data. Unfortunately, almost all ORM implementations are following this path, none of them can survive from this.

## The New Way

When you're using relational database, implementing your business logic using SQL/stored procedure is the shortest path, therefore can have best performance. The cons lies in the code maintainability of SQL. On the other hand, implementing your business logic as in-memory code, has many advantages in terms of code maintainability, but may have performance issue in some cases, and most importantly, it will end up with object-relational impedance mismatch as described above. How can we get the best of both?

RDO.Data is the answer to this question. You can write your business logic in both ways, as stored procedures alike or in-memory code, using C#/VB.Net, independent of your physical database. To achieve this, we're implementing relational schema and data into a comprehensive yet simple object model:

[!include[RDO.Data Overview](../_rdo_data_overview.md)]
