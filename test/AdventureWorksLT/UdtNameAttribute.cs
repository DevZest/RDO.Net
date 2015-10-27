
using System;
using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class UdtNameAttribute : UdtAttribute
    {
        public override Type DataType
        {
            get { return typeof(string); }
        }

        protected override void Initialize(Column column)
        {
            column.Nullable(true);
            ((Column<string>)column).AsNVarChar(50);
        }
    }
}
