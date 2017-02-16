using DevZest.Data.Windows.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevZest.Data.Windows
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
    }
}
