using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<ValidationPlaceholder> BindToValidationPlaceholder(this IColumns source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var explicitTrigger = new ExplicitTrigger<ValidationPlaceholder>();
            var input = new RowBinding<ValidationPlaceholder>(null).BeginInput(explicitTrigger);

            foreach (var column in source)
            {
                input.WithFlush(column, (r, v) => true);
            }
            return input.EndInput();
        }

        public static ScalarBinding<ValidationPlaceholder> BindToValidationPlaceholder(this IScalars source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var explicitTrigger = new ExplicitTrigger<ValidationPlaceholder>();
            var input = new ScalarBinding<ValidationPlaceholder>((Action<ValidationPlaceholder>)null).BeginInput(explicitTrigger);

            foreach (var scalar in source)
            {
                input.WithFlush(scalar, v => true);
            }
            return input.EndInput();
        }
    }
}
