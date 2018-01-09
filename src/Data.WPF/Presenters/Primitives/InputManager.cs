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

        internal virtual IScalarValidationMessages PerformValidateScalars()
        {
            return DataPresenter == null ? ScalarValidationMessages.Empty : DataPresenter.ValidateScalars();
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

        private void FocusToInputError(IColumnValidationMessages errors)
        {
            for (int i = 0; i < errors.Count; i++)
            {
                var error = errors[i];
                if (FocusToInputError(error, CurrentRow))
                    return;
            }
        }

        internal bool FocusToInputError(ColumnValidationMessage error, RowPresenter row)
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

        private IReadOnlyList<FlushErrorMessage> ScalarFlushErrors
        {
            get { return _scalarValidation == null ? Array<FlushErrorMessage>.Empty : _scalarValidation.FlushErrors; }
        }

        private IReadOnlyList<FlushErrorMessage> RowFlushErrors
        {
            get { return _rowValidation == null ? Array<FlushErrorMessage>.Empty : _rowValidation.FlushErrors; }
        }

        private IReadOnlyList<ScalarValidationMessage> ScalarValidationErrors
        {
            get { return _scalarValidation == null ? Array<ScalarValidationMessage>.Empty : _scalarValidation.ValidationErrors; }
        }

        private IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> RowValidationErrors
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
    }
}
