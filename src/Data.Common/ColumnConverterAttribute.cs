using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    public sealed class ColumnConverterAttribute : ColumnConverterProviderAttribute
    {
        public ColumnConverterAttribute(Type converterType)
        {
            Check.NotNull(converterType, nameof(converterType));
            Converter = (ColumnConverter)Activator.CreateInstance(converterType);
        }

        private static string GetDefaultTypeId(Type targetType)
        {
            var fullName = targetType.FullName;
            var nspace = targetType.Namespace;
            return string.IsNullOrEmpty(nspace) ? fullName : fullName.Substring(nspace.Length + 1);
        }

        public ColumnConverter Converter { get; private set; }

        internal override void Initialize(Type targetType)
        {
            if (string.IsNullOrEmpty(TypeId))
                TypeId = GetDefaultTypeId(targetType);
            Converter.TypeId = TypeId;
        }

        internal override ColumnConverter Provide(Column column)
        {
            return Converter;
        }

        internal override ColumnConverter Provide(string typeId, IReadOnlyList<string> typeArgs)
        {
            return Converter;
        }
    }
}
