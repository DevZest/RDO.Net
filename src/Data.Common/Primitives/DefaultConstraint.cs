
namespace DevZest.Data.Primitives
{
    public abstract class DefaultConstraint : IExtension
    {
        internal DefaultConstraint(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public abstract DbExpression DbExpression { get; }

        object IExtension.Key
        {
            get { return typeof(DefaultConstraint); }
        }
    }
}
