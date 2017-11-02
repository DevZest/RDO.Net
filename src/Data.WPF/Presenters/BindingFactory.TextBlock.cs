using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

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

        public static RowBinding<TextBlock> AsTextBlockOfHyperlink(this Column source, ICommand command, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<TextBlock>(
                onSetup: (v, p) =>
                {
                    var hyperlink = new Hyperlink(new Run());
                    hyperlink.Command = command;
                    v.Inlines.Add(hyperlink);
                },
                onRefresh: (v, p) =>
                {
                    var hyperlink = (Hyperlink)v.Inlines.FirstInline;
                    var run = (Run)hyperlink.Inlines.FirstInline;
                    run.Text = source.GetValue(p.DataRow).ToString(format, formatProvider);              
                },
                onCleanup: (v, p) =>
                {
                    v.Inlines.Clear();
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

        public static ScalarBinding<TextBlock> AsTextBlock(this string text)
        {
            return new ScalarBinding<TextBlock>(
                onSetup: e =>
                {
                    e.Text = text;
                },
                onRefresh: null,
                onCleanup: null);
        }

        public static ScalarBinding<TextBlock> AsTextBlock<T>(this Scalar<T> source, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<TextBlock>(
                onRefresh: e =>
                {
                    e.Text = source.Value.ToString(format, formatProvider);
                });
        }
    }
}
