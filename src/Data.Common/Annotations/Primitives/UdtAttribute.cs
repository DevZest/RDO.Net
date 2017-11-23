using System;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class UdtAttribute : ColumnAttribute
    {
        public abstract Type DataType { get; }
    }
}
