using DevZest.Data;
using DevZest.Windows.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Windows
{
    public static partial class BindingFactory
    {
        public static ScalarBinding<ColumnHeader> ColumnHeader(this Column source)
        {
            return new ScalarBinding<ColumnHeader>(
                onRefresh: e =>
                {
                    e.Column = source;
                });
        }

        public static RowBinding<Label> Label<TTarget>(this Column source, RowBinding<TTarget> target = null, string format = null, IFormatProvider formatProvider = null)
            where TTarget : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<Label>(
                onSetup: (e, r) =>
                {
                    e.Content = source.DisplayName.ToString(format, formatProvider);
                    if (target != null)
                        e.Target = target.SettingUpElement;
                },
                onRefresh: (e, r) =>
                {
                },
                onCleanup: (e, r) =>
                {
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

        public static ScalarBinding<TextBlock> TextBlock<T>(this Scalar<T> source, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<TextBlock>(
                onRefresh: e =>
                {
                    e.Text = source.Value.ToString(format, formatProvider);
                });
        }

        public static RowBinding<TextBlock> TextBlock<T>(this Column<T> source, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<TextBlock>(
                onRefresh: (e, r) =>
                {
                    e.Text = r.GetValue(source).ToString(format, formatProvider);
                });
        }

        public static RowBinding<ValidationView> ValidationView<T>(this RowInput<T> source)
            where T : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<ValidationView>(
                onSetup: (e, r) =>
                {
                    e.AsyncValidators = source.AsyncValidators;
                },
                onRefresh: (e, r) =>
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
                onSetup: (e, r) =>
                {
                    e.AsyncValidators = r.AsyncValidators;
                },
                onRefresh: (e, r) =>
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

        public static RowBinding<Image> Image(this Column<ImageSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new RowBinding<Image>(onRefresh: (e, row) =>
            {
                e.Source = row.GetValue(source);
            });
        }
    }
}
