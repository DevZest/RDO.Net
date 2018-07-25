namespace DevZest.Data.Primitives
{
    public sealed class IdentityMapping : Model<IdentityMapping.PK>
    {
        public sealed class PK : PrimaryKey
        {
            public static IDataValues ValueOf(int oldValue)
            {
                return DataValues.Create(_Int32.Const(oldValue));
            }

            public PK(_Int32 oldValue)
                : base(oldValue)
            {
            }

            public _Int32 OldValue
            {
                get { return GetColumn<_Int32>(0); }
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
