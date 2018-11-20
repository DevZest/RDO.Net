using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class DbTablePropertyAttribute : Attribute
    {
        protected DbTablePropertyAttribute()
        {
        }

        protected abstract void Initialize(PropertyInfo propertyInfo);

        protected abstract void Wireup<T>(DbTable<T> dbTable) where T : Model, new();

        private static ConcurrentDictionary<PropertyInfo, IReadOnlyList<DbTablePropertyAttribute>> s_attributes = new ConcurrentDictionary<PropertyInfo, IReadOnlyList<DbTablePropertyAttribute>>();

        internal static void WireupAttributes<T>(DbTable<T> dbTable)
            where T : Model, new()
        {
            var propertyInfo = dbTable.DbSession.GetType().GetProperty(dbTable.Name);
            var attributes = s_attributes.GetOrAdd(propertyInfo, ResolveAttributes);
            foreach (var attribute in attributes)
                attribute.Wireup(dbTable);
        }

        private static IReadOnlyList<DbTablePropertyAttribute> ResolveAttributes(PropertyInfo propertyInfo)
        {
            var result = propertyInfo.GetCustomAttributes<DbTablePropertyAttribute>(false).ToArray();
            for (int i = 0; i < result.Length; i++)
                result[i].Initialize(propertyInfo);
            return result;
        }

        protected static Type GetModelType(PropertyInfo propertyInfo)
        {
            var tableType = propertyInfo.PropertyType;
            return tableType.GenericTypeArguments[0];
        }
    }
}
