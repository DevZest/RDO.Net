using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Utilities
{
    internal static class ColumnManager
    {
        internal static IEnumerable<ColumnAttribute> Verify<TParent, TColumn>(this Expression<Func<TParent, TColumn>> getter, string paramName)
        {
            Check.NotNull(getter, paramName);
            var memberExpr = getter.Body as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException(Strings.Property_InvalidGetter, nameof(getter));

            var propertyInfo = typeof(TParent).GetProperty(memberExpr.Member.Name, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null)
                throw new ArgumentException(Strings.Property_InvalidGetter, nameof(getter));

            return propertyInfo.GetCustomAttributes<ColumnAttribute>();
        }

        internal static Action<T> Merge<T>(this Action<T> initializer, IEnumerable<ColumnAttribute> columnAttributes)
            where T : Column, new()
        {
            return x =>
            {
                if (initializer != null)
                    initializer(x);
                InitializeColumnAttributes(x, columnAttributes);
            };
        }

        private static void InitializeColumnAttributes(Column column, IEnumerable<ColumnAttribute> columnAttributes)
        {
            if (columnAttributes == null)
                return;
            foreach (var columnAttribute in columnAttributes)
            {
                columnAttribute.Initialize(column);

                var columnValidator = columnAttribute as IColumnValidator;
                if (columnValidator != null)
                {
                    var validator = columnValidator.GetValidator(column);
                    var model = column.ParentModel;
                    Debug.Assert(model != null);
                    model.Validators.Add(validator);
                }
            }
        }
    }
}
