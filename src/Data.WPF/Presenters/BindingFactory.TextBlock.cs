using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        /// <summary>
        /// Binds column to <see cref="TextBlock"/>.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<TextBlock> BindToTextBlock(this Column source, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<TextBlock>(
                onRefresh: (v, p) =>
                {
                    v.Text = p[source]?.ToString(format, formatProvider);
                });
        }
        /// <summary>
        /// Binds column to <see cref="TextBlock"/>.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <param name="format">A delegate to return composite format string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<TextBlock> BindToTextBlock(this Column source, Func<RowPresenter, string> format, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (format == null)
                throw new ArgumentNullException(nameof(format));

            return new RowBinding<TextBlock>(
                onRefresh: (v, p) =>
                {
                    v.Text = p[source]?.ToString(format(p), formatProvider);
                });
        }

        /// <summary>
        /// Binds text to <see cref="TextBlock"/> as row binding.
        /// </summary>
        /// <param name="_">The model.</param>
        /// <param name="text">The text.</param>
        /// <returns>The row binding object.</returns>
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

        /// <summary>
        /// Binds column to hyperlink <see cref="TextBlock"/>.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <param name="command">The command.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<TextBlock> BindToHyperlink(this Column source, ICommand command, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<TextBlock>(
                onSetup: (v, p) =>
                {
                    var hyperlink = new Hyperlink(new Run())
                    {
                        Command = command
                    };
                    v.Inlines.Add(hyperlink);
                },
                onRefresh: (v, p) =>
                {
                    var hyperlink = (Hyperlink)v.Inlines.FirstInline;
                    var run = (Run)hyperlink.Inlines.FirstInline;
                    run.Text = p[source]?.ToString(format, formatProvider);              
                },
                onCleanup: (v, p) =>
                {
                    v.Inlines.Clear();
                });
        }

        /// <summary>
        /// Binds function to <see cref="TextBlock"/>.
        /// </summary>
        /// <typeparam name="T">The function return type.</typeparam>
        /// <param name="source">The source function that returns a value.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>The scalar binding object.</returns>
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

        /// <summary>
        /// Binds the text to <see cref="TextBlock"/> as scalar binding.
        /// </summary>
        /// <param name="presenter">The presenter.</param>
        /// <param name="text">The text.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBlock> BindToTextBlock(this BasePresenter presenter, string text)
        {
            return new ScalarBinding<TextBlock>(
                onSetup: v =>
                {
                    v.Text = text;
                },
                onRefresh: null,
                onCleanup: null);
        }

        /// <summary>
        /// Binds scalar data to <see cref="TextBlock"/>.
        /// </summary>
        /// <param name="source">The source scalar data.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBlock> BindToTextBlock(this Scalar source, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<TextBlock>(
                onRefresh: v =>
                {
                    v.Text = source.GetValue().ToString(format, formatProvider);
                });
        }
    }
}
