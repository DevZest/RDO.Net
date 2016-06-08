using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    internal interface IColumnConverterProvider
    {
        void Initialize(Type targetType);

        string TypeId { get; }

        ColumnConverter Provide(string typeId, IReadOnlyList<string> typeArgs);

        ColumnConverter Provide(Column column);
    }
}
