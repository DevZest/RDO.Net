using DevZest.Data.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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
                    var messages = GetValidationMessages(errors, warnings);
                    if (ShouldUpdateItemsSource(v, messages))
                    {
                        v.ItemsSource = messages;
                        p.DataPresenter.InvalidateMeasure();
                    }
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

        private static bool ShouldUpdateItemsSource(ValidationMessagesControl control, IReadOnlyList<ValidationMessage> messages)
        {
            var itemsSource = control.ItemsSource as IReadOnlyList<ValidationMessage>;
            return itemsSource == null ? true : !AreEqual(itemsSource, messages);
        }

        private static bool AreEqual(IReadOnlyList<ValidationMessage> list1, IReadOnlyList<ValidationMessage> list2)
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
                if (item1.Severity != item2.Severity || item1.Description != item2.Description)
                    return false;
            }
            return true;
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
                    var messages = GetValidationMessages(errors, warnings);
                    if (ShouldUpdateItemsSource(v, messages))
                    {
                        v.ItemsSource = messages;
                        p.DataPresenter.InvalidateMeasure();
                    }
                },
                onSetup: null, onCleanup: null);
        }
    }
}
