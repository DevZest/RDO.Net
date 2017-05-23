using DevZest.Data;
using DevZest.Data.Primitives;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Windows
{
    public class ValidationView : Control
    {
        public static readonly DependencyProperty ErrorsProperty = DependencyProperty.Register(nameof(Errors),
            typeof(IAbstractValidationMessageGroup), typeof(ValidationView), new FrameworkPropertyMetadata(AbstractValidationMessageGroup.Empty));

        public static readonly DependencyProperty WarningsProperty = DependencyProperty.Register(nameof(Warnings),
            typeof(IAbstractValidationMessageGroup), typeof(ValidationView), new FrameworkPropertyMetadata(AbstractValidationMessageGroup.Empty));

        public static readonly DependencyProperty AsyncValidatorsProperty = DependencyProperty.Register(nameof(AsyncValidators),
            typeof(IAsyncValidatorGroup), typeof(ValidationView), new FrameworkPropertyMetadata(AsyncValidatorGroup.Empty, OnAsyncValidatorsChanged));

        private static readonly DependencyPropertyKey RunningAsyncValidatorsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(RunningAsyncValidators),
            typeof(IAsyncValidatorGroup), typeof(ValidationView), new FrameworkPropertyMetadata(AsyncValidatorGroup.Empty));
        public static readonly DependencyProperty RunningAsyncValidatorsProperty = RunningAsyncValidatorsPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey CompletedAsyncValidatorsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CompletedAsyncValidators),
            typeof(IAsyncValidatorGroup), typeof(ValidationView), new FrameworkPropertyMetadata(AsyncValidatorGroup.Empty));
        public static readonly DependencyProperty CompletedAsyncValidatorsProperty = CompletedAsyncValidatorsPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey FaultedAsyncValidatorsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(FaultedAsyncValidators),
            typeof(IAsyncValidatorGroup), typeof(ValidationView), new FrameworkPropertyMetadata(AsyncValidatorGroup.Empty));
        public static readonly DependencyProperty FaultedAsyncValidatorsProperty = FaultedAsyncValidatorsPropertyKey.DependencyProperty;

        private static void OnAsyncValidatorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ValidationView)d).RefreshStatus();
        }

        public IAbstractValidationMessageGroup Errors
        {
            get { return (IAbstractValidationMessageGroup)GetValue(ErrorsProperty); }
            set
            {
                if (value == null || value.Count == 0)
                    ClearValue(ErrorsProperty);
                else
                    SetValue(ErrorsProperty, value);
            }
        }

        public IAbstractValidationMessageGroup Warnings
        {
            get { return (IAbstractValidationMessageGroup)GetValue(WarningsProperty); }
            set
            {
                if (value == null || value.Count == 0)
                    ClearValue(WarningsProperty);
                else
                    SetValue(WarningsProperty, value);
            }
        }

        public IAsyncValidatorGroup AsyncValidators
        {
            get { return (IAsyncValidatorGroup)GetValue(AsyncValidatorsProperty); }
            set
            {
                if (value == null || value.Count == 0)
                    ClearValue(AsyncValidatorsProperty);
                else
                    SetValue(AsyncValidatorsProperty, value);
            }
        }

        public IAsyncValidatorGroup RunningAsyncValidators
        {
            get { return (IAsyncValidatorGroup)GetValue(RunningAsyncValidatorsProperty); }
            set
            {
                if (value == null || value.Count == 0)
                    ClearValue(RunningAsyncValidatorsPropertyKey);
                else
                    SetValue(RunningAsyncValidatorsPropertyKey, value);
            }
        }

        public IAsyncValidatorGroup CompletedAsyncValidators
        {
            get { return (IAsyncValidatorGroup)GetValue(CompletedAsyncValidatorsProperty); }
            set
            {
                if (value == null || value.Count == 0)
                    ClearValue(CompletedAsyncValidatorsPropertyKey);
                else
                    SetValue(CompletedAsyncValidatorsPropertyKey, value);
            }
        }

        public IAsyncValidatorGroup FaultedAsyncValidators
        {
            get { return (IAsyncValidatorGroup)GetValue(FaultedAsyncValidatorsProperty); }
            set
            {
                if (value == null || value.Count == 0)
                    ClearValue(FaultedAsyncValidatorsPropertyKey);
                else
                    SetValue(FaultedAsyncValidatorsPropertyKey, value);
            }
        }

        public void RefreshStatus()
        {
            if (AnyStatusChange(AsyncValidatorStatus.Running, RunningAsyncValidators))
                RunningAsyncValidators = AsyncValidators.Where(x => x.Status == AsyncValidatorStatus.Running);
            if (AnyStatusChange(AsyncValidatorStatus.Completed, CompletedAsyncValidators))
                CompletedAsyncValidators = AsyncValidators.Where(x => x.Status == AsyncValidatorStatus.Completed);
            if (AnyStatusChange(AsyncValidatorStatus.Faulted, FaultedAsyncValidators))
                FaultedAsyncValidators = AsyncValidators.Where(x => x.Status == AsyncValidatorStatus.Faulted);
        }

        private bool AnyStatusChange(AsyncValidatorStatus status, IAsyncValidatorGroup statusAsyncValidators)
        {
            return AnyStatusChange(AsyncValidators, status, statusAsyncValidators);
        }

        /// <remarks>Predicts if async validtor status has been changed to avoid unnecessary object creation.</remarks>
        private static bool AnyStatusChange(IAsyncValidatorGroup asyncValidators, AsyncValidatorStatus status, IAsyncValidatorGroup statusAsyncValidators)
        {
            int count = 0;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                if (asyncValidator.Status != status)
                    continue;

                count++;
                if (statusAsyncValidators.Count < count || statusAsyncValidators[count - 1] != asyncValidator)
                    return true;
            }

            return statusAsyncValidators.Count != count;
        }
    }
}
