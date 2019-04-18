namespace DevZest.Data
{
    public interface IEntity
    {
        Model Model { get; }
    }

    public interface IEntity<T> : IEntity
        where T : CandidateKey
    {
        new Model<T> Model { get; }
    }
}
