using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DevZest.Data.Primitives
{
    public abstract class ExpressionConverter
    {
        private static readonly ConcurrentDictionary<Type, ExpressionConverterAttribute> s_attributesByType = new ConcurrentDictionary<Type, ExpressionConverterAttribute>();
        private static readonly ConcurrentDictionary<string, ExpressionConverterAttribute> s_attributesByTypeId = new ConcurrentDictionary<string, ExpressionConverterAttribute>();

        internal static void EnsureInitialized<T>(ColumnExpression<T> expression)
        {
            var type = GetTypeKey(expression);
            if (!s_attributesByType.ContainsKey(type))
                throw new InvalidOperationException(Strings.ExpressionConverter_NotDefined(type.FullName));
        }

        private static Type GetTypeKey<T>(ColumnExpression<T> expression)
        {
            var result = expression.GetType();
            if (result.GetTypeInfo().IsGenericType)
                result = result.GetGenericTypeDefinition();
            return result;
        }

        internal static void Initialize(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
                TryAddConverterAttribute(type);
        }

        private static bool TryAddConverterAttribute(Type expressionType)
        {
            Debug.Assert(expressionType != null && !s_attributesByType.ContainsKey(expressionType));
            var attribute = expressionType.GetTypeInfo().GetCustomAttribute<ExpressionConverterAttribute>();
            if (attribute == null)
                return false;

            attribute.Initialize(expressionType);
            s_attributesByType.AddOrUpdate(expressionType, attribute, (t, oldValue) => attribute);
            s_attributesByTypeId.AddOrUpdate(attribute.Id, attribute, (t, oldValue) => attribute);
            return true;
        }

        internal static ExpressionConverter Get<T>(ColumnExpression<T> expression)
        {
            return Get(GetTypeId(expression), GetArgTypeIds(expression));
        }

        private static IReadOnlyList<string> GetArgTypeIds<T>(ColumnExpression<T> expression)
        {
            var argTypes = expression.ArgColumnTypes;
            if (argTypes == null || argTypes.Length == 0)
                return Array<string>.Empty;

            var result = new string[argTypes.Length];
            for (int i = 0; i < argTypes.Length; i++)
                result[i] = ColumnConverter.GetTypeId(argTypes[i]);

            return result;
        }

        internal static Type GetConverterType<T>(ColumnExpression<T> expression)
        {
            ExpressionConverterAttribute attribute;
            return s_attributesByType.TryGetValue(GetTypeKey(expression), out attribute) ? attribute.ConverterType : null;
        }

        internal static string GetTypeId<T>(ColumnExpression<T> expression)
        {
            var typeKey = GetTypeKey(expression);
            ExpressionConverterAttribute attribute;
            return s_attributesByType.TryGetValue(typeKey, out attribute) ? attribute.Id : null;
        }

        internal static ExpressionConverter Get(string typeId, IReadOnlyList<string> argColumnTypeIds)
        {
            ExpressionConverterAttribute attribute;
            return s_attributesByTypeId.TryGetValue(typeId, out attribute) ? attribute.GetConverter(argColumnTypeIds) : null;
        }

        internal abstract void WriteJson(JsonWriter jsonWriter, ColumnExpression expression);

        internal abstract ColumnExpression ParseJson(Model model, ColumnJsonParser parser);
    }
}
