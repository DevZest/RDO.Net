
using System;
using DevZest.Data;

namespace AdventureWorksLT
{
    public sealed class UdtNameStyleAttribute : UdtAttribute
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
