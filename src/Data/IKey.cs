namespace DevZest.Data
{
    public interface IKey<T>
        where T : PrimaryKey
    {
        T PrimaryKey { get; }
    }
}
