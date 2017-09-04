using DevZest.Data.Presenters;
using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Linq;
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

        public static readonly DependencyProperty AsyncValidatorsProperty = DependencyProperty.Register(nameof(AsyncValidators),
            typeof(IReadOnlyList<AsyncValidator>), typeof(ValidationView), new FrameworkPropertyMetadata(Array<AsyncValidator>.Empty, OnAsyncValidatorsChanged));

        private static readonly DependencyPropertyKey RunningAsyncValidatorsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(RunningAsyncValidators),
            typeof(IReadOnlyList<AsyncValidator>), typeof(ValidationView), new FrameworkPropertyMetadata(Array<AsyncValidator>.Empty));
        public static readonly DependencyProperty RunningAsyncValidatorsProperty = RunningAsyncValidatorsPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey CompletedAsyncValidatorsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CompletedAsyncValidators),
            typeof(IReadOnlyList<AsyncValidator>), typeof(ValidationView), new FrameworkPropertyMetadata(Array<AsyncValidator>.Empty));
        public static readonly DependencyProperty CompletedAsyncValidatorsProperty = CompletedAsyncValidatorsPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey FaultedAsyncValidatorsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(FaultedAsyncValidators),
            typeof(IReadOnlyList<AsyncValidator>), typeof(ValidationView), new FrameworkPropertyMetadata(Array<AsyncValidator>.Empty));
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

        public IReadOnlyList<AsyncValidator> AsyncValidators
        {
            get { return (IReadOnlyList<AsyncValidator>)GetValue(AsyncValidatorsProperty); }
            set
            {
                if (value == null || value.Count == 0)
                    ClearValue(AsyncValidatorsProperty);
                else
                    SetValue(AsyncValidatorsProperty, value);
            }
        }

        public IReadOnlyList<AsyncValidator> RunningAsyncValidators
        {
            get { return (IReadOnlyList<AsyncValidator>)GetValue(RunningAsyncValidatorsProperty); }
            private set
            {
                if (value == null || value.Count == 0)
                    ClearValue(RunningAsyncValidatorsPropertyKey);
                else
                    SetValue(RunningAsyncValidatorsPropertyKey, value);
            }
        }

        public IReadOnlyList<AsyncValidator> CompletedAsyncValidators
        {
            get { return (IReadOnlyList<AsyncValidator>)GetValue(CompletedAsyncValidatorsProperty); }
            private set
            {
                if (value == null || value.Count == 0)
                    ClearValue(CompletedAsyncValidatorsPropertyKey);
                else
                    SetValue(CompletedAsyncValidatorsPropertyKey, value);
            }
        }

        public IReadOnlyList<AsyncValidator> FaultedAsyncValidators
        {
            get { return (IReadOnlyList<AsyncValidator>)GetValue(FaultedAsyncValidatorsProperty); }
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
                RunningAsyncValidators = Where(AsyncValidators, AsyncValidatorStatus.Running);
            if (AnyStatusChange(AsyncValidatorStatus.Completed, CompletedAsyncValidators))
                CompletedAsyncValidators = Where(AsyncValidators, AsyncValidatorStatus.Completed);
            if (AnyStatusChange(AsyncValidatorStatus.Faulted, FaultedAsyncValidators))
                FaultedAsyncValidators = Where(AsyncValidators, AsyncValidatorStatus.Faulted);
        }

        private bool AnyStatusChange(AsyncValidatorStatus status, IReadOnlyList<AsyncValidator> statusAsyncValidators)
        {
            return AnyStatusChange(AsyncValidators, status, statusAsyncValidators);
        }

        /// <remarks>Predicts if async validtor status has been changed to avoid unnecessary object creation.</remarks>
        private static bool AnyStatusChange(IReadOnlyList<AsyncValidator> asyncValidators, AsyncValidatorStatus status, IReadOnlyList<AsyncValidator> statusAsyncValidators)
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

        private static IReadOnlyList<AsyncValidator> Where(IReadOnlyList<AsyncValidator> asyncValidators, AsyncValidatorStatus status)
        {
            var rowAsyncValidators = asyncValidators as IRowAsyncValidators;
            if (rowAsyncValidators != null)
                return rowAsyncValidators.Where(x => x.Status == status);

            return asyncValidators.Where(x => x.Status == status).ToArray();
        }
    }
}
