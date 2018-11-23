using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public interface IKey<T> : IModelReference
        where T : CandidateKey
    {
        T PrimaryKey { get; }
    }
}
