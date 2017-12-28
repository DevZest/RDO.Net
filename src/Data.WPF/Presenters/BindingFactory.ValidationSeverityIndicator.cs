using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<ValidationSeverityIndicator> BindToValidationSeverityIndicator(this IColumns source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<ValidationSeverityIndicator>(
                onRefresh: (v, p) =>
                {
                    if (p.GetValidationErrors(source).Count > 0)
                        v.ValidationSeverity = ValidationSeverity.Error;
                    else if (p.GetValidationWarnings(source).Count > 0)
                        v.ValidationSeverity = ValidationSeverity.Warning;
                    else
                        v.ValidationSeverity = null;
                },
                onSetup: null, onCleanup: null);
        }

        public static ScalarBinding<ValidationSeverityIndicator> BindToValidationSeverityIndicator(this IScalars source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<ValidationSeverityIndicator>(
                onRefresh: (v, p) =>
                {
                    var validation = p.DataPresenter.ScalarValidation;
                    if (validation.GetErrors(source).Count > 0)
                        v.ValidationSeverity = ValidationSeverity.Error;
                    else if (validation.GetWarnings(source).Count > 0)
                        v.ValidationSeverity = ValidationSeverity.Warning;
                    else
                        v.ValidationSeverity = null;
                },
                onSetup: null, onCleanup: null);
        }
    }
}
