using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<ValidityIndicator> BindToValidityIndicator(this IColumns source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<ValidityIndicator>(
                onRefresh: (v, p) =>
                {
                    if (p.GetValidationErrors(source).Count > 0)
                        v.IsValid = false;
                    else
                        v.IsValid = true;
                },
                onSetup: null, onCleanup: null);
        }

        public static ScalarBinding<ValidityIndicator> BindToValidityIndicator(this IScalars source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<ValidityIndicator>(
                onRefresh: (v, p) =>
                {
                    var validation = p.DataPresenter.ScalarValidation;
                    if (validation.GetErrors(source).Count > 0)
                        v.IsValid = false;
                    else
                        v.IsValid = true;
                },
                onSetup: null, onCleanup: null);
        }
    }
}
