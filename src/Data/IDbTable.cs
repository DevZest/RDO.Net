namespace DevZest.Data
{
    public interface IDbTable : IDbSet
    {
        string Name { get; }
        string Description { get; }
        string Identifier { get; }
        bool DesignMode { get; }
    }
}
