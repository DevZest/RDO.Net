using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public sealed class UniqueAttribute : ValidatorAttribute
    {
        protected internal override void Initialize(Column column)
        {
            throw new NotImplementedException();
        }

        protected override string FormatMessage(IColumns columns, DataRow dataRow)
        {
            throw new NotImplementedException();
        }
    }
}
