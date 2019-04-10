namespace DevZest.Data
{
    public struct DbGenerationProgress
    {
        public DbGenerationProgress(IDbTable dbTable, int index, int count)
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
