namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class PrimaryKeyInvalidArgument : Model<PrimaryKeyInvalidArgument.PK>
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
            return new PK(1);
        }

        public static readonly Mounter<_Int32> _ID = RegisterColumn((PrimaryKeyInvalidArgument _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
