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
            var columnValidatorProviders = GetColumnValidatorProviders<TParent, TColumn>(columnName);
            return Merge(columnAttributes, columnInitializers, columnValidatorProviders);
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

        private struct ColumnInitializer<T>
            where T : Column
        {
            public ColumnInitializer(ColumnInitializerAttribute attribute, Action<T> action)
            {
                Debug.Assert(attribute != null);
                Debug.Assert(action != null);

                _attribute = attribute;
                _action = action;
            }

            private readonly ColumnInitializerAttribute _attribute;
            private readonly Action<T> _action;

            public void Initialize(T column)
            {
                if (_attribute.VerifyDeclaringType(column))
                    _action(column);
            }
        }

        private static IEnumerable<ColumnInitializer<TColumn>> GetColumnInitializers<TParent, TColumn>(string columnName)
            where TColumn : Column
        {
            List<ColumnInitializer<TColumn>> result = null;

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
                    result = new List<ColumnInitializer<TColumn>>();
                result.Add(new ColumnInitializer<TColumn>(columnInitializerAttribute, columnInitializer));
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

        private struct ColumnValidatorProvider<T>
            where T : Column
        {
            public ColumnValidatorProvider(ColumnValidatorAttribute attribute, Func<T, DataRow, IColumnValidationMessages> func)
            {
                Debug.Assert(attribute != null);
                Debug.Assert(func != null);
                _attribute = attribute;
                _func = func;
            }

            private readonly ColumnValidatorAttribute _attribute;
            private readonly Func<T, DataRow, IColumnValidationMessages> _func;

            public IValidator GetValidator(T column)
            {
                return new Validator<T>(column, _attribute, _func);
            }
        }

        private static IEnumerable<ColumnValidatorProvider<TColumn>> GetColumnValidatorProviders<TParent, TColumn>(string columnName)
            where TColumn : Column
        {
            List<ColumnValidatorProvider<TColumn>> result = null;

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
                    result = new List<ColumnValidatorProvider<TColumn>>();
                result.Add(new ColumnValidatorProvider<TColumn>(columnValidatorAttribute, columnValidator));
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
            IEnumerable<ColumnInitializer<T>> columnInitializers,
            IEnumerable<ColumnValidatorProvider<T>> columnValidatorProviders)
            where T : Column, new()
        {
            return x =>
            {
                Initialize(x, columnAttributes);
                Initialize(x, columnInitializers);
                Initialize(x, columnValidatorProviders);
            };
        }

        private static void Initialize(Column column, IEnumerable<ColumnAttribute> columnAttributes)
        {
            if (columnAttributes == null)
                return;
            foreach (var columnAttribute in columnAttributes)
                columnAttribute.TryInitialize(column);
        }

        private static void Initialize<T>(T column, IEnumerable<ColumnInitializer<T>> columnInitializers)
            where T : Column
        {
            if (columnInitializers == null)
                return;
            foreach (var columnInitializer in columnInitializers)
                columnInitializer.Initialize(column);
        }

        private sealed class Validator<T> : IValidator
            where T : Column
        {
            public Validator(T column, ColumnValidatorAttribute attribute, Func<T, DataRow, IColumnValidationMessages> func)
            {
                Debug.Assert(column != null);
                Debug.Assert(attribute != null);
                Debug.Assert(func != null);
                _column = column;
                _attribute = attribute;
                _func = func;
            }

            private readonly T _column;
            private readonly ColumnValidatorAttribute _attribute;
            private Func<T, DataRow, IColumnValidationMessages> _func;

            public IColumnValidationMessages Validate(DataRow dataRow)
            {
                return _attribute.VerifyDeclaringType(_column) ? _func(_column, dataRow) : ColumnValidationMessages.Empty;
            }
        }

        private static void Initialize<T>(T column, IEnumerable<ColumnValidatorProvider<T>> columnValidatorProviders)
            where T : Column
        {
            if (columnValidatorProviders == null)
                return;

            var validators = column.ParentModel.Validators;
            foreach (var provider in columnValidatorProviders)
                validators.Add(provider.GetValidator(column));
        }
    }
}
