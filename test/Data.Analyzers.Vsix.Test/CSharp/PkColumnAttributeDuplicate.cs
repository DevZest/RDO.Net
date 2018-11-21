using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class PkColumnAttributeDuplicate : Model<PkColumnAttributeDuplicate.PK>
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

        public static readonly Mounter<_Int32> _ID = RegisterColumn((PkColumnAttributeDuplicate _) => _.ID);
        public static readonly Mounter<_String> _Name = RegisterColumn((PkColumnAttributeDuplicate _) => _.Name);

        [PkColumn]
        public _Int32 ID { get; private set; }

        [PkColumn]
        public _String Name { get; private set; }
    }
}
