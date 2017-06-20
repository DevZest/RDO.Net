using System.Collections.Generic;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows.Controls;
using DevZest.Data;
using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public abstract partial class DataPresenter
    {
        public event EventHandler ViewRefreshing = delegate { };
        public event EventHandler ViewRefreshed = delegate { };

        protected internal virtual void OnViewRefreshing()
        {
            ViewRefreshing(this, EventArgs.Empty);
        }

        protected internal virtual void OnViewRefreshed()
        {
            ViewRefreshed(this, EventArgs.Empty);
        }

        public DataView View { get; private set; }

        public virtual void DetachView()
        {
            if (View == null)
                return;
            View.CleanupCommandEntries();
            View.DataPresenter = null;
            View = null;
        }

        internal void AttachView(DataView value)
        {
            if (View != null)
                DetachView();

            Debug.Assert(View == null && value != null);
            View = value;
            View.DataPresenter = this;
        }

        internal abstract LayoutManager LayoutManager { get; }

        public Template Template
        {
            get { return LayoutManager == null ? null : LayoutManager.Template; }
        }

        public Orientation? LayoutOrientation
        {
            get { return Template.Orientation; }
        }

        public DataSet DataSet
        {
            get { return LayoutManager == null ? null : LayoutManager.DataSet; }
        }

        private LayoutManager RequireLayoutManager()
        {
            if (LayoutManager == null)
                throw new InvalidOperationException(Strings.DataPresenter_NullDataSet);
            return LayoutManager;
        }

        public Predicate<DataRow> Where
        {
            get { return LayoutManager == null ? null : LayoutManager.Where; }
            set { Apply(value, OrderBy); }
        }
        
        public IComparer<DataRow> OrderBy
        {
            get { return LayoutManager == null ? null : LayoutManager.OrderBy; }
            set { Apply(Where, value); }
        }

        public void Apply(Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            RequireLayoutManager().Apply(where, orderBy);
        }

        public IReadOnlyList<RowPresenter> Rows
        {
            get { return LayoutManager == null ? null : LayoutManager.Rows; }
        }

        public bool CanChangeCurrentRow
        {
            get { return LayoutManager == null ? true : LayoutManager.CanChangeCurrentRow; }
        }

        public RowPresenter CurrentRow
        {
            get { return LayoutManager == null ? null : LayoutManager.CurrentRow; }
            set
            {
                VerifyRowPresenter(value, nameof(value));
                RequireLayoutManager().CurrentRow = value;
            }
        }

        public ContainerView CurrentContainerView
        {
            get { return RequireLayoutManager().CurrentContainerView; }
        }

        public int FlowCount
        {
            get { return LayoutManager == null ? 1 : LayoutManager.FlowCount; }
        }

        private void VerifyRowPresenter(RowPresenter value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);

            if (value.DataPresenter != this)
                throw new ArgumentException(Strings.DataPresenter_InvalidRowPresenter, paramName);
        }

        public void Select(RowPresenter rowPresenter, SelectionMode selectionMode, bool ensureVisible = true)
        {
            VerifyRowPresenter(rowPresenter, nameof(rowPresenter));
            var oldCurrentRow = CurrentRow;
            CurrentRow = rowPresenter;
            RequireLayoutManager().Select(rowPresenter, selectionMode, oldCurrentRow);
            if (ensureVisible && Scrollable != null)
                Scrollable.EnsureCurrentRowVisible();
        }

        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return LayoutManager == null ? null : LayoutManager.SelectedRows; }
        }

        public bool IsEditing
        {
            get { return LayoutManager == null ? false : LayoutManager.IsEditing; }
        }

        public RowPresenter EditingRow
        {
            get { return CurrentRow != null && IsEditing ? CurrentRow : null; }
        }

        public bool IsInserting
        {
            get { return IsEditing && LayoutManager.CurrentRow == LayoutManager.VirtualRow; }
        }

        public RowPresenter InsertingRow
        {
            get { return IsInserting ? CurrentRow : null; }
        }

        public bool CanInsert
        {
            get { return !IsEditing && RequireLayoutManager().DataSet.EditingRow == null; }
        }

        public void BeginInsertBefore(RowPresenter row = null)
        {
            VerifyInsert(row);
            RequireLayoutManager().BeginInsertBefore(null, row);
        }

        public void BeginInsertAfter(RowPresenter row = null)
        {
            VerifyInsert(row);
            RequireLayoutManager().BeginInsertAfter(null, row);
        }

        private void VerifyInsert(RowPresenter row)
        {
            if (!CanInsert)
                throw new InvalidOperationException(Strings.DataPresenter_VerifyCanInsert);
            if (row != null && row.DataPresenter != this)
                throw new ArgumentException(Strings.DataPresenter_InvalidRowPresenter, nameof(row));
        }

        public void InvalidateView()
        {
            RequireLayoutManager().InvalidateView();
        }

        public void FlushScalars()
        {
            RequireLayoutManager().FlushScalars();
        }

        public void FlushCurrentRow()
        {
            RequireLayoutManager().FlushCurrentRow();
        }

        public void Validate()
        {
            RequireLayoutManager().Validate();
        }

        public IReadOnlyList<ViewInputError> ScalarInputErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.ScalarInputErrors; }
        }

        public IReadOnlyList<ViewInputError> RowInputErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.RowInputErrors; }
        }

        public IValidationDictionary ValidationErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.Errors; }
        }

        public IValidationDictionary ValidationWarnings
        {
            get { return LayoutManager == null ? null : LayoutManager.Warnings; }
        }

        public IValidationMessageGroup CurrentRowErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.CurrentRowErrors; }
        }

        public IValidationMessageGroup CurrentRowWarnings
        {
            get { return LayoutManager == null ? null : LayoutManager.CurrentRowWarnings; }
        }

        public ValidationProgress ValidationProgress
        {
            get { return LayoutManager == null ? null : LayoutManager.Progress; }
        }

        public IAsyncValidatorGroup AsyncValidators
        {
            get { return LayoutManager == null ? null : LayoutManager.AllRowsAsyncValidators; }
        }

        public IValidationDictionary ValidationResult
        {
            get { return LayoutManager == null ? null : LayoutManager.ValidationResult; }
        }

        public void Show(IValidationResult validationResult)
        {
            if (validationResult == null)
                throw new ArgumentNullException(nameof(validationResult));
            RequireLayoutManager().Show(validationResult);
        }

        public IScrollable Scrollable
        {
            get { return LayoutManager as ScrollableManager; }
        }

        internal protected virtual void ToggleExpandState(RowPresenter rowPresenter)
        {
            rowPresenter.InternalToggleExpandState();
        }

        internal protected virtual bool HasChildren(RowPresenter rowPresenter)
        {
            return rowPresenter.InternalHasChildren;
        }

        private Dictionary<Type, IDataPresenterService> _services;

        public IDataPresenterService this[Type type]
        {
            get
            {
                if (_services == null || type == null)
                    return null;
                IDataPresenterService result;
                return _services.TryGetValue(type, out result) ? result : null;
            }
            set
            {
                if (type == null)
                    throw new ArgumentNullException(nameof(type));
                IDataPresenterService oldValue;
                if (_services == null)
                    oldValue = null;
                else
                    _services.TryGetValue(type, out oldValue);

                if (value == null)
                {
                    if (_services != null && _services.ContainsKey(type))
                        _services.Remove(type);
                }
                else
                {
                    if (_services == null)
                        _services = new Dictionary<Type, IDataPresenterService>();
                    _services[type] = value;
                }

                if (oldValue != null)
                    oldValue.DataPresenter = null;
                if (value != null)
                    value.DataPresenter = this;
            }
        }

        public T GetService<T>(Type type)
            where T : IDataPresenterService
        {
            return (T)this[type];
        }
    }
}
