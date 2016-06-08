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
        private static readonly ConcurrentDictionary<string, IColumnConverterProvider> s_providersByName = new ConcurrentDictionary<string, IColumnConverterProvider>();

        internal static bool EnsureInitialized(Column column)
        {
            return EnsureInitialized(column.GetType());
        }

        internal static bool EnsureInitialized<T>(ColumnExpression<T> expression)
        {
            return EnsureInitialized(expression.GetType());
        }

        private static bool EnsureInitialized(Type type)
        {
            if (s_providersByType.ContainsKey(type))
                return true;

            Initialize(type.GetTypeInfo().Assembly);

            return s_providersByType.ContainsKey(type);
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
            s_providersByName.AddOrUpdate(provider.TypeId, provider, (t, oldValue) => provider);
            return true;
        }

        internal static ColumnConverter Get(Column column)
        {
            return s_providersByType[column.GetType()].Provide(column);
        }

        internal static ColumnConverter Get<T>(ColumnExpression<T> expression)
        {
            return s_providersByType[expression.GetType()].Provide(expression.Owner);
        }

        internal static ColumnConverter Get(string typeId, IReadOnlyList<string> typeArgs)
        {
            IColumnConverterProvider provider;
            var success = s_providersByName.TryGetValue(typeId, out provider);
            return success ? provider.Provide(typeId, typeArgs) : null;
        }

        internal static string GetTypeId(Column column)
        {
            IColumnConverterProvider provider;
            var success = s_providersByType.TryGetValue(column.GetType(), out provider);
            return success ? provider.TypeId : null;
        }

        public string TypeId { get; internal set; }

        public abstract Type ColumnType { get; }

        internal abstract Column MakeColumn();

        internal void WriteJson(object obj, StringBuilder stringBuilder)
        {
            stringBuilder.Append('{');
            JsonHelper.WriteObjectName(stringBuilder, ColumnJsonParser.TYPE_ID);
            JsonValue.String(TypeId).Write(stringBuilder);
            stringBuilder.Append(',');
            WriteJsonContent(obj, stringBuilder);
            stringBuilder.Append('}');
        }

        internal abstract void WriteJsonContent(object obj, StringBuilder stringBuilder);

        internal abstract Column ParseJson(Model model, ColumnJsonParser parser);
    }
}
