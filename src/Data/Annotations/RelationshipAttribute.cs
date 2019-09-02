using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the relationship declaration from model level.
    /// </summary>
    [CrossReference(typeof(_RelationshipAttribute))]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class RelationshipAttribute : DbTablePropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RelationshipAttribute"/>.
        /// </summary>
        /// <param name="name">The name of the relationship.</param>
        public RelationshipAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of the relationship.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the relationship.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets a value indicates how to enforce this relationship when foreign key is deleted.
        /// </summary>
        public ForeignKeyRule DeleteRule { get; set; } = ForeignKeyRule.None;

        /// <summary>
        /// Gets a value indicates how to enforce this relationship when foreign key is updated.
        /// </summary>
        public ForeignKeyRule UpdateRule { get; set; } = ForeignKeyRule.None;

        private Func<DbSession, Model, KeyMapping> _keyMappingGetter;

        /// <inheritdoc />
        protected override void Initialize(PropertyInfo propertyInfo)
        {
            var dbSessionType = propertyInfo.DeclaringType;
            var modelType = GetEntityType(propertyInfo);
            var methodInfo = GetMethodInfo(dbSessionType, modelType);
            _keyMappingGetter = BuildKeyMappingGetter(methodInfo, dbSessionType, modelType);
        }

        private MethodInfo GetMethodInfo(Type dbSessionType, params Type[] paramTypes)
        {
            paramTypes.VerifyNotNull(nameof(paramTypes));
            var result = dbSessionType.GetMethod(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);
            var returnType = typeof(KeyMapping);
            if (result == null || result.ReturnType != returnType)
                throw new InvalidOperationException(DiagnosticMessages.Common_FailedToResolveInstanceMethod(dbSessionType, Name, string.Join(", ", (object[])paramTypes), returnType));
            return result;
        }

        private Func<DbSession, Model, KeyMapping> BuildKeyMappingGetter(MethodInfo methodInfo, Type dbSessionType, Type modelType)
        {
            var paramModel = Expression.Parameter(typeof(Model));
            var model = Expression.Convert(paramModel, modelType);
            var paramDbSession = Expression.Parameter(typeof(DbSession));
            var dbSession = Expression.Convert(paramDbSession, dbSessionType);
            var call = Expression.Call(dbSession, methodInfo, model);

            return Expression.Lambda<Func<DbSession, Model, KeyMapping>>(call, paramDbSession, paramModel).Compile();
        }

        /// <inheritdoc />
        protected override void Wireup<T>(DbTable<T> dbTable)
        {
            var model = dbTable.Model;
            var keyMapping = _keyMappingGetter(dbTable.DbSession, model);
            var fkConstraint = new DbForeignKeyConstraint(Name, Description, keyMapping.SourceKey, keyMapping.TargetKey, DeleteRule, UpdateRule);
            model.AddDbTableConstraint(fkConstraint, false);
        }
    }
}
