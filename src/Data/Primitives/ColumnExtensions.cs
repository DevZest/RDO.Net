using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    /// <summary>Extension methods for <see cref="Column"/>.</summary>
    public static class ColumnExtensions
    {
        internal static void Construct<T>(this T column, Model parentModel, Type declaringType, string name, ColumnKind kind, Action<T> baseInitializer, Action<T> initializer)
            where T : Column
        {
            initializer = column.MergeInitializer(baseInitializer, initializer);
            Action<Column> action = null;
            if (initializer != null)
            {
                ColumnInitializerManager<T>.SetInitializer(column, initializer);
                action = (x) => initializer((T)x);
            }
            column.Initialize(parentModel, declaringType, name, kind, action);
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

        /// <summary>Specifies default expression for column.</summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="name">The name of the default constraint.</param>
        /// <param name="description">The  description of the default constraint.</param>
        /// <remarks>To specify default constant value, call <see cref="Column{T}.SetDefaultValue(T, string, string)"/> method.</remarks>
        public static void SetDefault<T>(this T column, T expression, string name, string description)
            where T : Column, new()
        {
            column.VerifyNotNull(nameof(column));
            expression.VerifyNotNull(nameof(expression));

            column.AddOrUpdate(expression.CreateDefault(name, description));
        }

        /// <summary>Specifies default expression for column.</summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="name">The name of the default constraint.</param>
        /// <param name="description">The  description of the default constraint.</param>
        /// <returns>This column for fluent coding.</returns>
        /// <remarks>To specify default constant value, call <see cref="WithDefaultValue{TColumn, TValue}(TColumn, TValue, string, string)"/> method.</remarks>
        public static T WithDefault<T>(this T column, T expression, string name, string description)
            where T : Column, new()
        {
            column.SetDefault(expression, name, description);
            return column;
        }

        /// <summary>
        /// Specifies default value for column.
        /// </summary>
        /// <typeparam name="TColumn">Type of column.</typeparam>
        /// <typeparam name="TValue">Type of value.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <param name="name">The name of the default constraint.</param>
        /// <param name="description">The description of the default constraint.</param>
        /// <returns>This column for fluent coding.</returns>
        public static TColumn WithDefaultValue<TColumn, TValue>(this TColumn column, TValue value, string name, string description)
            where TColumn : Column<TValue>, new()
        {
            column.SetDefaultValue(value, name, description);
            return column;
        }

        /// <summary>
        /// Specifies display description for column.
        /// </summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>This column for fluent coding.</returns>
        public static T WithDisplayDescription<T>(this T column, string value)
            where T : Column, new()
        {
            column.DisplayDescription = value;
            return column;
        }

        /// <summary>
        /// Specifies display description for column.
        /// </summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="displayDescriptionGetter">Delegate to get the display description.</param>
        /// <returns>This column for fluent coding.</returns>
        public static T WithDisplayDescription<T>(this T column, Func<string> displayDescriptionGetter)
            where T : Column, new()
        {
            column.SetDisplayDescription(displayDescriptionGetter);
            return column;
        }

        /// <summary>
        /// Specifies display name for column.
        /// </summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>This column for fluent coding.</returns>
        public static T WithDisplayName<T>(this T column, string value)
            where T : Column, new()
        {
            column.DisplayName = value;
            return column;
        }

        /// <summary>
        /// Specifies display name for column.
        /// </summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="displayNameGetter">Delegate to get the display name.</param>
        /// <returns>This column for fluent coding.</returns>
        public static T WithDisplayName<T>(this T column, Func<string> displayNameGetter)
            where T : Column, new()
        {
            column.SetDisplayName(displayNameGetter);
            return column;
        }

        /// <summary>
        /// Specifies display prompt for column.
        /// </summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>This column for fluent coding.</returns>
        public static T WithDisplayPrompt<T>(this T column, string value)
            where T : Column, new()
        {
            column.DisplayPrompt = value;
            return column;
        }

        /// <summary>
        /// Specifies display prompt for column.
        /// </summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="displayPromptGetter">Delegate to get display prompt.</param>
        /// <returns>This column for fluent coding.</returns>
        public static T WithDisplayPrompt<T>(this T column, Func<string> displayPromptGetter)
            where T : Column, new()
        {
            column.SetDisplayPrompt(displayPromptGetter);
            return column;
        }

        /// <summary>
        /// Specifies display short name for column.
        /// </summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>This column for fluent coding.</returns>
        public static T WithDisplayShortName<T>(this T column, string value)
            where T : Column, new()
        {
            column.DisplayShortName = value;
            return column;
        }

        /// <summary>
        /// Specifies display short name for column.
        /// </summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="displayShortNameGetter">Delegate to return display short name.</param>
        /// <returns>This column for fluent coding.</returns>
        public static T WithDisplayShortName<T>(this T column, Func<string> displayShortNameGetter)
            where T : Column, new()
        {
            column.SetDisplayShortName(displayShortNameGetter);
            return column;
        }

        /// <summary>
        /// Specifies value comparer for column.
        /// </summary>
        /// <typeparam name="TColumn">Type of the column.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>This column for fluent coding.</returns>
        public static TColumn WithValueComparer<TColumn, TValue>(this TColumn column, IComparer<TValue> value)
            where TColumn : Column<TValue>, new()
        {
            column.ValueComparer = value;
            return column;
        }

        /// <summary>
        /// Translates the column to another model.
        /// </summary>
        /// <typeparam name="T">Type of column.</typeparam>
        /// <param name="column">The source column.</param>
        /// <param name="model">The model.</param>
        /// <returns>The translated column.</returns>
        public static T TranslateTo<T>(this T column, Model model)
            where T : Column
        {
            if (column == null)
                return null;
            model.VerifyNotNull(nameof(model));
            return (T)column.PerformTranslateTo(model);
        }

        /// <summary>
        /// Enforces mapping between two columns.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <param name="target">The target column.</param>
        /// <returns>The result column mapping.</returns>
        public static ColumnMapping UnsafeMap(this Column source, Column target)
        {
            source.VerifyNotNull(nameof(source));
            target.VerifyNotNull(nameof(target));
            return new ColumnMapping(source, target);
        }
    }
}
