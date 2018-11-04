using DevZest.Data.Annotations;

namespace DevZest.Data.SqlServer
{
    internal interface IIdentityOutput
    {
        Column NewValue { get; }
    }

    internal abstract class IdentityOutput : Model, IIdentityOutput
    {
        protected override string DbAlias => nameof(IdentityOutput);

        protected abstract Column GetNewValueColumn();

        Column IIdentityOutput.NewValue => GetNewValueColumn();
    }

    internal sealed class Int16IdentityOutput : IdentityOutput
    {
        static Int16IdentityOutput()
        {
            RegisterColumn((Int16IdentityOutput x) => x.NewValue);
        }

        [Required]
        public _Int16 NewValue { get; private set; }

        protected override Column GetNewValueColumn()
        {
            return NewValue;
        }
    }


    internal sealed class Int32IdentityOutput : IdentityOutput
    {
        static Int32IdentityOutput()
        {
            RegisterColumn((Int32IdentityOutput x) => x.NewValue);
        }

        [Required]
        public _Int32 NewValue { get; private set; }

        protected override Column GetNewValueColumn()
        {
            return NewValue;
        }
    }

    internal sealed class Int64IdentityOutput : IdentityOutput
    {
        static Int64IdentityOutput()
        {
            RegisterColumn((Int64IdentityOutput x) => x.NewValue);
        }

        [Required]
        public _Int64 NewValue { get; private set; }

        protected override Column GetNewValueColumn()
        {
            return NewValue;
        }
    }
}
