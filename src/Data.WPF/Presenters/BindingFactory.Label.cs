using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        /// <summary>
        /// Binds column to <see cref="Label"/>.
        /// </summary>
        /// <typeparam name="TTarget">The element type of target row binding.</typeparam>
        /// <param name="source">The source column.</param>
        /// <param name="target">The target row binding.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<Label> BindToLabel<TTarget>(this Column source, RowBinding<TTarget> target, string format = null, IFormatProvider formatProvider = null)
            where TTarget : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<Label>(
                onSetup: (v, p) =>
                {
                    v.Content = source.DisplayName.ToString(format, formatProvider);
                    if (target != null)
                        v.Target = target.SettingUpElement;
                },
                onRefresh: null, onCleanup: null);
        }

        /// <summary>
        /// Binds the string to <see cref="Label"/>.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<Label> BindToLabel(this string source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<Label>(onSetup: (v, p) => v.Content = source, onRefresh: null, onCleanup: null);
        }

        /// <summary>
        /// Binds the string to <see cref="Label"/>, with specified target scalar binding.
        /// </summary>
        /// <typeparam name="TTarget">The element type of target scalar binding.</typeparam>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target scalar binding.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<Label> BindToLabel<TTarget>(this string source, ScalarBinding<TTarget> target)
            where TTarget : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<Label>(
                onSetup: (v, p) =>
                {
                    v.Content = source;
                    v.Target = target.GetSettingUpElement(p);
                },
                onRefresh: null, onCleanup: null);
        }
    }
}
