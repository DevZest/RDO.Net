namespace DevZest.Data
{
    internal interface IDbTable : IDbSet
    {
        string Name { get; }
    }
}
