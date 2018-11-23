namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class RecursiveModel : Model<RecursiveModel.PK>
    {
        public sealed class PK : CandidateKey
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

        public static readonly Mounter<_Int32> _ID = RegisterColumn((RecursiveModel _) => _.ID);
        public static readonly Mounter<_Int32> _ParentID = RegisterColumn((RecursiveModel _) => _.ParentID);

        public _Int32 ID { get; private set; }

        public _Int32 ParentID { get; private set; }

        public RecursiveModel Child { get; private set; }

        private PK _fk_Parent;
        public PK FK_Parent
        {
            get { return _fk_Parent ?? (_fk_Parent = new PK(ParentID)); }
        }
    }
}
