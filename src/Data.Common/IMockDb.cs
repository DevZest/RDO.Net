
using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    internal interface IMockDb
    {
        DbTable<T> GetMockTable<T>(string tableName, params Func<T, ForeignKeyConstraint>[] foreignKeys)
            where T : Model, new();
    }
}
