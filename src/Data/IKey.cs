using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public interface IKey<T> : IEntity
        where T : CandidateKey
    {
        T PrimaryKey { get; }
    }
}
