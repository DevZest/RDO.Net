namespace DevZest.Data.Primitives
{
    public sealed class IdentityMapping : Model<IdentityMapping.PK>
    {
        public sealed class PK : PrimaryKey
        {
            public PK(_Int32 oldValue)
                : base(oldValue)
            {
            }
        }

        static IdentityMapping()
        {
            RegisterColumn((IdentityMapping x) => x.OldValue);
            RegisterColumn((IdentityMapping x) => x.NewValue);
            RegisterColumn((IdentityMapping x) => x.OriginalSysRowId);
        }

        public IdentityMapping()
        {
        }

        protected override PK CreatePrimaryKey()
        {
            return new PK(OldValue);
        }

        public _Int32 OldValue { get; private set; }

        public _Int32 NewValue { get; private set; }

        public _Int32 OriginalSysRowId { get; private set; }

        protected internal override string DbAlias
        {
            get { return "sys_identity_mapping"; }
        }
    }
}
