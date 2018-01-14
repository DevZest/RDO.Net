using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters.Primitives
{
    internal abstract class InputManager : ElementManager
    {
        protected InputManager(Template template, DataSet dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy, bool emptyContainerViewList)
            : base(template, dataSet, where, orderBy, emptyContainerViewList)
        {
        }

        private ScalarValidation _scalarValidation;
        public ScalarValidation ScalarValidation
        {
            get { return _scalarValidation ?? (_scalarValidation = new ScalarValidation(this)); }
        }

        internal virtual IScalarValidationErrors PerformValidateScalars()
        {
            return base.DataPresenter == null ? Presenters.ScalarValidationErrors.Empty : base.DataPresenter.ValidateScalars();
        }

        private RowValidation _rowValidation;
        public RowValidation RowValidation
        {
            get { return _rowValidation ?? (_rowValidation = new RowValidation(this)); }
        }

        protected override void Reload()
        {
            base.Reload();
            _rowValidation?.OnReloaded();
            DataPresenter?.OnRowsLoaded(true);
        }

        protected sealed override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            base.OnCurrentRowChanged(oldValue);
            _rowValidation?.OnCurrentRowChanged(oldValue);
            DataPresenter?.OnCurrentRowChanged(oldValue);
        }

        protected override void DisposeRow(RowPresenter rowPresenter)
        {
            base.DisposeRow(rowPresenter);
            _rowValidation?.OnRowDisposed(rowPresenter);
        }

        internal override void BeginEdit()
        {
            base.BeginEdit();
            RowValidation.EnterEdit();
        }

        internal sealed override void CancelEdit()
        {
            var cancelEdit = DataPresenter == null ? true : DataPresenter.QueryCancelEdit();
            if (cancelEdit)
            {
                RowValidation.CancelEdit();
                base.CancelEdit();
            }
        }

        internal sealed override bool EndEdit()
        {
            var endEdit = DataPresenter == null ? QueryEndEdit() : DataPresenter.QueryEndEdit();
            if (!endEdit)
                return false;

            RowValidation.ExitEdit();
            return base.EndEdit();
        }

        internal bool QueryEndEdit()
        {
            RowValidation.ValidateCurrentRow();
            var hasError = RowValidation.CurrentRowErrors.Count > 0;
            if (hasError)
            {
                FocusToInputError(RowValidation.CurrentRowErrors);
                return false;
            }
            return true;
        }

        private void FocusToInputError(IDataValidationErrors errors)
        {
            for (int i = 0; i < errors.Count; i++)
            {
                var error = errors[i];
                if (FocusToInputError(error, CurrentRow))
                    return;
            }
        }

        internal bool FocusToInputError(DataValidationError error, RowPresenter row)
        {
            foreach (var rowBinding in Template.RowBindings)
            {
                var rowInput = rowBinding.RowInput;
                if (rowInput == null)
                    continue;

                if (error.Source.IsSupersetOf(rowInput.Target))
                {
                    if (Focus(rowBinding, row))
                        return true;
                }
            }
            return false;
        }

        private bool Focus(RowBinding rowBinding, RowPresenter row)
        {
            if (rowBinding[row] == null)
            {
                if (IsEditing)
                    CurrentRow.EndEdit();
                CurrentRow = row;
            }

            var element = rowBinding[row];
            Debug.Assert(element != null);
            return element.Focus();
        }

        private IReadOnlyList<FlushError> ScalarFlushErrors
        {
            get { return _scalarValidation == null ? Array<FlushError>.Empty : _scalarValidation.FlushErrors; }
        }

        private IReadOnlyList<FlushError> RowFlushErrors
        {
            get { return _rowValidation == null ? Array<FlushError>.Empty : _rowValidation.FlushErrors; }
        }

        private IReadOnlyList<ScalarValidationError> ScalarValidationErrors
        {
            get { return _scalarValidation == null ? Array<ScalarValidationError>.Empty : _scalarValidation.ValidationErrors; }
        }

        private IReadOnlyDictionary<RowPresenter, IDataValidationErrors> RowValidationErrors
        {
            get { return _rowValidation == null ? RowValidationResults.Empty : _rowValidation.ValidationErrors; }
        }

        public bool HasVisibleError
        {
            get
            {
                if (ScalarFlushErrors.Count > 0 || RowFlushErrors.Count > 0)
                    return true;

                var scalarValidationErrors = ScalarValidationErrors;
                for (int i = 0; i < scalarValidationErrors.Count; i++)
                {
                    var error = scalarValidationErrors[i];
                    if (ScalarValidation.IsVisible(error.Source))
                        return true;
                }

                var rowValidationErrors = RowValidationErrors;
                foreach (var keyValuePair in rowValidationErrors)
                {
                    var rowPresenter = keyValuePair.Key;
                    var messages = keyValuePair.Value;

                    for (int i = 0; i < messages.Count; i++)
                    {
                        var message = messages[i];
                        if (RowValidation.IsVisible(rowPresenter, message.Source))
                            return true;
                    }
                }

                return false;
            }
        }

        public void FlushScalars()
        {
            var scalarBindings = Template.ScalarBindings;
            foreach (var scalarBinding in scalarBindings)
            {
                for (int i = 0; i < scalarBinding.FlowRepeatCount; i++)
                {
                    var element = scalarBinding[i];
                    scalarBinding.FlushInput(element);
                }
            }
        }

        public void FlushCurrentRow()
        {
            if (CurrentRow != null && CurrentRow.View != null)
                CurrentRow.View.Flush();
        }
    }
}
