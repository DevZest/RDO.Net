
namespace DevZest.Data.Primitives
{
    public abstract class Default : IResource
    {
        public abstract DbExpression DbExpression { get; }

        object IResource.Key
        {
            get { return typeof(Default); }
        }
    }
}
