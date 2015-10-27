
using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public abstract class UdtAttribute : ColumnAttribute
    {
        public abstract Type DataType { get; }
    }
}
