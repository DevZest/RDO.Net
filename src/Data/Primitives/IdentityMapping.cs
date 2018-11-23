using DevZest.Data.Annotations;

namespace DevZest.Data.Primitives
{
    public interface IIdentityMapping
    {
        _Int32 OriginalSysRowId { get; }
        Column OldValue { get; }
        Column NewValue { get; }
    }

    public abstract class IdentityMapping<T> : Model<T>, IIdentityMapping
        where T : CandidateKey
    {
        static IdentityMapping()
        {
            RegisterColumn((IdentityMapping<T> x) => x.OriginalSysRowId);
        }

        public _Int32 OriginalSysRowId { get; private set; }

        protected internal sealed override string DbAlias
        {
            get { return "sys_identity_mapping"; }
        }

        Column IIdentityMapping.OldValue
        {
            get { return OldValueColumn; }
        }

        protected abstract Column OldValueColumn { get;  }

        Column IIdentityMapping.NewValue
        {
            get { return NewValueColumn; }
        }

        protected abstract Column NewValueColumn { get; }
    }

    public sealed class Int16IdentityMapping : IdentityMapping<Int16IdentityMapping.PK>
    {
        public sealed class PK : CandidateKey
        {
            public PK(_Int16 oldValue)
                : base(oldValue)
            {
            }
        }

        static Int16IdentityMapping()
        {
            RegisterColumn((Int16IdentityMapping x) => x.OldValue);
            RegisterColumn((Int16IdentityMapping x) => x.NewValue);
        }

        public Int16IdentityMapping()
        {
        }

        protected override PK CreatePrimaryKey()
        {
            return new PK(OldValue);
        }

        public _Int16 OldValue { get; private set; }

        protected override Column OldValueColumn => OldValue;

        public _Int16 NewValue { get; private set; }

        protected override Column NewValueColumn => NewValue;
    }

    public sealed class Int32IdentityMapping : IdentityMapping<Int32IdentityMapping.PK>
    {
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 oldValue)
                : base(oldValue)
            {
            }
        }

        static Int32IdentityMapping()
        {
            RegisterColumn((Int32IdentityMapping x) => x.OldValue);
            RegisterColumn((Int32IdentityMapping x) => x.NewValue);
        }

        public Int32IdentityMapping()
        {
        }

        protected override PK CreatePrimaryKey()
        {
            return new PK(OldValue);
        }

        public _Int32 OldValue { get; private set; }

        protected override Column OldValueColumn => OldValue;

        public _Int32 NewValue { get; private set; }

        protected override Column NewValueColumn => NewValue;
    }

    public sealed class Int64IdentityMapping : IdentityMapping<Int64IdentityMapping.PK>
    {
        public sealed class PK : CandidateKey
        {
            public PK(_Int64 oldValue)
                : base(oldValue)
            {
            }
        }

        static Int64IdentityMapping()
        {
            RegisterColumn((Int64IdentityMapping x) => x.OldValue);
            RegisterColumn((Int64IdentityMapping x) => x.NewValue);
            RegisterColumn((Int64IdentityMapping x) => x.OriginalSysRowId);
        }

        public Int64IdentityMapping()
        {
        }

        protected override PK CreatePrimaryKey()
        {
            return new PK(OldValue);
        }

        public _Int64 OldValue { get; private set; }

        protected override Column OldValueColumn => OldValue;

        public _Int64 NewValue { get; private set; }

        protected override Column NewValueColumn => NewValue;
    }
}
