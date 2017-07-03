using DevZest.Data;
using DevZest.Data.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<ComboBox> AsComboBox<T>(this Column<T> source, IEnumerable selectionData, string selectedValuePath, string displayMemberPath)
        {
            return new RowBinding<ComboBox>(
                onSetup: (e, r) =>
                {
                    e.ItemsSource = selectionData;
                    e.SelectedValuePath = selectedValuePath;
                    e.DisplayMemberPath = displayMemberPath;
                },
                onRefresh: (e, r) =>
                {
                    e.SelectedValue = r.GetValue(source);
                },
                onCleanup: (e, r) =>
                {
                    e.ItemsSource = null;
                    e.SelectedValuePath = null;
                    e.DisplayMemberPath = null;
                }
                ).WithInput(new PropertyChangedTrigger<ComboBox>(ComboBox.SelectedValueProperty), source, e => (T)e.SelectedValue);
        }

        public static ScalarBinding<ColumnHeader> AsColumnHeader(this Column column, object title = null)
        {
            return new ScalarBinding<ColumnHeader>(
                onRefresh: null,
                onCleanup: null,
                onSetup: e =>
                {
                    e.Column = column;
                    e.Content = title ?? column.DisplayName;
                });
        }

        public static RowBinding<Label> AsLabel<TTarget>(this Column source, RowBinding<TTarget> target = null, string format = null, IFormatProvider formatProvider = null)
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

        public static ScalarBinding<Label> AsScalarLabel<TTarget>(this Column source, ScalarBinding<TTarget> target = null, string format = null, IFormatProvider formatProvider = null)
            where TTarget : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<Label>(
                onSetup: e =>
                {
                    e.Content = source.DisplayName.ToString(format, formatProvider);
                    if (target != null)
                        e.Target = target.SettingUpElement;
                },
                onRefresh: null,
                onCleanup: null);
        }

        public static RowBinding<RowHeader> AsRowHeader(this Model source)
        {
            return new RowBinding<RowHeader>(
                onRefresh: (e, r) =>
                {
                    e.IsCurrent = r.IsCurrent;
                    e.IsSelected = r.IsSelected;
                    e.IsEditing = r.IsEditing;
                });
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

        public static RowBinding<TextBlock> AsTextBlock<T>(this Column<T> source, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<TextBlock>(
                onRefresh: (e, r) =>
                {
                    e.Text = r.GetValue(source).ToString(format, formatProvider);
                });
        }

        public static RowBinding<ValidationView> AsValidationView<T>(this RowInput<T> source)
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

        public static RowBinding<ValidationView> AsValidationView(this Model source)
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

        public static ScalarBinding<ValidationView> AsValidationView(this DataPresenter source)
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

        public static RowBinding<Image> AsImage(this Column<ImageSource> source)
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
