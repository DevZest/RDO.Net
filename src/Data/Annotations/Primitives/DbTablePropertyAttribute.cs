using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>
    /// Base class of attribute for <see cref="DbTable{T}"/> property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class DbTablePropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbTablePropertyAttribute"/>.
        /// </summary>
        protected DbTablePropertyAttribute()
        {
        }

        /// <summary>
        /// Initializes this attribute.
        /// </summary>
        /// <param name="propertyInfo">The property infor.</param>
        protected abstract void Initialize(PropertyInfo propertyInfo);

        /// <summary>
        /// Wireup this attribute with the <see cref="DbTable{T}"/> property value..
        /// </summary>
        /// <typeparam name="T">The entity type of the DbTable.</typeparam>
        /// <param name="dbTable">The <see cref="DbTable{T}"/> property value.</param>
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

        /// <summary>
        /// Gets the entity type for specified DbTable property.
        /// </summary>
        /// <param name="propertyInfo">The specified DbTable property.</param>
        /// <returns>The entity type of the DbTable property.</returns>
        protected static Type GetEntityType(PropertyInfo propertyInfo)
        {
            var tableType = propertyInfo.PropertyType;
            return tableType.GenericTypeArguments[0];
        }
    }
}
