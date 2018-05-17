using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public abstract class Ref<T> : Ref
        where T : PrimaryKey
    {
        protected abstract T CreatePrimaryKey();

        private T _key;
        public T Key
        {
            get { return _key ?? (_key = CreatePrimaryKey()); }
        }

        internal sealed override Type PrimaryKeyType
        {
            get { return typeof(T); }
        }
    }
}
