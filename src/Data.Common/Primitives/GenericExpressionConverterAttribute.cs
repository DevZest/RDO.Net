using DevZest.Data.Utilities;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace DevZest.Data.Primitives
{
    public sealed class GenericExpressionConverterAttribute : ColumnConverterProviderAttribute
    {
        private readonly ConcurrentDictionary<string, ColumnConverter> s_convertersByTypeArg = new ConcurrentDictionary<string, ColumnConverter>();

        public GenericExpressionConverterAttribute(Type genericExpressionType)
        {
            Check.NotNull(genericExpressionType, nameof(genericExpressionType));
            if (!genericExpressionType.GetTypeInfo().IsGenericTypeDefinition || genericExpressionType.GetGenericArguments().Length != 2)
                throw new ArgumentException(Strings.GenericExpressionConverterAttribute_InvalidGenericExpressionType, nameof(genericExpressionType));

            GenericExpressionType = genericExpressionType;
        }

        public Type GenericExpressionType { get; private set; }

        public override Type DataType
        {
            get { return GenericExpressionType.GetGenericArguments()[0]; }
        }

        public override Type ColumnType
        {
            get { return GenericExpressionType.GetGenericArguments()[1]; }
        }

        internal override ColumnConverter Provide(Column column)
        {
            return Provide(column.TypeId);
        }

        internal override ColumnConverter Provide(string typeArg)
        {
            return s_convertersByTypeArg.GetOrAdd(typeArg, MakeColumnConverter);
        }

        private ColumnConverter MakeColumnConverter(string typeArg)
        {
            var typeArgConverterProvider = ColumnConverter.GetConverterProvider(typeArg);
            return MakeColumnConverter(typeArgConverterProvider.DataType, typeArgConverterProvider.ColumnType);
        }

        private ColumnConverter MakeColumnConverter(Type dataType, Type columnType)
        {
            var type = GenericExpressionType.MakeGenericType(new Type[] { dataType, columnType });
            var result = (ColumnConverter)Activator.CreateInstance(type);
            result.TypeId = TypeId;
            return result;
        }
    }
}
