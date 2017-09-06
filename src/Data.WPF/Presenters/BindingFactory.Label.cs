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
    }
}
