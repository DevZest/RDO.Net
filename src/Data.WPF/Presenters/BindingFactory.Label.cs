using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<Label> AsLabel<TTarget>(this Column source, RowBinding<TTarget> target = null, string format = null, IFormatProvider formatProvider = null)
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

        public static ScalarBinding<Label> AsLabel(this string source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<Label>(onSetup: (v, p) => v.Content = source, onRefresh: null, onCleanup: null);
        }


        public static ScalarBinding<Label> AsLabel<TTarget>(this string source, ScalarBinding<TTarget> target)
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
