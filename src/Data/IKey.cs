using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public interface IKey<T> : IModelReference
        where T : PrimaryKey
    {
        T PrimaryKey { get; }
    }
}
