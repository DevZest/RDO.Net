using DevZest.Data.Annotations;
using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data
{
    public abstract class Model<T> : Model, IKey<T>
        where T : PrimaryKey
    {
        protected Model()
        {
            AddDbTableConstraint(new DbPrimaryKey(this, GetDbPrimaryKeyName(), GetDbPrimaryKeyDescription(), true, () => PrimaryKey), true);
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
            return dbConstraintAttribute?.Description;
        }

        public KeyMapping Match(T target)
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(PrimaryKey, target);
        }

        public KeyMapping Match(Model<T> target)
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(PrimaryKey, target.PrimaryKey);
        }

        public KeyMapping Match(Key<T> target)
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(PrimaryKey, target.PrimaryKey);
        }

        /// <summary>
        /// Registers a child model.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the child model is registered on.</typeparam>
        /// <typeparam name="TChildModel">The type of the child model.</typeparam>
        /// <param name="getter">The lambda expression of the child model getter.</param>
        /// <param name="relationshipGetter">Gets relationship between child model and parent model.</param>
        /// <returns>Mounter of the child model.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relationshipGetter"/> is <see langword="null"/>.</exception>
        [MounterRegistration]
        protected static Mounter<TChildModel> RegisterChildModel<TModel, TChildModel>(Expression<Func<TModel, TChildModel>> getter,
            Func<TChildModel, T> relationshipGetter, Func<TModel, TChildModel> constructor = null)
            where TModel : Model<T>
            where TChildModel : Model, new()
        {
            getter.VerifyNotNull(nameof(getter));
            relationshipGetter.VerifyNotNull(nameof(relationshipGetter));
            if (constructor == null)
                constructor = _ => new TChildModel();
            return s_childModelManager.Register(getter, a => CreateChildModel<TModel, TChildModel>(a, relationshipGetter, constructor), null);
        }

        private static TChildModel CreateChildModel<TModel, TChildModel>(Mounter<TModel, TChildModel> mounter,
            Func<TChildModel, T> relationshipGetter, Func<TModel, TChildModel> constructor)
            where TModel : Model<T>
            where TChildModel : Model, new()
        {
            var parentModel = mounter.Parent;
            TChildModel result = constructor(parentModel);
            var parentRelationship = relationshipGetter(result).Join(parentModel.PrimaryKey);
            var parentMappings = AppendColumnMappings(parentRelationship, null, result, parentModel);
            result.Construct(parentModel, mounter.DeclaringType, mounter.Name, parentRelationship, parentMappings);
            return result;
        }

        private static IReadOnlyList<ColumnMapping> AppendColumnMappings<TChildModel, TParentModel>(IReadOnlyList<ColumnMapping> parentRelationship,
            Action<ColumnMapper, TChildModel, TParentModel> parentMappingsBuilderAction, TChildModel childModel, TParentModel parentModel)
            where TChildModel : Model
            where TParentModel : Model
        {
            if (parentMappingsBuilderAction == null)
                return parentRelationship;

            var parentMappingsBuilder = new ColumnMapper(childModel, parentModel);
            var parentMappings = parentMappingsBuilder.Build(x => parentMappingsBuilderAction(x, childModel, parentModel));

            var result = new ColumnMapping[parentRelationship.Count + parentMappings.Count];
            for (int i = 0; i < parentRelationship.Count; i++)
                result[i] = parentRelationship[i];
            for (int i = 0; i < parentMappings.Count; i++)
                result[i + parentRelationship.Count] = parentMappings[i];
            return result;
        }
    }
}
