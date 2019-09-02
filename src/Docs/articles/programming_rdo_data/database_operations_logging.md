# Database Operations Logging

You can log the following database operations for troubleshooting purpose:

* Connection (opening/opened, closing/closed)
* Command (executing/executed)
* Transaction (beginning/began, committing/committed, rolling-back/rolled-back)

You can set logger to your `Db` class via <xref:DevZest.Data.Primitives.DbSession.SetLogger*> API. The following code sets logging to a `StringBuilder`:

```cs
Db CreateDb(StringBuilder logger, LogCategory logCategory = LogCategory.CommandText)
{
    return new Db(App.GetConnectionString(), db =>
    {
        db.SetLogger(s => logger.Append(s), logCategory);
    });
}
```

You can customize logging message by:

* Derive your own logger class from [Db.Logger](xref:DevZest.Data.Primitives.DbSession`3.Logger), and override `Write` or `Write*` methods;
* Return your own logger by override [Db.CreateLogger](xref:DevZest.Data.Primitives.DbSession`3.CreateLogger*).
