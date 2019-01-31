using DevZest.Data.Annotations;

namespace DevZest.Data.SqlServer
{
    internal interface IIdentityOutput
    {
        Column NewValue { get; }
    }

    internal interface IIdentityOutput<T>
        where T : Column
    {
        void Update(T identityColumn, DataRow dataRow, SqlReader sqlReader);
    }

    internal abstract class IdentityOutput : Model, IIdentityOutput
    {
        protected override string DbAlias => nameof(IdentityOutput);

        protected abstract Column GetNewValueColumn();

        Column IIdentityOutput.NewValue => GetNewValueColumn();
    }

    internal sealed class Int16IdentityOutput : IdentityOutput, IIdentityOutput<_Int16>
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

        public void Update(_Int16 identityColumn, DataRow dataRow, SqlReader sqlReader)
        {
            identityColumn[dataRow] = NewValue[sqlReader];
        }
    }


    internal sealed class Int32IdentityOutput : IdentityOutput, IIdentityOutput<_Int32>
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

        public void Update(_Int32 identityColumn, DataRow dataRow, SqlReader sqlReader)
        {
            identityColumn[dataRow] = NewValue[sqlReader];
        }
    }

    internal sealed class Int64IdentityOutput : IdentityOutput, IIdentityOutput<_Int64>
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

        public void Update(_Int64 identityColumn, DataRow dataRow, SqlReader sqlReader)
        {
            identityColumn[dataRow] = NewValue[sqlReader];
        }
    }
}
