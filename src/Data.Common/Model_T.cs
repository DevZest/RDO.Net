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
            AddDbTableConstraint(new PrimaryKeyConstraint(this, GetPrimaryKeyConstraintName(), true, () => PrimaryKey._columns), true);
        }

        public abstract new T PrimaryKey { get; }

        internal sealed override PrimaryKey GetPrimaryKeyCore()
        {
            return this.PrimaryKey;
        }

        protected virtual string GetPrimaryKeyConstraintName()
        {
            var constraintNameAttribute = typeof(T).GetTypeInfo().GetCustomAttribute<DbConstraintAttribute>();
            return constraintNameAttribute == null ? "PK_%" : constraintNameAttribute.Name;
        }
    }
}
