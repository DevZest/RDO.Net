using DevZest.Data.Utilities;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.Primitives
{
    public sealed class ExpressionConverterGenericsAttribute : ExpressionConverterAttribute
    {
        private readonly ConcurrentDictionary<string, ExpressionConverter> s_convertersBycolumnTypeIds = new ConcurrentDictionary<string, ExpressionConverter>();

        public ExpressionConverterGenericsAttribute(Type converterType)
        {
            Check.NotNull(converterType, nameof(converterType));
            if (!converterType.GetTypeInfo().IsGenericTypeDefinition)
                throw new ArgumentException("", nameof(converterType));

            _converterType = converterType;
        }

        private Type _converterType;

        public override Type ConverterType
        {
            get { return _converterType; }
        }

        internal override ExpressionConverter GetConverter(IReadOnlyList<string> argTypeIds)
        {
            var key = string.Join(", ", argTypeIds);
            return s_convertersBycolumnTypeIds.GetOrAdd(key, x => MakeExpression(argTypeIds));
        }

        private Type[] GetTypeArgs(IReadOnlyList<string> argTypeIds)
        {
            var genericArgs = _converterType.GetGenericArguments();

            var result = new Type[argTypeIds.Count];
            for (int i = 0; i < argTypeIds.Count; i++)
            {
                var columnConverter = ColumnConverter.Get(argTypeIds[i]);
                var typeArg = GetTypeArg(columnConverter, genericArgs[i]);
                result[i] = typeArg;
            }
            return result;
        }

        private Type GetTypeArg(ColumnConverter columnConverter, Type argType)
        {
            var argBaseType = argType.GetTypeInfo().BaseType;
            if (typeof(Column).IsAssignableFrom(argBaseType))
                return columnConverter.ColumnType;

            var dataType = columnConverter.DataType;
            if (argType.GetTypeInfo().GetCustomAttribute<UnderlyingValueTypeAttribute>() != null)
                return dataType.GetGenericArguments()[0];
            else
                return dataType;
        }

        private ExpressionConverter MakeExpression(IReadOnlyList<string> argTypeIds)
        {
            var typeArgs = GetTypeArgs(argTypeIds);
            return (ExpressionConverter)Activator.CreateInstance(_converterType.MakeGenericType(typeArgs));
        }
    }
}
