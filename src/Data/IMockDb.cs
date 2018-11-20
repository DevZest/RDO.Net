namespace DevZest.Data
{
    internal interface IMockDb
    {
        DbTable<T> GetMockTable<T>(string tableName)
            where T : Model, new();
    }
}
