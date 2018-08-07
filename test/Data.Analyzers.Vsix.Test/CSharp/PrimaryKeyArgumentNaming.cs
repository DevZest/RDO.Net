namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class PrimaryKeyArgumentNaming : Model<PrimaryKeyArgumentNaming.PK>
    {
        public sealed class PK : PrimaryKey
        {
            public PK(_Int32 id2)
                : base(id2)
            {
            }
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ID);
        }

        public static readonly Mounter<_Int32> _ID = RegisterColumn((PrimaryKeyArgumentNaming _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
