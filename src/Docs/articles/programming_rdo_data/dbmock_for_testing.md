# DbMock for Testing

Your class derived from <xref:DevZest.Data.DbMock`1> can be used for automatic testing. You should provide a `CreateAsync` static class to return a mocked `Db` instance:

# [C#](#tab/cs)

```cs
public sealed class MockSalesOrder : DbMock<Db>
{
    public static Task<Db> CreateAsync(Db db, IProgress<DbInitProgress> progress = null, CancellationToken ct = default(CancellationToken))
    {
        return new MockSalesOrder().MockAsync(db, progress, ct);
    }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
Public Class MockSalesOrder
    Inherits DbMock(Of Db)

    Public Shared Function CreateAsync(db As Db, Optional progress As IProgress(Of DbInitProgress) = Nothing, Optional ct As CancellationToken = Nothing) As Task(Of Db)
        Return New MockSalesOrder().MockAsync(db, progress, ct)
    End Function
    ...
End Class
```

***

RDO.Data will report a compile warning if your <xref:DevZest.Data.DbMock`1> derived class does not provide `CreateAsync` static method. You can fix this warning by clicking CTRL-. in Visual Studio and select `Add Factory Method..` to insert the above code automatically.

Then you can use the `Db` object returned by `CreateAsync` method in your testing code:

* Each `Db` instance will be isolated and with testing data initialized, making it at a known starting states which is perfect for testing;
* Relationship of mocked tables will be kept;
* If table is not mocked, the table property of your `Db` class will return null.
