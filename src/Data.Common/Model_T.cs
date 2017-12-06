using DevZest.Data.Annotations;
using DevZest.Data.Primitives;
using System;
using System.Reflection;

namespace DevZest.Data
{
    public abstract class Model<T> : Model
        where T : PrimaryKey
    {
        protected Model()
        {
            AddDbTableConstraint(new PrimaryKeyConstraint(this, GetDbPrimaryKeyName(), GetDbPrimaryKeyDescription(), true, () => PrimaryKey._columns), true);
        }

        public abstract new T PrimaryKey { get; }

        internal sealed override PrimaryKey GetPrimaryKeyCore()
        {
            return this.PrimaryKey;
        }

        protected virtual string GetDbPrimaryKeyName()
        {
            var dbConstraintAttribute = typeof(T).GetTypeInfo().GetCustomAttribute<DbConstraintAttribute>();
            return dbConstraintAttribute == null ? "PK_%" : dbConstraintAttribute.Name;
        }

        protected virtual string GetDbPrimaryKeyDescription()
        {
            var dbConstraintAttribute = typeof(T).GetTypeInfo().GetCustomAttribute<DbConstraintAttribute>();
            return dbConstraintAttribute == null ? null : dbConstraintAttribute.Description;
        }
    }
}
