using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    internal abstract class InputManager : ElementManager
    {
        protected InputManager(Template template, DataSet dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy, bool emptyContainerViewList)
            : base(template, dataSet, where, orderBy, emptyContainerViewList)
        {
            ScalarValidation = new ScalarValidation(this);
            RowValidation = new RowValidation(this);
        }

        public ScalarValidation ScalarValidation { get; private set; }

        internal virtual IScalarValidationMessages PerformValidateScalars()
        {
            return DataPresenter == null ? ScalarValidationMessages.Empty : DataPresenter.ValidateScalars();
        }

        public RowValidation RowValidation { get; private set; }

        protected override void Reload()
        {
            base.Reload();
            RowValidation.OnReloaded();
        }

        protected sealed override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            base.OnCurrentRowChanged(oldValue);
            RowValidation.OnCurrentRowChanged(oldValue);
        }

        protected override void DisposeRow(RowPresenter rowPresenter)
        {
            base.DisposeRow(rowPresenter);
            RowValidation.OnRowDisposed(rowPresenter);
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

        public bool HasVisibleError
        {
            get
            {
                if (ScalarValidation.FlushErrors.Count > 0 || RowValidation.FlushErrors.Count > 0)
                    return true;

                for (int i = 0; i < ScalarValidation.ValidationErrors.Count; i++)
                {
                    var error = ScalarValidation.ValidationErrors[i];
                    if (ScalarValidation.IsVisible(error.Source))
                        return true;
                }

                foreach (var keyValuePair in RowValidation.ValidationErrors)
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
