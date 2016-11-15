using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DevZest.Data.Primitives
{
    public abstract class ColumnConverter
    {
        private static readonly HashSet<Assembly> s_initializedAssemblies = new HashSet<Assembly>();
        private static readonly ConcurrentDictionary<Type, ColumnConverter> s_convertersByType = new ConcurrentDictionary<Type, ColumnConverter>();
        private static readonly ConcurrentDictionary<string, ColumnConverter> s_convertersByTypeId = new ConcurrentDictionary<string, ColumnConverter>();

        internal static void EnsureInitialized(Column column)
        {
            var type = column.GetType();
            if (type.GetTypeInfo().IsGenericType)
                return;

            EnsureInitialized(column.GetType());
        }

        internal static void EnsureInitialized(Type type)
        {
            if (s_convertersByType.ContainsKey(type))
                return;

            Initialize(type.GetTypeInfo().Assembly);

            if (!s_convertersByType.ContainsKey(type))
                throw new InvalidOperationException(Strings.ColumnConverter_NotDefined(type.FullName));
        }

        private static void Initialize(Assembly assembly)
        {
            lock (s_initializedAssemblies)
            {
                if (s_initializedAssemblies.Contains(assembly))
                    return;

                foreach (var type in assembly.GetTypes())
                    TryAddConverterProvider(type);

                ExpressionConverter.Initialize(assembly);

                s_initializedAssemblies.Add(assembly);
            }
        }

        private static bool TryAddConverterProvider(Type targetType)
        {
            Debug.Assert(targetType != null && !s_convertersByType.ContainsKey(targetType));
            var attribute = targetType.GetTypeInfo().GetCustomAttribute<ColumnConverterAttribute>();
            if (attribute == null)
                return false;

            var converter = attribute.Converter;
            converter.CoerceTypeId(targetType);
            s_convertersByType.AddOrUpdate(targetType, converter, (t, oldValue) => converter);
            s_convertersByTypeId.AddOrUpdate(converter.TypeId, converter, (t, oldValue) => converter);
            return true;
        }

        internal static ColumnConverter Get(Column column)
        {
            return s_convertersByType[column.GetType()];
        }

        internal static ColumnConverter Get(string typeId)
        {
            ColumnConverter converter;
            var success = s_convertersByTypeId.TryGetValue(typeId, out converter);
            return success ? converter : null;
        }

        internal static string GetTypeId(Column column)
        {
            return GetTypeId(column.GetType());
        }

        internal static string GetTypeId(Type columnType)
        {
            Debug.Assert(columnType != null && typeof(Column).IsAssignableFrom(columnType));
            ColumnConverter converter;
            var success = s_convertersByType.TryGetValue(columnType, out converter);
            return success ? converter.TypeId : null;
        }

        internal static string GetTypeId<T>()
            where T : Column, new()
        {
            return GetTypeId(typeof(T));
        }

        public string TypeId { get; internal set; }

        private void CoerceTypeId(Type targetType)
        {
            if (string.IsNullOrEmpty(TypeId))
                TypeId = targetType.GetDefaultTypeId();
        }

        public abstract Type ColumnType { get; }

        public abstract Type DataType { get; }

        internal abstract Column MakeColumn(ColumnExpression expression);

        internal void WriteJson(JsonWriter jsonWriter, Column column)
        {
            jsonWriter.WriteStartObject().WriteNameStringPair(JsonColumn.TYPE_ID, TypeId).WriteComma();
            if (column.IsExpression)
                WriteExpressionJson(jsonWriter, column);
            else
                jsonWriter.WriteNameStringPair(JsonColumn.NAME, column.Name);
            jsonWriter.WriteEndObject();
        }

        internal abstract void WriteExpressionJson(JsonWriter jsonWriter, Column column);
    }
}
