
namespace DevZest.Data.SqlServer
{
    public sealed class IdentityOutput : Model
    {
        static IdentityOutput()
        {
            RegisterColumn((IdentityOutput x) => x.NewValue);
        }

        [Nullable(false)]
        public _Int32 NewValue { get; private set; }
    }
}
