using DevZest.Data.Presenters;
using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class ValidationView : Control
    {
        public static readonly DependencyProperty FlushErrorProperty = DependencyProperty.Register(nameof(FlushError),
            typeof(FlushErrorMessage), typeof(ValidationView), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ErrorsProperty = DependencyProperty.Register(nameof(Errors),
            typeof(IReadOnlyList<ValidationMessage>), typeof(ValidationView), new FrameworkPropertyMetadata(Array<ValidationMessage>.Empty));

        public static readonly DependencyProperty WarningsProperty = DependencyProperty.Register(nameof(Warnings),
            typeof(IReadOnlyList<ValidationMessage>), typeof(ValidationView), new FrameworkPropertyMetadata(Array<ValidationMessage>.Empty));

        public static readonly DependencyProperty AsyncValidatorsProperty = DependencyProperty.Register(nameof(ValidationView.AsyncValidators),
            typeof(IRowAsyncValidators), typeof(ValidationView), new FrameworkPropertyMetadata(Presenters.RowAsyncValidators.Empty, OnAsyncValidatorsChanged));

        private static readonly DependencyPropertyKey RunningAsyncValidatorsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ValidationView.RunningAsyncValidators),
            typeof(IRowAsyncValidators), typeof(ValidationView), new FrameworkPropertyMetadata(Presenters.RowAsyncValidators.Empty));
        public static readonly DependencyProperty RunningAsyncValidatorsProperty = RunningAsyncValidatorsPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey CompletedAsyncValidatorsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ValidationView.CompletedAsyncValidators),
            typeof(IRowAsyncValidators), typeof(ValidationView), new FrameworkPropertyMetadata(Presenters.RowAsyncValidators.Empty));
        public static readonly DependencyProperty CompletedAsyncValidatorsProperty = CompletedAsyncValidatorsPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey FaultedAsyncValidatorsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ValidationView.FaultedAsyncValidators),
            typeof(IRowAsyncValidators), typeof(ValidationView), new FrameworkPropertyMetadata(Presenters.RowAsyncValidators.Empty));
        public static readonly DependencyProperty FaultedAsyncValidatorsProperty = FaultedAsyncValidatorsPropertyKey.DependencyProperty;

        private static void OnAsyncValidatorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ValidationView)d).RefreshStatus();
        }

        public FlushErrorMessage FlushError
        {
            get { return (FlushErrorMessage)GetValue(FlushErrorProperty); }
            set
            {
                if (value == null)
                    ClearValue(FlushErrorProperty);
                else
                    SetValue(FlushErrorProperty, value);
            }
        }

        public IReadOnlyList<ValidationMessage> Errors
        {
            get { return (IReadOnlyList<ValidationMessage>)GetValue(ErrorsProperty); }
            set
            {
                if (value == null || value.Count == 0)
                    ClearValue(ErrorsProperty);
                else
                    SetValue(ErrorsProperty, value);
            }
        }

        public IReadOnlyList<ValidationMessage> Warnings
        {
            get { return (IReadOnlyList<ValidationMessage>)GetValue(WarningsProperty); }
            set
            {
                if (value == null || value.Count == 0)
                    ClearValue(WarningsProperty);
                else
                    SetValue(WarningsProperty, value);
            }
        }

        public IRowAsyncValidators AsyncValidators
        {
            get { return (IRowAsyncValidators)GetValue(AsyncValidatorsProperty); }
            set
            {
                if (value == null || value.Count == 0)
                    ClearValue(AsyncValidatorsProperty);
                else
                    SetValue(AsyncValidatorsProperty, value);
            }
        }

        public IRowAsyncValidators RunningAsyncValidators
        {
            get { return (IRowAsyncValidators)GetValue(RunningAsyncValidatorsProperty); }
            private set
            {
                if (value == null || value.Count == 0)
                    ClearValue(RunningAsyncValidatorsPropertyKey);
                else
                    SetValue(RunningAsyncValidatorsPropertyKey, value);
            }
        }

        public IRowAsyncValidators CompletedAsyncValidators
        {
            get { return (IRowAsyncValidators)GetValue(CompletedAsyncValidatorsProperty); }
            private set
            {
                if (value == null || value.Count == 0)
                    ClearValue(CompletedAsyncValidatorsPropertyKey);
                else
                    SetValue(CompletedAsyncValidatorsPropertyKey, value);
            }
        }

        public IRowAsyncValidators FaultedAsyncValidators
        {
            get { return (IRowAsyncValidators)GetValue(FaultedAsyncValidatorsProperty); }
            private set
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

        private bool AnyStatusChange(AsyncValidatorStatus status, IRowAsyncValidators statusAsyncValidators)
        {
            return AnyStatusChange(AsyncValidators, status, statusAsyncValidators);
        }

        /// <remarks>Predicts if async validtor status has been changed to avoid unnecessary object creation.</remarks>
        private static bool AnyStatusChange(IRowAsyncValidators asyncValidators, AsyncValidatorStatus status, IRowAsyncValidators statusAsyncValidators)
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
