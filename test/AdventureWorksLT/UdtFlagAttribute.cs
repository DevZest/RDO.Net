
using System;
using DevZest.Data;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class UdtFlagAttribute : UdtAttribute
    {
        public override Type DataType
        {
            get { return typeof(bool?); }
        }

        protected override void Initialize(Column column)
        {
            column.Nullable(false);
        }
    }
}
