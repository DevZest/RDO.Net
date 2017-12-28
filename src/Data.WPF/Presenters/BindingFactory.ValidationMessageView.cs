using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<ValidationMessageView> BindToValidationMessageView(this IColumns source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<ValidationMessageView>(
                onRefresh: (v, p) =>
                {
                    {
                        var errors = p.GetValidationErrors(source);
                        if (errors.Count > 0)
                        {
                            v.Message = errors[0];
                            return;
                        }
                    }
                    {
                        var warnings = p.GetValidationErrors(source);
                        if (warnings.Count > 0)
                        {
                            v.Message = warnings[0];
                            return;
                        }
                    }
                    v.Message = null;
                },
                onSetup: null, onCleanup: null);
        }

        public static ScalarBinding<ValidationMessageView> BindToValidationMessageView(this IScalars source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<ValidationMessageView>(
                onRefresh: (v, p) =>
                {
                    var validation = p.DataPresenter.ScalarValidation;
                    {
                        var errors = validation.GetErrors(source);
                        if (errors.Count > 0)
                        {
                            v.Message = errors[0];
                            return;
                        }
                    }
                    {
                        var warnings = validation.GetWarnings(source);
                        if (warnings.Count > 0)
                        {
                            v.Message = warnings[0];
                            return;
                        }
                    }
                    v.Message = null;
                },
                onSetup: null, onCleanup: null);
        }
    }
}
