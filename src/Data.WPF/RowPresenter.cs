using DevZest.Data.Primitives;
using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DevZest.Data.Windows
{
    public sealed class RowPresenter
    {
        internal RowPresenter(RowManager rowManager, DataRow dataRow)
        {
            Debug.Assert(rowManager != null);
            _rowManager = rowManager;
            DataRow = dataRow;
        }

        internal void Dispose()
        {
            Debug.Assert(View == null, "Row should be virtualized first before dispose.");
            _rowManager = null;
            _ordinal = -1;
            _subviewPresenters = null;
        }

        private RowManager _rowManager;
        internal RowManager RowManager
        {
            get
            {
                if (_rowManager == null)
                    throw new ObjectDisposedException(GetType().FullName);
                return _rowManager;
            }
        }

        //public DataPresenter DataPresenter
        //{
        //    get { return RowManager as DataPresenter; }
        //}

        private Template Template
        {
            get { return RowManager.Template; }
        }

        public DataRow DataRow { get; internal set; }

        public bool IsEof
        {
            get { return DataRow == null; }
        }

        private IReadOnlyList<DataPresenter> _subviewPresenters;
        public IReadOnlyList<DataPresenter> SubviewPresenters
        {
            get
            {
                if (_subviewPresenters == null)
                    _subviewPresenters = InitSubviewPresenters();
                return _subviewPresenters;
            }
        }

        private IReadOnlyList<DataPresenter> InitSubviewPresenters()
        {
            if (IsEof)
                return EmptyArray<DataPresenter>.Singleton;

            var subviewItems = Template.SubviewItems;
            if (subviewItems.Count == 0)
                return EmptyArray<DataPresenter>.Singleton;

            var result = new DataPresenter[subviewItems.Count];
            for (int i = 0; i < subviewItems.Count; i++)
                result[i] = subviewItems[i].DataPresenterConstructor(this);

            return result;
        }

        private void OnGetState(RowPresenterState rowPresenterState)
        {
            RowManager.OnGetState(this, rowPresenterState);
        }

        private void OnSetState(RowPresenterState rowPresenterState)
        {
            RowManager.OnSetState(this, rowPresenterState);
        }

        private int _ordinal;
        public int Ordinal
        {
            get
            {
                OnGetState(RowPresenterState.Ordinal);
                if (RowManager.IsHierarchical)
                    return _ordinal;
                else
                    return IsEof ? RowManager.Rows.Count - 1 : DataRow.Index;
            }
            internal set
            {
                _ordinal = value;
            }
        }

        public int HierarchicalLevel
        {
            get { return IsEof ? 0 : DataRow.Model.GetHierarchicalLevel(); }
        }

        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get
            {
                OnGetState(RowPresenterState.IsExpanded);
                return _isExpanded;
            }
        }

        public void Expand()
        {
        }

        public void Collapse()
        {
        }

        private bool _isCurrent;
        public bool IsCurrent
        {
            get
            {
                OnGetState(RowPresenterState.IsCurrent);
                return _isCurrent;
            }
            internal set
            {
                if (_isCurrent == value)
                    return;

                _isCurrent = value;
                OnSetState(RowPresenterState.IsCurrent);
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                OnGetState(RowPresenterState.IsSelected);
                return _isSelected;
            }
            set
            {
                if (_isSelected == value)
                    return;

                if (_isSelected)
                    RowManager.RemoveSelectedRow(this);

                _isSelected = value;

                if (_isSelected)
                    RowManager.AddSelectedRow(this);

                OnSetState(RowPresenterState.IsSelected);
            }
        }

        public T GetValue<T>(Column<T> column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return DataRow == null ? default(T) : column[DataRow];
        }

        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }

        public object GetValue(Column column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return DataRow == null ? GetDefault(column.DataType) : column.GetValue(DataRow);
        }

        private void SuppressViewUpdate()
        {
            Debug.Assert(DataRow != null);
            RowManager.SuppressViewUpdate(DataRow);
        }

        private void ResumeViewUpdate()
        {
            Debug.Assert(DataRow != null);
            RowManager.ResumeViewUpdate();
        }

        public void SetValue<T>(Column<T> column, T value, bool suppressViewUpdate = true)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            BeginEdit();

            if (suppressViewUpdate)
                SuppressViewUpdate();

            try
            {
                column[DataRow] = value;
            }
            finally
            {
                if (suppressViewUpdate)
                    ResumeViewUpdate();
            }
        }

        public void SetValue(Column column, object value, bool suppressUpdateTarget = true)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            BeginEdit();

            if (suppressUpdateTarget)
                SuppressViewUpdate();

            try
            {
                column.SetValue(DataRow, value);
            }
            finally
            {
                if (suppressUpdateTarget)
                    ResumeViewUpdate();
            }
        }

        public ReadOnlyCollection<ValidationMessage> ValidationMessages
        {
            get { return DataRow == null ? null : DataRow.ValidationMessages; }
        }

        public ReadOnlyCollection<ValidationMessage> MergedValidationMessages
        {
            get { return DataRow == null ? null : DataRow.MergedValidationMessages; }
        }

        private static ConditionalWeakTable<Model, RowPresenter> s_editingRows = new ConditionalWeakTable<Model, RowPresenter>();

        private RowPresenter EditingRow
        {
            get
            {
                Debug.Assert(DataRow != null);

                RowPresenter result;
                Model model = DataRow.Model;
                return s_editingRows.TryGetValue(model, out result) ? result : null;
            }
        }

        public bool IsEditing
        {
            get
            {
                OnGetState(RowPresenterState.IsEditing);
                return DataRow != null && EditingRow == this;
            }
            private set
            {
                Debug.Assert(DataRow != null);
                Debug.Assert(IsEditing != value);

                Model model = DataRow.Model;
                if (value)
                    s_editingRows.Add(model, this);
                else
                    s_editingRows.Remove(model);

                OnSetState(RowPresenterState.IsEditing);
            }
        }

        public void BeginEdit()
        {
            bool isEof = IsEof;
            if (isEof)
                RowManager.BeginEditEof();

            Debug.Assert(DataRow != null);

            var editingRow = EditingRow;
            if (editingRow == this)
                return;
            if (editingRow != null)
                editingRow.EndEdit();

            if (!isEof)
                DataRow.Save();
            IsEditing = true;
        }

        public void EndEdit()
        {
            if (!IsEditing)
                return;

            IsEditing = false;
        }

        public void CancelEdit()
        {
            if (RowManager.EditingEofRow == this)
                RowManager.CancelEditEof();
            else
                DataRow.Load();
            IsEditing = false;
        }

        internal RowView View { get; set; }
    }
}
