using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public static class BindingFactory
    {
        public static ScalarBinding<ColumnHeader> ColumnHeader(this Column source)
        {
            return new ScalarBinding<ColumnHeader>(
                onRefresh: e =>
                {
                    e.Column = source;
                });
        }

        public static RowBinding<RowHeader> RowHeader(this Model source)
        {
            return new RowBinding<RowHeader>(
                onRefresh: (e, r) =>
                {
                    e.IsCurrent = r.IsCurrent;
                    e.IsSelected = r.IsSelected;
                    e.IsEditing = r.IsEditing;
                });
        }

        public static ScalarBinding<TextBlock> TextBlock<T>(this Scalar<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<TextBlock>(
                onRefresh: e =>
                {
                    e.Text = source.Value.ToString();
                });
        }

        public static ScalarBinding<TextBlock> TextBlock<T>(this Scalar<T> source, string format)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            return new ScalarBinding<TextBlock>(
                onRefresh: e =>
                {
                    e.Text = string.Format(format, source.Value);
                });
        }

        public static ScalarBinding<TextBlock> TextBlock<T>(this Scalar<T> source, IFormatProvider formatProvider, string format)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (formatProvider == null)
                throw new ArgumentNullException(nameof(formatProvider));
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            return new ScalarBinding<TextBlock>(
                onRefresh: e =>
                {
                    e.Text = string.Format(formatProvider, format, source.Value);
                });
        }

        public static RowBinding<TextBlock> TextBlock<T>(this Column<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<TextBlock>(
                onRefresh: (e, r) =>
                {
                    e.Text = r.GetValue(source).ToString();
                });
        }

        public static RowBinding<TextBlock> TextBlock<T>(this Column<T> source, string format)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            return new RowBinding<TextBlock>(
                onRefresh: (e, r) =>
                {
                    e.Text = string.Format(format, r.GetValue(source));
                });
        }

        public static RowBinding<TextBlock> TextBlock<T>(this Column<T> source, IFormatProvider formatProvider, string format)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (formatProvider == null)
                throw new ArgumentNullException(nameof(formatProvider));
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            return new RowBinding<TextBlock>(
                onRefresh: (e, r) =>
                {
                    e.Text = string.Format(formatProvider, format, r.GetValue(source));
                });
        }

        public static RowBinding<ValidationView> ValidationView<T>(this RowInput<T> source)
            where T : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<ValidationView>(
                onRefresh: (e, r) =>
                {
                    e.AsyncValidators = source.AsyncValidators;
                },
                onSetup: (e, r) =>
                {
                    e.Errors = source.GetErrors(r);
                    e.Warnings = source.GetErrors(r);
                    e.RefreshStatus();
                },
                onCleanup: (e, r) =>
                {
                    e.AsyncValidators = AsyncValidatorGroup.Empty;
                    e.Errors = e.Warnings = AbstractValidationMessageGroup.Empty;
                });
        }

        public static RowBinding<ValidationView> ValidationView(this Model source)
        {
            return new RowBinding<ValidationView>(
                onRefresh: (e, r) =>
                {
                    e.AsyncValidators = r.AsyncValidators;
                },
                onSetup: (e, r) =>
                {
                    e.RefreshStatus();
                },
                onCleanup: (e, r) =>
                {
                    e.AsyncValidators = AsyncValidatorGroup.Empty;
                });
        }

        public static ScalarBinding<ValidationView> ValidationView(this DataPresenter source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<ValidationView>(
                onRefresh: e =>
                {
                    e.AsyncValidators = source.AsyncValidators;
                },
                onSetup: e =>
                {
                    e.RefreshStatus();
                },
                onCleanup: e =>
                {
                    e.AsyncValidators = AsyncValidatorGroup.Empty;
                });
        }
    }
}
