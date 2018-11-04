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

        /// <summary>Declares default expression for column.</summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="expression">The expression.</param>
        /// <remarks>To define default constant value, call <see cref="Column{T}.SetDefaultValue(T)"/> method.</remarks>
        public static void SetDefault<T>(this T column, T expression, string name, string description)
            where T : Column, new()
        {
            column.VerifyNotNull(nameof(column));
            expression.VerifyNotNull(nameof(expression));

            column.AddOrUpdate(expression.CreateDefault(name, description));
        }

        public static T WithDefault<T>(this T column, T expression, string name, string description)
            where T : Column, new()
        {
            column.SetDefault(expression, name, description);
            return column;
        }

        public static TColumn WithDefaultValue<TColumn, TValue>(this TColumn column, TValue value, string name, string description)
            where TColumn : Column<TValue>, new()
        {
            column.SetDefaultValue(value, name, description);
            return column;
        }

        public static T WithDisplayDescription<T>(this T column, string value)
            where T : Column, new()
        {
            column.DisplayDescription = value;
            return column;
        }

        public static T WithDisplayDescription<T>(this T column, Func<string> displayDescriptionGetter)
            where T : Column, new()
        {
            column.SetDisplayDescription(displayDescriptionGetter);
            return column;
        }

        public static T WithDisplayName<T>(this T column, string value)
            where T : Column, new()
        {
            column.DisplayName = value;
            return column;
        }

        public static T WithDisplayName<T>(this T column, Func<string> displayNameGetter)
            where T : Column, new()
        {
            column.SetDisplayName(displayNameGetter);
            return column;
        }

        public static T WithDisplayPrompt<T>(this T column, string value)
            where T : Column, new()
        {
            column.DisplayPrompt = value;
            return column;
        }

        public static T WithDisplayPrompt<T>(this T column, Func<string> displayPromptGetter)
            where T : Column, new()
        {
            column.SetDisplayPrompt(displayPromptGetter);
            return column;
        }

        public static T WithDisplayShortName<T>(this T column, string value)
            where T : Column, new()
        {
            column.DisplayShortName = value;
            return column;
        }

        public static T WithDisplayShortName<T>(this T column, Func<string> displayShortNameGetter)
            where T : Column, new()
        {
            column.SetDisplayShortName(displayShortNameGetter);
            return column;
        }

        public static TColumn WithValueComparer<TColumn, TValue>(this TColumn column, IComparer<TValue> value)
            where TColumn : Column<TValue>, new()
        {
            column.ValueComparer = value;
            return column;
        }

        public static T TranslateTo<T>(this T column, Model model)
            where T : Column
        {
            if (column == null)
                return null;
            model.VerifyNotNull(nameof(model));
            return (T)column.PerformTranslateTo(model);
        }

        public static ColumnMapping UnsafeMap(this Column source, Column target)
        {
            source.VerifyNotNull(nameof(source));
            target.VerifyNotNull(nameof(target));
            return new ColumnMapping(source, target);
        }
    }
}
