using System;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<TextBlock> AsTextBlock(this Column source, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<TextBlock>(
                onRefresh: (v, p) =>
                {
                    v.Text = source.GetValue(p.DataRow).ToString(format, formatProvider);
                });
        }

        public static ScalarBinding<TextBlock> AsTextBlock<T>(this Func<T> source, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<TextBlock>(
                onRefresh: (v, p) =>
                {
                    v.Text = source().ToString(format, formatProvider);
                });
        }
    }
}
