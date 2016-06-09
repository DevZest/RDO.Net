using System;

namespace DevZest.Data.Primitives
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]

    public abstract class ColumnConverterProviderAttribute : Attribute, IColumnConverterProvider
    {
        public abstract Type ColumnType { get; }
        public abstract Type DataType { get; }
        public string TypeId { get; set; }

        internal virtual void Initialize(Type targetType)
        {
            if (string.IsNullOrEmpty(TypeId))
                TypeId = GetDefaultTypeId(targetType);
        }

        internal virtual string GetDefaultTypeId(Type targetType)
        {
            var fullName = targetType.FullName;
            var nspace = targetType.Namespace;
            return string.IsNullOrEmpty(nspace) ? fullName : fullName.Substring(nspace.Length + 1);
        }

        internal abstract ColumnConverter Provide(string typeArgId);

        internal abstract ColumnConverter Provide(Column column);

        void IColumnConverterProvider.Initialize(Type targetType)
        {
            Initialize(targetType);
        }

        ColumnConverter IColumnConverterProvider.Provide(string typeArgId)
        {
            return Provide(typeArgId);
        }

        ColumnConverter IColumnConverterProvider.Provide(Column column)
        {
            return Provide(column);
        }

    }
}
