using DevZest.Data.Windows.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ValidationManager : ElementManager
    {
        private sealed class Validity : INotifyDataErrorInfo
        {
            private static DataErrorsChangedEventArgs SingletonEventArgs = new DataErrorsChangedEventArgs(string.Empty);

            public Validity(IReadOnlyList<ValidationMessage<Column>> validationMessages)
            {
                Debug.Assert(validationMessages != null);
                _validationMessages = validationMessages;
            }

            private IReadOnlyList<ValidationMessage<Column>> _validationMessages;
            public IReadOnlyList<ValidationMessage<Column>> ValidationMessages
            {
                get { return _validationMessages; }
                set
                {
                    _validationMessages = value;
                    OnErrorsChanged();
                }
            }

            public bool HasErrors
            {
                get { return _validationMessages.Count > 0; }
            }

            public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

            private void OnErrorsChanged()
            {
                var errorsChanged = ErrorsChanged;
                if (errorsChanged != null)
                    errorsChanged(this, SingletonEventArgs);
            }

            public IEnumerable GetErrors(string propertyName)
            {
                return propertyName == null ? null : _validationMessages;
            }
        }

        protected ValidationManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyBlockViewList)
            : base(template, dataSet, where, orderBy, emptyBlockViewList)
        {
        }

        bool _progressiveModeBypassed;
        private RowValidationResult _errorResult;
        private RowValidationResult _warningResult;
        private HashSet<RowPresenter> _progress;
        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> _rowErrors = new Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>>();
        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> _rowWarnings = new Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>>();

        public IReadOnlyList<ValidationMessage> GetErrors(RowPresenter rowPresenter)
        {
            return _rowErrors.GetValues(rowPresenter);
        }

        public IReadOnlyList<ValidationMessage> GetWarnings(RowPresenter rowPresenter)
        {
            return _rowWarnings.GetValues(rowPresenter);
        }

        public bool HasPreValidatorError
        {
            get
            {
                foreach (var rowBinding in Template.RowBindings)
                {
                    if (rowBinding.HasPreValidatorError)
                        return true;
                }
                return false;
            }
        }

        public bool HasValidationError
        {
            get
            {
                if (!_progressiveModeBypassed)
                    Validate(true);
                return !_errorResult.IsEmpty;
            }
        }

        public bool HasValidationWarning
        {
            get
            {
                if (!_progressiveModeBypassed)
                    Validate(true);
                return !_warningResult.IsEmpty;
            }
        }

        private ValidationMode ValidationMode
        {
            get { return Template.ValidationMode; }
        }

        private ValidationScope ValidationScope
        {
            get { return Template.ValidationScope; }
        }

        public void Validate()
        {
            Validate(true);
        }

        internal void Validate(bool bypassProgressiveMode)
        {
            if (bypassProgressiveMode && !_progressiveModeBypassed)
                BypassProgressiveMode();

            SetValidationResult(Validate(ValidationSeverity.Error, Template.MaxValidationErrors), 
                Validate(ValidationSeverity.Warning, Template.MaxValidationWarnings));
        }

        private void BypassProgressiveMode()
        {
            foreach (var rowBinding in Template.RowBindings)
                rowBinding.BypassProgressiveValidationMode();
            _progressiveModeBypassed = true;
        }

        private void SetValidationResult(RowValidationResult errors, RowValidationResult warnings)
        {
            _errorResult = errors;
            _warningResult = warnings;
            foreach (var rowBinding in Template.RowBindings)
                rowBinding.SetValidationResult(_errorResult, _warningResult);
        }

        private RowValidationResult Validate(ValidationSeverity severity, int maxEntries)
        {
            List<RowValidationEntry> result = null;
            
            if (CurrentRow != null)
            {
                var messages = CurrentRow.DataRow.Validate(severity);
                if (messages.Count > 0)
                    result = result.AddItem(new RowValidationEntry(CurrentRow, messages));
            }

            if (ValidationScope == ValidationScope.AllRows)
            {
                foreach (var row in Rows)
                {
                    if (row == CurrentRow)
                        continue;

                    var messages = row.DataRow.Validate(severity);
                    if (messages.Count > 0)
                        result = result.AddItem(new RowValidationEntry(row, messages));

                    if (result.GetCount() == maxEntries)
                        break;
                }
            }

            return RowValidationResult.New(result);
        }

        protected override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            base.OnCurrentRowChanged(oldValue);
            foreach (var rowBinding in Template.RowBindings)
                rowBinding.OnCurrentRowChanged();
            if (ValidationMode == ValidationMode.Progressive)
                _progressiveModeBypassed = false;
        }

        protected override void DisposeRow(RowPresenter row)
        {
            base.DisposeRow(row);
            foreach (var rowBinding in Template.RowBindings)
                rowBinding.OnRowDisposed(row);
        }

        protected override void Reload()
        {
            base.Reload();
            Reset(true);
        }

        private void Reset(bool dataReloaded)
        {
            _progressiveModeBypassed = false;
            _errorResult = RowValidationResult.Empty;
            _warningResult = RowValidationResult.Empty;
            foreach (var rowBinding in Template.RowBindings)
                rowBinding.ResetInput(dataReloaded);
        }

        public void SetValidationResult(ValidationResult validationResult)
        {

        }
    }
}
