using DevZest.Data.Views;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<ValidityIndicator> BindToValidityIndicator<T>(this RowInput<T> source)
            where T : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<ValidityIndicator>(
                onRefresh: (v, p) =>
                {
                    v.IsValid = !source.HasValidationError(p);
                },
                onSetup: null, onCleanup: null);
        }

        public static ScalarBinding<ValidityIndicator> BindToValidityIndicator<T>(this ScalarInput<T> source)
            where T : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<ValidityIndicator>(
                onRefresh: (v, p) =>
                {
                    v.IsValid = !source.HasValidationError;
                },
                onSetup: null, onCleanup: null);
        }
    }
}
