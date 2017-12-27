using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<ValidationSeverityIndicator> AsValidationSeverityIndicator(this IColumns source)
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

        public static ScalarBinding<ValidationSeverityIndicator> AsValidationSeverityIndicator(this IScalars source, bool isProgressive = true)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<ValidationSeverityIndicator>(
                onRefresh: (v, p) =>
                {
                    var dataPresenter = p.DataPresenter;
                    if (dataPresenter.GetValidationErrors(source).Count > 0)
                        v.ValidationSeverity = ValidationSeverity.Error;
                    else if (dataPresenter.GetValidationWarnings(source).Count > 0)
                        v.ValidationSeverity = ValidationSeverity.Warning;
                    else
                        v.ValidationSeverity = null;
                },
                onSetup: null, onCleanup: null);
        }
    }
}
