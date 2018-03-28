using System;
using System.Linq.Expressions;

namespace DevZest
{
    internal static partial class ObjectExtensions
    {
        internal static Func<T> GetPropertyOrFieldGetter<T>(this object obj, string propertyOrFieldName)
        {
            var constantExpression = Expression.Constant(obj);
            Expression propertyGetterExpression = Expression.PropertyOrField(constantExpression, propertyOrFieldName);

            Func<T> result = Expression.Lambda<Func<T>>(propertyGetterExpression).Compile();

            return result;
        }
    }
}
