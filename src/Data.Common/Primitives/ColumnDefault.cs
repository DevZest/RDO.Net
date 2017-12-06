
namespace DevZest.Data.Primitives
{
    public abstract class ColumnDefault : IExtension
    {
        internal ColumnDefault(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public abstract DbExpression DbExpression { get; }

        object IExtension.Key
        {
            get { return typeof(ColumnDefault); }
        }
    }
}
