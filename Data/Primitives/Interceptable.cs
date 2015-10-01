using DevZest.Data.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DevZest.Data.Primitives
{
    public abstract partial class Interceptable
    {
        protected Interceptable()
        {
            _interceptors = new InterceptorCollection(this);
        }

        private InterceptorCollection _interceptors;

        internal ReadOnlyCollection<T> GetInterceptors<T>()
            where T : class, IInterceptor
        {
            return _interceptors.Filter<T>();
        }

        internal T GetInterceptor<T>()
            where T : class, IInterceptor
        {
            return GetInterceptors<T>().SingleOrDefault();
        }

        internal IInterceptor GetInterceptor(string interceptorName)
        {
            if (!ContainsInterceptor(interceptorName))
                return null;
            return _interceptors[interceptorName];
        }

        internal void AddInterceptor(IInterceptor interceptor)
        {
            Check.NotNull(interceptor, nameof(interceptor));

            _interceptors.Add(interceptor);
        }

        internal void AddOrUpdateInterceptor(IInterceptor interceptor)
        {
            Check.NotNull(interceptor, nameof(interceptor));

            var fullName = interceptor.FullName;
            if (_interceptors.Contains(fullName))
                _interceptors.Remove(fullName);
            _interceptors.Add(interceptor);
        }

        internal bool ContainsInterceptor(string interceptorName)
        {
            Check.NotEmpty(interceptorName, nameof(interceptorName));

            return _interceptors.Contains(interceptorName);
        }

        internal void RemoveInterceptor(string interceptorName)
        {
            Check.NotEmpty(interceptorName, nameof(interceptorName));

            _interceptors.Remove(interceptorName);
        }

        internal void BrutalRemoveInterceptor(string interceptorName)
        {
            _interceptors.AllowFrozenChange(true);
            RemoveInterceptor(interceptorName);
            _interceptors.AllowFrozenChange(false);
        }
    }
}
