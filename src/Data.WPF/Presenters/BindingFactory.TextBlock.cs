using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<TextBlock> BindToTextBlock(this Column source, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<TextBlock>(
                onRefresh: (v, p) =>
                {
                    v.Text = p.GetObject(source)?.ToString(format, formatProvider);
                });
        }

        public static RowBinding<TextBlock> BindToTextBlock(this Model _, string text)
        {
            return new RowBinding<TextBlock>(
                onSetup: (v, p) =>
                {
                    v.Text = text;
                },
                onRefresh: null,
                onCleanup: null);
        }

        public static RowBinding<TextBlock> BindToTextBlockHyperlink(this Column source, ICommand command, string format = null, IFormatProvider formatProvider = null)
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
                    run.Text = p.GetObject(source).ToString(format, formatProvider);              
                },
                onCleanup: (v, p) =>
                {
                    v.Inlines.Clear();
                });
        }

        public static ScalarBinding<TextBlock> BindToTextBlock<T>(this Func<T> source, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<TextBlock>(
                onRefresh: (v, p) =>
                {
                    v.Text = source().ToString(format, formatProvider);
                });
        }

        public static ScalarBinding<TextBlock> BindToTextBlock(this DataPresenter dataPresenter, string text)
        {
            return new ScalarBinding<TextBlock>(
                onSetup: v =>
                {
                    v.Text = text;
                },
                onRefresh: null,
                onCleanup: null);
        }

        public static ScalarBinding<TextBlock> BindToTextBlock(this Scalar source, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<TextBlock>(
                onRefresh: v =>
                {
                    v.Text = source.GetObject().ToString(format, formatProvider);
                });
        }
    }
}
