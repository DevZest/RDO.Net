using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class DbIndexBaseAttribute : NamedModelAttribute
    {
        protected DbIndexBaseAttribute(string name)
            : base(name)
        {
        }

        public string DbName { get; set; }

        private Func<Model, ColumnSort[]> _sortOrderGetter;
        protected sealed override void Initialize()
        {
            var getMethod = GetPropertyGetter(typeof(ColumnSort[]));
            _sortOrderGetter = BuildSortOrderGetter(ModelType, getMethod);
        }

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

        protected sealed override void Wireup(Model model)
        {
            var sortOrder = _sortOrderGetter(model);
            Wireup(model, DbName ?? Name, sortOrder);
        }

        protected abstract void Wireup(Model model, string dbName, ColumnSort[] sortOrder);

        public bool IsCluster { get; set; }
    }
}
