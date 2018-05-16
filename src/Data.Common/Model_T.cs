﻿using DevZest.Data.Annotations;
using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System.Reflection;

namespace DevZest.Data
{
    public abstract class Model<T> : Model
        where T : PrimaryKey
    {
        protected Model()
        {
            AddDbTableConstraint(new DbPrimaryKey(this, GetDbPrimaryKeyName(), GetDbPrimaryKeyDescription(), true, () => PrimaryKey._columns), true);
        }

        private T _primaryKey;
        public new T PrimaryKey
        {
            get { return _primaryKey ?? (_primaryKey = CreatePrimaryKey()); }
        }

        protected abstract T CreatePrimaryKey();

        internal sealed override PrimaryKey GetPrimaryKeyCore()
        {
            return this.PrimaryKey;
        }

        protected virtual string GetDbPrimaryKeyName()
        {
            var dbConstraintAttribute = typeof(T).GetTypeInfo().GetCustomAttribute<DbPrimaryKeyAttribute>();
            return dbConstraintAttribute == null ? "PK_%" : dbConstraintAttribute.Name;
        }

        protected virtual string GetDbPrimaryKeyDescription()
        {
            var dbConstraintAttribute = typeof(T).GetTypeInfo().GetCustomAttribute<DbPrimaryKeyAttribute>();
            return dbConstraintAttribute == null ? null : dbConstraintAttribute.Description;
        }

        public KeyMapping Match(T target)
        {
            Check.NotNull(target, nameof(target));
            return new KeyMapping(PrimaryKey, target);
        }

        public KeyMapping Match(Model<T> target)
        {
            Check.NotNull(target, nameof(target));
            return new KeyMapping(PrimaryKey, target.PrimaryKey);
        }
    }
}
