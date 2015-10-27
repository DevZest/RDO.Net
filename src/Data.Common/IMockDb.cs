
using System;

namespace DevZest.Data
{
    internal interface IMockDb
    {
        DbTable<T> GetMockTable<T>(string tableName, Action<T> initializer)
            where T : Model, new();
    }
}
