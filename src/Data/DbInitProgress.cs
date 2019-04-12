namespace DevZest.Data
{
    public struct DbInitProgress
    {
        public DbInitProgress(IDbTable dbTable, int index, int count)
        {
            DbTable = dbTable;
            Index = index;
            Count = count;
        }

        public IDbTable DbTable { get; }
        public int Index { get; }
        public int Count { get; }
    }
}
