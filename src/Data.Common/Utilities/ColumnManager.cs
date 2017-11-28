using DevZest.Data.Annotations;
using DevZest.Data.Annotations.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Utilities
{
    internal static class ColumnManager
    {
        internal static Action<TColumn> Verify<TParent, TColumn>(this Expression<Func<TParent, TColumn>> getter, string paramName)
            where TColumn : Column, new()
        {
            Check.NotNull(getter, paramName);
            var memberExpr = getter.Body as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException(Strings.InvalidGetterExpression, paramName);
            var columnName = memberExpr.Member.Name;

            var columnAttributes = GetColumnAttributes<TParent>(columnName, paramName);
            var columnInitializers = GetColumnInitializers<TParent, TColumn>(columnName);
            var columnValidators = GetColumnValidators<TParent, TColumn>(columnName);
            return Merge(columnAttributes, columnInitializers, columnValidators);
        }

        private static IEnumerable<ColumnAttribute> GetColumnAttributes<T>(string columnName, string paramName)
        {
            var propertyInfo = typeof(T).GetProperty(columnName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null)
                throw new ArgumentException(Strings.InvalidGetterExpression, paramName);

            var result = propertyInfo.GetCustomAttributes<ColumnAttribute>().ToArray();
            foreach (var columnAttribute in result)
                columnAttribute.DeclaringType = propertyInfo.DeclaringType;
            return result;
        }

        private static IEnumerable<Action<TColumn>> GetColumnInitializers<TParent, TColumn>(string columnName)
        {
            List<Action<TColumn>> result = null;

            var methods = GetStaticMethods<TParent>();
            if (methods == null)
                return result;

            foreach (var method in methods)
            {
                var columnInitializerAttribute = method.GetCustomAttribute<ColumnInitializerAttribute>();
                if (columnInitializerAttribute == null || columnInitializerAttribute.ColumnName != columnName)
                    continue;
                columnInitializerAttribute.DeclaringType = typeof(TParent);
                var columnInitializer = GetColumnInitializer<TColumn>(method);
                if (result == null)
                    result = new List<Action<TColumn>>();
                result.Add(columnInitializer);
            }

            return result;
        }

        private static MethodInfo[] GetStaticMethods<T>()
        {
            return typeof(T).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static Action<T> GetColumnInitializer<T>(MethodInfo methodInfo)
        {
            var paramColumn = Expression.Parameter(typeof(T), methodInfo.GetParameters()[0].Name);
            var call = Expression.Call(methodInfo, paramColumn);
            return Expression.Lambda<Action<T>>(call, paramColumn).Compile();
        }

        private static IEnumerable<Func<TColumn, DataRow, IColumnValidationMessages>> GetColumnValidators<TParent, TColumn>(string columnName)
        {
            List<Func<TColumn, DataRow, IColumnValidationMessages>> result = null;

            var methods = GetStaticMethods<TParent>();
            if (methods == null)
                return result;

            foreach (var method in methods)
            {
                var columnValidatorAttribute = method.GetCustomAttribute<ColumnValidatorAttribute>();
                if (columnValidatorAttribute == null || columnValidatorAttribute.ColumnName != columnName)
                    continue;
                columnValidatorAttribute.DeclaringType = typeof(TParent);
                var columnValidator = GetColumnValidator<TColumn>(method);
                if (result == null)
                    result = new List<Func<TColumn, DataRow, IColumnValidationMessages>>();
                result.Add(columnValidator);
            }

            return result;
        }

        private static Func<T, DataRow, IColumnValidationMessages> GetColumnValidator<T>(MethodInfo methodInfo)
        {
            var paramColumn = Expression.Parameter(typeof(T), methodInfo.GetParameters()[0].Name);
            var paramDataRow = Expression.Parameter(typeof(DataRow), methodInfo.GetParameters()[1].Name);
            var call = Expression.Call(methodInfo, paramColumn, paramDataRow);
            return Expression.Lambda<Func<T, DataRow, IColumnValidationMessages>>(call, paramColumn, paramDataRow).Compile();
        }

        private static Action<T> Merge<T>(IEnumerable<ColumnAttribute> columnAttributes,
            IEnumerable<Action<T>> columnInitializers,
            IEnumerable<Func<T, DataRow, IColumnValidationMessages>> columnValidators)
            where T : Column, new()
        {
            return x =>
            {
                Initialize(x, columnAttributes);
                Initialize(x, columnInitializers);
                Initialize(x, columnValidators);
            };
        }

        private static void Initialize(Column column, IEnumerable<ColumnAttribute> columnAttributes)
        {
            if (columnAttributes == null)
                return;
            foreach (var columnAttribute in columnAttributes)
                columnAttribute.TryInitialize(column);
        }

        private static void Initialize<T>(T column, IEnumerable<Action<T>> columnInitializers)
        {
            if (columnInitializers == null)
                return;
            foreach (var columnInitializer in columnInitializers)
                columnInitializer(column);
        }

        private sealed class Validator<T> : IValidator
            where T : Column
        {
            public Validator(T column, Func<T, DataRow, IColumnValidationMessages> func)
            {
                Debug.Assert(column != null);
                Debug.Assert(func != null);
                _column = column;
                _func = func;
            }

            private readonly T _column;
            private Func<T, DataRow, IColumnValidationMessages> _func;

            public IColumnValidationMessages Validate(DataRow dataRow)
            {
                return _func(_column, dataRow);
            }
        }

        private static void Initialize<T>(T column, IEnumerable<Func<T, DataRow, IColumnValidationMessages>> columnValidators)
            where T : Column
        {
            if (columnValidators == null)
                return;

            var validators = column.ParentModel.Validators;
            foreach (var columnValidator in columnValidators)
                validators.Add(new Validator<T>(column, columnValidator));
        }
    }
}
