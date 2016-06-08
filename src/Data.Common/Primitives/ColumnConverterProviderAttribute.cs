using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]

    public abstract class ColumnConverterProviderAttribute : Attribute, IColumnConverterProvider
    {
        public string TypeId { get; set; }

        internal abstract void Initialize(Type targetType);

        internal abstract ColumnConverter Provide(string typeId, IReadOnlyList<string> typeArgs);

        internal abstract ColumnConverter Provide(Column column);

        void IColumnConverterProvider.Initialize(Type targetType)
        {
            Initialize(targetType);
        }

        ColumnConverter IColumnConverterProvider.Provide(string typeId, IReadOnlyList<string> typeArgs)
        {
            return Provide(typeId, typeArgs);
        }

        ColumnConverter IColumnConverterProvider.Provide(Column column)
        {
            return Provide(column);
        }

    }
}
