using DevZest.Data.Utilities;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Collections.Generic;

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
                var typeArg = (typeof(Column).IsAssignableFrom(genericArgs[i].GetTypeInfo().BaseType)) ? columnConverter.ColumnType : columnConverter.DataType;
                result[i] = typeArg;
            }
            return result;
        }

        private ExpressionConverter MakeExpression(IReadOnlyList<string> argTypeIds)
        {
            var typeArgs = GetTypeArgs(argTypeIds);
            return (ExpressionConverter)Activator.CreateInstance(_converterType.MakeGenericType(typeArgs));
        }
    }
}
