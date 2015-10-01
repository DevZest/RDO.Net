using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    /// <summary>Extension methods for <see cref="Column"/>.</summary>
    public static class ColumnExtensions
    {
        internal static void Construct<T>(this T column, Model parentModel, Type ownerType, string name, ColumnKind kind, Action<T> baseInitializer, Action<T> initializer)
            where T : Column
        {
            initializer = column.MergeInitializer(baseInitializer, initializer);
            Action<Column> action = null;
            if (initializer != null)
            {
                ColumnInitializerManager<T>.SetInitializer(column, initializer);
                action = (x) => initializer((T)x);
            }
            column.Initialize(parentModel, ownerType, name, kind, action);
        }

        private static Action<T> MergeInitializer<T>(this T column, Action<T> baseInitializer, Action<T> initializer)
        {
            var result = initializer;
            if (baseInitializer != null)
            {
                result = result == null ? baseInitializer : x =>
                {
                    baseInitializer(x);
                    initializer(x);
                };
            }

            return result;
        }

        /// <summary>Declares default expression for column.</summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="expression">The expression.</param>
        /// <remarks>To define default constant value, call <see cref="Column{T}.DefaultValue(T)"/> method.</remarks>
        public static void Default<T>(this T column, T expression)
            where T : Column, new()
        {
            Check.NotNull(column, nameof(column));
            Check.NotNull(expression, nameof(expression));

            column.AddOrUpdateInterceptor(expression.CreateDefault());
        }
    }
}
