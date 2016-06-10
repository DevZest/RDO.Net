using DevZest.Data.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DevZest.Data.Primitives
{
    public abstract class ColumnConverter<T> : ColumnConverter
        where T : Column, new()
    {
        public sealed override Type ColumnType
        {
            get { return typeof(T); }
        }

        internal sealed override Column MakeColumn()
        {
            return new T();
        }
    }

    public abstract class ColumnConverter
    {
        private static readonly HashSet<Assembly> s_initializedAssemblies = new HashSet<Assembly>();
        private static readonly ConcurrentDictionary<Type, IColumnConverterProvider> s_providersByType = new ConcurrentDictionary<Type, IColumnConverterProvider>();
        private static readonly ConcurrentDictionary<string, IColumnConverterProvider> s_providersByTypeId = new ConcurrentDictionary<string, IColumnConverterProvider>();

        internal static void EnsureInitialized(Column column)
        {
            var type = column.GetType();
            if (type.GetTypeInfo().IsGenericType)
                return;

            EnsureInitialized(column.GetType());
        }

        internal static void EnsureInitialized<T>(ColumnExpression<T> expression)
        {
            EnsureInitialized(GetTypeKey(expression));
        }

        private static Type GetTypeKey<T>(ColumnExpression<T> expression)
        {
            var result = expression.GetType();
            if (result.GetTypeInfo().IsGenericType)
                result = result.GetGenericTypeDefinition();
            return result;
        }

        private static void EnsureInitialized(Type type)
        {
            if (s_providersByType.ContainsKey(type))
                return;

            Initialize(type.GetTypeInfo().Assembly);

            if (!s_providersByType.ContainsKey(type))
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

                s_initializedAssemblies.Add(assembly);
            }
        }

        private static bool TryAddConverterProvider(Type targetType)
        {
            Debug.Assert(targetType != null && !s_providersByType.ContainsKey(targetType));
            var attribute = targetType.GetTypeInfo().GetCustomAttribute<ColumnConverterProviderAttribute>();
            if (attribute == null)
                return false;

            IColumnConverterProvider provider = attribute;
            provider.Initialize(targetType);
            s_providersByType.AddOrUpdate(targetType, provider, (t, oldValue) => provider);
            s_providersByTypeId.AddOrUpdate(provider.TypeId, provider, (t, oldValue) => provider);
            return true;
        }

        internal static ColumnConverter Get(Column column)
        {
            return s_providersByType[column.GetType()].Provide(column);
        }

        internal static ColumnConverter Get<T>(ColumnExpression<T> expression)
        {
            return s_providersByType[GetTypeKey(expression)].Provide(expression.Owner);
        }

        internal static ColumnConverter Get(string typeId, string typeArgId)
        {
            IColumnConverterProvider provider;
            var success = s_providersByTypeId.TryGetValue(typeId, out provider);
            return success ? provider.Provide(typeArgId) : null;
        }

        internal static string GetTypeId(Column column)
        {
            IColumnConverterProvider provider;
            var success = s_providersByType.TryGetValue(column.GetType(), out provider);
            return success ? provider.TypeId : null;
        }

        internal static string GetTypeId<T>()
            where T : Column, new()
        {
            return s_providersByType[typeof(T)].TypeId;
        }

        internal static IColumnConverterProvider GetConverterProvider(string typeId)
        {
            return s_providersByTypeId[typeId];
        }

        public string TypeId { get; internal set; }

        public abstract Type ColumnType { get; }

        public abstract Type DataType { get; }

        internal abstract Column MakeColumn();

        internal void WriteJson(StringBuilder stringBuilder, object obj)
        {
            stringBuilder.WriteStartObject()
                .WriteNameStringPair(ColumnJsonParser.TYPE_ID, TypeId).WriteComma()
                .WriteColumnProperties(this, obj)
                .WriteEndObject();
        }

        internal abstract void WritePropertiesJson(StringBuilder stringBuilder, object obj);

        internal abstract Column ParseJson(Model model, ColumnJsonParser parser);
    }
}
