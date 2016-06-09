using System;

namespace DevZest.Data.Primitives
{
    internal interface IColumnConverterProvider
    {
        void Initialize(Type targetType);

        string TypeId { get; }

        Type ColumnType { get; }

        Type DataType { get; }

        ColumnConverter Provide(string typeArgId);

        ColumnConverter Provide(Column column);
    }
}
