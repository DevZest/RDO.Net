
namespace DevZest.Data.Primitives
{
    public abstract class Default : IInterceptor
    {
        public abstract DbExpression DbExpression { get; }

        public string FullName
        {
            get { return typeof(Default).FullName; }
        }
    }
}
