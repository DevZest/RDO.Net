namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class PkColumnAttributeMissing : Model<PkColumnAttributeMissing.PK>
    {
        public sealed class PK : PrimaryKey
        {
            public PK(_Int32 id)
                : base(id)
            {
            }
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ID);
        }

        public static readonly Mounter<_Int32> _ID = RegisterColumn((PkColumnAttributeMissing _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
