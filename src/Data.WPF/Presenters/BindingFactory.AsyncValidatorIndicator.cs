using DevZest.Data.Views;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<AsyncValidatorIndicator> BindToAsyncValidatorIndicator<T>(this RowAsyncValidator source)
            where T : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<AsyncValidatorIndicator>(
                onSetup: (v, p) =>
                {
                    v.Validator = source;
                }, onRefresh: null, onCleanup: null);
        }

        public static ScalarBinding<AsyncValidatorIndicator> BindToAsyncValidatorIndicator<T>(this ScalarAsyncValidator source)
            where T : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<AsyncValidatorIndicator>(
                onSetup: (v, p) =>
                {
                    v.Validator = source;
                }, onRefresh: null, onCleanup: null);
        }
    }
}
