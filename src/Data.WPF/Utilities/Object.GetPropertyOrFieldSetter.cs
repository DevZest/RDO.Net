using System;
using System.Linq.Expressions;

namespace DevZest
{
    internal static partial class ObjectExtensions
    {
        internal static Action<T> GetPropertyOrFieldSetter<T>(this object obj, string propertyName)
        {
            var constantExpression = Expression.Constant(obj);

            ParameterExpression paramExpression = Expression.Parameter(typeof(T), propertyName);

            MemberExpression propertyGetterExpression = Expression.PropertyOrField(constantExpression, propertyName);

            Action<T> result = Expression.Lambda<Action<T>>
            (
                Expression.Assign(propertyGetterExpression, paramExpression), paramExpression
            ).Compile();

            return result;
        }
    }
}
