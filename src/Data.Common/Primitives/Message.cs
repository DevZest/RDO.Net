using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public abstract class Message
    {
        protected Message(string id, string description)
        {
            Check.NotEmpty(description, nameof(description));

            Id = id;
            Description = description;
        }

        public readonly string Id;

        public abstract Severity Severity { get; }

        public readonly string Description;

        public override string ToString()
        {
            return Description;
        }
    }
}
