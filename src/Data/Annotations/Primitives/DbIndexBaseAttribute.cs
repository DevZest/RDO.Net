using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>
    /// Base class for attribute to specify database index.
    /// </summary>
    public abstract class DbIndexBaseAttribute : ModelDeclarationAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbIndexBaseAttribute"/>.
        /// </summary>
        /// <param name="name">The name of the database index.</param>
        protected DbIndexBaseAttribute(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets or sets the name in database.
        /// </summary>
        public string DbName { get; set; }

        private Func<Model, ColumnSort[]> _sortOrderGetter;
        /// <inheritdoc />
        protected sealed override void Initialize()
        {
            var getMethod = GetPropertyGetter(typeof(ColumnSort[]));
            _sortOrderGetter = BuildSortOrderGetter(ModelType, getMethod);
        }

        /// <inheritdoc />
        protected sealed override ModelWireupEvent WireupEvent
        {
            get { return ModelWireupEvent.Initializing; }
        }

        private static Func<Model, ColumnSort[]> BuildSortOrderGetter(Type modelType, MethodInfo getMethod)
        {
            var paramModel = Expression.Parameter(typeof(Model));
            var model = Expression.Convert(paramModel, modelType);
            var call = Expression.Call(model, getMethod);
            return Expression.Lambda<Func<Model, ColumnSort[]>>(call, paramModel).Compile();
        }

        /// <inheritdoc />
        protected sealed override void Wireup(Model model)
        {
            var sortOrder = _sortOrderGetter(model);
            Wireup(model, DbName ?? Name, sortOrder);
        }

        /// <summary>
        /// Wireup this attribute with the model.
        /// </summary>
        /// <param name="model">The model object.</param>
        /// <param name="dbName">The name in the database.</param>
        /// <param name="sortOrder">The columns of the database index.</param>
        protected abstract void Wireup(Model model, string dbName, ColumnSort[] sortOrder);

        /// <summary>
        /// Gets a value indicates whether this is a clustered database index.
        /// </summary>
        public bool IsCluster { get; set; }
    }
}
