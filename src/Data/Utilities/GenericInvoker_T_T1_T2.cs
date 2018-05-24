using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Utilities
{
    public abstract class GenericInvoker<T, T1, T2>
    {
        private Func<T, T1, T2, T> _func;

        protected GenericInvoker(MethodInfo methodInfo, Func<Type> typeResolver)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            var resolvedType = typeResolver();
            if (resolvedType == null)
                throw new ArgumentException(DiagnosticMessages.GenericInvoker_TypeResolverReturnsNull, nameof(typeResolver));

            BuildFunc(methodInfo, resolvedType);
        }

        private void BuildFunc(MethodInfo methodInfo, Type resolvedType)
        {
            methodInfo = methodInfo.MakeGenericMethod(typeof(T), resolvedType);
            var param0 = Expression.Parameter(typeof(T), methodInfo.GetParameters()[0].Name);
            var param1 = Expression.Parameter(typeof(T1), methodInfo.GetParameters()[1].Name);
            var param2 = Expression.Parameter(typeof(T2), methodInfo.GetParameters()[2].Name);
            var call = Expression.Call(methodInfo, param0, param1, param2);
            _func = Expression.Lambda<Func<T, T1, T2, T>>(call, param0, param1, param2).Compile();
        }

        public T Invoke(T arg, T1 param1, T2 param2)
        {
            return _func(arg, param1, param2);
        }
    }
}
