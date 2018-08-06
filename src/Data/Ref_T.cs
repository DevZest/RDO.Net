﻿using System.Threading;

namespace DevZest.Data
{
    public abstract class Ref<T> : Projection
        where T : PrimaryKey
    {
        protected abstract T CreateForeignKey();

        private T _foreignKey;
        public T ForeignKey
        {
            get { return LazyInitializer.EnsureInitialized(ref _foreignKey, () => CreateForeignKey()); }
        }
    }

}
