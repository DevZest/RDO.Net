namespace DevZest.Data
{
    /// <summary>
    /// Represents an entity.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets the model of the entity.
        /// </summary>
        Model Model { get; }
    }

    /// <summary>
    /// Represents an entity with key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEntity<T> : IEntity
        where T : CandidateKey
    {
        /// <summary>
        /// Gets the model with key of the entity.
        /// </summary>
        new Model<T> Model { get; }
    }
}
