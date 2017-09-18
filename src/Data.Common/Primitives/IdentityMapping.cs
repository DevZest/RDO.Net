
using System;

namespace DevZest.Data.Primitives
{
    public sealed class IdentityMapping : Model<IdentityMapping.Key>
    {
        public sealed class Key : KeyBase
        {
            public Key(_Int32 oldValue)
            {
                OldValue = oldValue;
            }

            public _Int32 OldValue { get; private set; }
        }

        static IdentityMapping()
        {
            RegisterColumn((IdentityMapping x) => x.OldValue);
            RegisterColumn((IdentityMapping x) => x.NewValue);
            RegisterColumn((IdentityMapping x) => x.OriginalSysRowId);
        }

        public IdentityMapping()
        {
            _primaryKey = new Key(OldValue);
        }

        Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get { return _primaryKey; }
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
