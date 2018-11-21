using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class PkColumnAttributeIndexOutOfRange1 : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((PkColumnAttributeIndexOutOfRange1 _) => _.ID);

        [PkColumn]
        public _Int32 ID { get; private set; }
    }

    public class PkColumnAttributeIndexOutOfRange2 : Model<PkColumnAttributeIndexOutOfRange2.PK>
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

        public static readonly Mounter<_Int32> _ID = RegisterColumn((PkColumnAttributeIndexOutOfRange2 _) => _.ID);
        public static readonly Mounter<_String> _Name = RegisterColumn((PkColumnAttributeIndexOutOfRange2 _) => _.Name);

        [PkColumn]
        public _Int32 ID { get; private set; }

        [PkColumn(1)]
        public _String Name { get; private set; }
    }
}
