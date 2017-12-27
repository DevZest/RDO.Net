using DevZest.Data.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<ValidationSeverityIndicator> AsValidationSeverityIndicator(this IColumns source, bool isProgressive = true)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<ValidationSeverityIndicator>(
                onRefresh: (v, p) =>
                {
                    if (IsVisible(source, p, ValidationSeverity.Error, isProgressive))
                        v.ValidationSeverity = ValidationSeverity.Error;
                    else if (IsVisible(source, p, ValidationSeverity.Warning, isProgressive))
                        v.ValidationSeverity = ValidationSeverity.Warning;
                    else
                        v.ValidationSeverity = null;
                },
                onSetup: null, onCleanup: null);
        }

        private static bool IsVisible(IColumns source, RowPresenter row, ValidationSeverity severity, bool isProgressive)
        {
            var dataPresenter = row.DataPresenter;
            var messagesByRow = severity == ValidationSeverity.Error ? dataPresenter.RowErrors : dataPresenter.RowWarnings;

            if (!messagesByRow.ContainsKey(row))
                return false;

            var messages = messagesByRow[row];
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (!message.Source.SetEquals(source))
                    continue;
                if (isProgressive)
                {
                    if (dataPresenter.RowValidationProgress.IsVisible(row, source))
                        return true;
                }
            }

            return false;
        }

        public static ScalarBinding<ValidationSeverityIndicator> AsValidationSeverityIndicator(this IScalars source, bool isProgressive = true)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            throw new NotImplementedException();
        }
    }
}
