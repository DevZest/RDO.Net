using DevZest.Data.Views;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<ValidationMessagesControl> BindToValidationMessagesControl(this IColumns source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<ValidationMessagesControl>(
                onRefresh: (v, p) =>
                {
                    var errors = p.GetValidationErrors(source);
                    var warnings = p.GetValidationWarnings(source);
                    v.ItemsSource = GetValidationMessages(errors, warnings);
                    p.DataPresenter.InvalidateMeasure();
                },
                onSetup: null, onCleanup: null);
        }

        private static IReadOnlyList<ValidationMessage> GetValidationMessages(IReadOnlyList<ValidationMessage> errors, IReadOnlyList<ValidationMessage> warnings)
        {
            var totalCount = errors.Count + warnings.Count;
            if (totalCount == 0)
                return Array<ValidationMessage>.Empty;

            var result = new ValidationMessage[totalCount];
            for (int i = 0; i < errors.Count; i++)
                result[i] = errors[i];
            for (int i = 0; i < warnings.Count; i++)
                result[errors.Count + i] = warnings[i];

            return result;
        }

        public static ScalarBinding<ValidationMessagesControl> BindToValidationMessageView(this IScalars source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<ValidationMessagesControl>(
                onRefresh: (v, p) =>
                {
                    var validation = p.DataPresenter.ScalarValidation;
                    var errors = validation.GetErrors(source);
                    var warnings = validation.GetWarnings(source);
                    v.ItemsSource = GetValidationMessages(errors, warnings);
                    p.DataPresenter.InvalidateMeasure();
                },
                onSetup: null, onCleanup: null);
        }
    }
}
