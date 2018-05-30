using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Primitives
{
    internal abstract class ColumnInvoker<T>
        where T : Column
    {
        private Func<T, T> _func;

        protected ColumnInvoker(MethodInfo methodInfo, bool byPassNullable = false)
        {
            methodInfo.VerifyNotNull(nameof(methodInfo));
            BuildFunc(methodInfo, typeof(T).ResolveColumnDataType(byPassNullable));
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
