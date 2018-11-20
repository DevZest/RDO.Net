using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    [CrossReference(typeof(_ForeignKeyAttribute))]
    public sealed class ForeignKeyAttribute : DbTablePropertyAttribute
    {
        public ForeignKeyAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public string Description { get; set; }

        public ForeignKeyAction DeleteAction { get; set; } = ForeignKeyAction.NoAction;

        public ForeignKeyAction UpdateAction { get; set; } = ForeignKeyAction.NoAction;

        private Func<DbSession, Model, KeyMapping> _keyMappingGetter;

        protected override void Initialize(PropertyInfo propertyInfo)
        {
            var dbSessionType = propertyInfo.DeclaringType;
            var modelType = GetModelType(propertyInfo);
            var methodInfo = GetMethodInfo(dbSessionType, modelType);
            _keyMappingGetter = BuildKeyMappingGetter(methodInfo, dbSessionType, modelType);
        }

        private MethodInfo GetMethodInfo(Type dbSessionType, params Type[] paramTypes)
        {
            paramTypes.VerifyNotNull(nameof(paramTypes));
            var result = dbSessionType.GetMethod(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);
            var returnType = typeof(KeyMapping);
            if (result == null || result.ReturnType != returnType)
                throw new InvalidOperationException(DiagnosticMessages.NamedModelAttribute_FailedToResolveMethod(dbSessionType, Name, string.Join(", ", (object[])paramTypes), returnType));
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

        protected override void Wireup<T>(DbTable<T> dbTable)
        {
            var model = dbTable.Model;
            var keyMapping = _keyMappingGetter(dbTable.DbSession, model);
            var fkConstraint = new DbForeignKey(Name, Description, keyMapping.SourceKey, keyMapping.TargetKey, DeleteAction, UpdateAction);
            model.AddDbTableConstraint(fkConstraint, false);
        }
    }
}
