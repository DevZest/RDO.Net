
namespace DevZest.Data.Primitives
{
    public abstract class Default : IExtension
    {
        public abstract DbExpression DbExpression { get; }

        object IExtension.Key
        {
            get { return typeof(Default); }
        }
    }
}
