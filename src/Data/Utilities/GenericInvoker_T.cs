using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Utilities
{
    public abstract class GenericInvoker<T>
    {
        private Func<T, T> _func;

        protected GenericInvoker(MethodInfo methodInfo, Func<Type> typeResolver)
        {
            methodInfo.VerifyNotNull(nameof(methodInfo));

            var resolvedType = typeResolver();
            if (resolvedType == null)
                throw new ArgumentException(DiagnosticMessages.GenericInvoker_TypeResolverReturnsNull, nameof(typeResolver));

            BuildFunc(methodInfo, resolvedType);
        }

        private void BuildFunc(MethodInfo methodInfo, Type resolvedType)
        {
            methodInfo = methodInfo.MakeGenericMethod(typeof(T), resolvedType);
            var param = Expression.Parameter(typeof(T), methodInfo.GetParameters()[0].Name);
            var call = Expression.Call(methodInfo, param);
            _func = Expression.Lambda<Func<T, T>>(call, param).Compile();
        }

        public T Invoke(T arg)
        {
            return _func(arg);
        }
    }
}
