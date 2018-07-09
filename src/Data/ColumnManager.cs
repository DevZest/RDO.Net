﻿using DevZest.Data.Annotations;
using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data
{
    internal static class ColumnManager
    {
        internal static Action<TColumn> Verify<TParent, TColumn>(this Expression<Func<TParent, TColumn>> getter, string paramName)
            where TColumn : Column, new()
        {
            getter.VerifyNotNull(paramName);
            var memberExpr = getter.Body as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException(DiagnosticMessages.InvalidGetterExpression, paramName);
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
                throw new ArgumentException(DiagnosticMessages.InvalidGetterExpression, paramName);

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
            public ColumnValidatorProvider(ColumnValidatorAttribute attribute, Func<T, DataRow, bool> func)
            {
                Debug.Assert(attribute != null);
                Debug.Assert(func != null);
                _attribute = attribute;
                _func = func;
            }

            private readonly ColumnValidatorAttribute _attribute;
            private readonly Func<T, DataRow, bool> _func;

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

        private static Func<T, DataRow, bool> GetColumnValidator<T>(MethodInfo methodInfo)
        {
            var paramColumn = Expression.Parameter(typeof(T), methodInfo.GetParameters()[0].Name);
            var paramDataRow = Expression.Parameter(typeof(DataRow), methodInfo.GetParameters()[1].Name);
            var call = Expression.Call(methodInfo, paramColumn, paramDataRow);
            return Expression.Lambda<Func<T, DataRow, bool>>(call, paramColumn, paramDataRow).Compile();
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
            public Validator(T column, ColumnValidatorAttribute attribute, Func<T, DataRow, bool> func)
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
            private Func<T, DataRow, bool> _func;

            public DataValidationError Validate(DataRow dataRow)
            {
                if (!_attribute.VerifyDeclaringType(_column))
                    return null;
                var isValid = _func(_column, dataRow);
                return isValid ? null : new DataValidationError(_attribute.MessageString, _column);
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

        internal static Action<Column<T>> Verify<TParent, T>(this Expression<Func<TParent, Column<T>>> getter, string paramName)
        {
            getter.VerifyNotNull(paramName);
            var memberExpr = getter.Body as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException(DiagnosticMessages.InvalidGetterExpression, paramName);
            var columnName = memberExpr.Member.Name;

            var columnAttributes = GetColumnAttributes<TParent>(columnName, paramName);
            var columnInitializers = GetColumnInitializers<TParent, Column<T>>(columnName);
            var columnValidatorProviders = GetColumnValidatorProviders<TParent, Column<T>>(columnName);
            return Merge(columnAttributes, columnInitializers, columnValidatorProviders);
        }

        private static Action<Column<T>> Merge<T>(IEnumerable<ColumnAttribute> columnAttributes,
            IEnumerable<ColumnInitializer<Column<T>>> columnInitializers,
            IEnumerable<ColumnValidatorProvider<Column<T>>> columnValidatorProviders)
        {
            return x =>
            {
                Initialize(x, columnAttributes);
                Initialize(x, columnInitializers);
                Initialize(x, columnValidatorProviders);
            };
        }

        internal static bool ContainsSource(this IReadOnlyList<ColumnMapping> columnMappings, Column source)
        {
            foreach (var mapping in columnMappings)
            {
                if (mapping.Source == source)
                    return true;
            }
            return false;
        }

        internal static List<T> TranslateToColumns<T>(this List<T> columns, Model model)
            where T : Column
        {
            if (columns == null)
                return null;

            List<T> result = null;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var translated = column.TranslateTo(model);
                if (result != null)
                    result.Add(translated);
                else if (translated != column)
                {
                    if (result == null)
                    {
                        result = new List<T>();
                        for (int j = 0; j < i; j++)
                            result.Add(columns[j]);
                    }
                    result.Add(translated);
                }
            }
            return result ?? columns;
        }

        internal static T[] TranslateToParams<T>(this IReadOnlyList<T> parameters, Model model)
            where T : Column
        {
            if (parameters == null)
                return null;

            T[] result = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                var column = parameters[i];
                var translated = column.TranslateTo(model);
                if (result != null)
                    result[i] = translated;
                else if (translated != column)
                {
                    if (result == null)
                    {
                        result = new T[parameters.Count];
                        for (int j = 0; j < i; j++)
                            result[j] = parameters[j];
                    }
                    result[i] = translated;
                }
            }
            return result;
        }

        internal static Type ResolveColumnDataType(this Type columnType, bool bypassNullable)
        {
            for (var type = columnType; type != null; type = type.GetTypeInfo().BaseType)
            {
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Column<>))
                {
                    var typeParam = type.GetGenericArguments()[0];
                    if (!bypassNullable)
                        return typeParam;

                    if (typeParam.GetTypeInfo().IsGenericType && typeParam.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return typeParam.GetGenericArguments()[0];
                    else
                        return null;
                }
            }
            return null;
        }
    }
}