using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<ValidationErrorsControl> BindToValidationErrorsControl(this Model source)
        {
            return new RowBinding<ValidationErrorsControl>(
                onRefresh: (v, p) =>
                {
                    var errors = p.VisibleValidationErrors;
                    if (ShouldUpdateItemsSource(v, errors))
                        v.ItemsSource = errors;
                },
                onSetup: null, onCleanup: null);
        }

        public static RowBinding<ValidationErrorsControl> BindToValidationErrorsControl<T>(this RowInput<T> source)
            where T : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<ValidationErrorsControl>(
                onRefresh: (v, p) =>
                {
                    var errors = source.GetValidationInfo(p).Errors;
                    if (ShouldUpdateItemsSource(v, errors))
                        v.ItemsSource = errors;
                },
                onSetup: null, onCleanup: null);
        }

        private static bool ShouldUpdateItemsSource(ValidationErrorsControl control, IReadOnlyList<ValidationError> errors)
        {
            var itemsSource = control.ItemsSource as IReadOnlyList<ValidationError>;
            return itemsSource == null ? true : !AreEqual(itemsSource, errors);
        }

        private static bool AreEqual(IReadOnlyList<ValidationError> list1, IReadOnlyList<ValidationError> list2)
        {
            Debug.Assert(list1 != null);
            Debug.Assert(list2 != null);

            if (list1 == list2)
                return true;

            if (list1.Count != list2.Count)
                return false;

            for (int i = 0; i < list1.Count; i++)
            {
                var item1 = list1[i];
                var item2 = list2[i];
                if (item1 == item2)
                    continue;
                if (item1.Message != item2.Message)
                    return false;
            }
            return true;
        }

        public static ScalarBinding<ValidationErrorsControl> BindToValidationErrorsControl(this BasePresenter source)
        {
            return new ScalarBinding<ValidationErrorsControl>(
                onRefresh: (v, p) =>
                {
                    var errors = p.Presenter.ScalarValidation.VisibleErrors;
                    if (ShouldUpdateItemsSource(v, errors))
                        v.ItemsSource = errors;
                },
                onSetup: null, onCleanup: null);
        }

        public static ScalarBinding<ValidationErrorsControl> BindToValidationErrorsControl<T>(this ScalarInput<T> source)
            where T : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<ValidationErrorsControl>(
                onRefresh: (v, p) =>
                {
                    var errors = source.GetValidationInfo(p.FlowIndex).Errors;
                    if (ShouldUpdateItemsSource(v, errors))
                        v.ItemsSource = errors;
                },
                onSetup: null, onCleanup: null);
        }
    }
}
