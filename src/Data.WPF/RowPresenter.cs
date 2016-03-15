using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    public sealed class RowPresenter
    {
        internal static RowPresenter Create(DataPresenter owner, DataRow dataRow)
        {
            return new RowPresenter(owner, dataRow, RowKind.DataRow);
        }

        internal static RowPresenter CreateEof(DataPresenter owner)
        {
            return new RowPresenter(owner, null, RowKind.Eof);
        }

        internal static RowPresenter CreateEmptySet(DataPresenter owner)
        {
            return new RowPresenter(owner, null, RowKind.EmptySet);
        }

        private RowPresenter(DataPresenter owner, DataRow dataRow, RowKind rowType)
        {
            Debug.Assert(owner != null);
            Debug.Assert(dataRow == null || owner.Model == dataRow.Model);
            _dataPresenter = owner;
            Initialize(dataRow, rowType);
        }

        internal void Initialize(DataRow dataRow, RowKind rowType)
        {
            DataRow = dataRow;
            Kind = rowType;
            _subviewPresenters = InitSubviewPresenters();
        }

        internal void Dispose()
        {
            Debug.Assert(View == null, "Row should be virtualized first before dispose.");
            _dataPresenter = null;
            _subviewPresenters = null;
        }

        private DataPresenter _dataPresenter;
        public DataPresenter DataPresenter
        {
            get
            {
                if (_dataPresenter == null)
                    throw new ObjectDisposedException(GetType().FullName);

                return _dataPresenter;
            }
        }

        private Template Template
        {
            get { return DataPresenter.Template; }
        }

        public DataRow DataRow { get; private set; }

        public Model Model
        {
            get { return DataPresenter == null ? null : DataPresenter.Model; }
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
            if (Kind != RowKind.DataRow)
                return EmptyArray<DataPresenter>.Singleton;

            var subviewItems = Template.SubviewItems;
            if (subviewItems.Count == 0)
                return EmptyArray<DataPresenter>.Singleton;

            var result = new DataPresenter[subviewItems.Count];
            for (int i = 0; i < subviewItems.Count; i++)
                result[i] = subviewItems[i].DataPresenterConstructor(this);

            return result;
        }

        private void OnGetValue(RowPresenterState bindingSource)
        {
            DataPresenter.OnGetValue(bindingSource);
        }

        private void OnUpdated(RowPresenterState bindingSource)
        {
            if (DataPresenter.IsConsumed(bindingSource))
                OnBindingsReset();
        }

        public event EventHandler BindingsReset;

        internal void OnBindingsReset()
        {
            if (_updateTargetSuppressed)
                return;

            var bindingsReset = BindingsReset;
            if (bindingsReset != null)
                bindingsReset(this, EventArgs.Empty);
        }

        public int Index
        {
            get
            {
                OnGetValue(RowPresenterState.Index);
                return DataRow == null ? DataPresenter.Count - 1 : DataRow.Index;
            }
        }

        private bool _isCurrent;
        public bool IsCurrent
        {
            get
            {
                OnGetValue(RowPresenterState.IsCurrent);
                return _isCurrent;
            }
            internal set
            {
                if (_isCurrent == value)
                    return;

                _isCurrent = value;
                OnUpdated(RowPresenterState.IsCurrent);
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                OnGetValue(RowPresenterState.IsSelected);
                return _isSelected;
            }
            set
            {
                if (_isSelected == value)
                    return;

                if (_isSelected)
                    DataPresenter.RemoveSelectedRow(this);

                _isSelected = value;

                if (_isSelected)
                    DataPresenter.AddSelectedRow(this);

                OnUpdated(RowPresenterState.IsSelected);
            }
        }

        public RowKind Kind { get; private set; }

        public bool IsFocused { get; internal set; }

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

        bool _updateTargetSuppressed;
        private void EnterSuppressUpdateTarget()
        {
            Debug.Assert(!_updateTargetSuppressed);
            _updateTargetSuppressed = true;
        }

        private void ExitSuppressUpdateTarget()
        {
            Debug.Assert(_updateTargetSuppressed);
            _updateTargetSuppressed = false;
        }
      

        public void SetValue<T>(Column<T> column, T value, bool suppressUpdateTarget = true)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            BeginEdit();

            if (suppressUpdateTarget)
                EnterSuppressUpdateTarget();

            try
            {
                column[DataRow] = value;
            }
            finally
            {
                if (suppressUpdateTarget)
                    ExitSuppressUpdateTarget();
            }
        }

        public void SetValue(Column column, object value, bool suppressUpdateTarget = true)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            BeginEdit();

            if (suppressUpdateTarget)
                EnterSuppressUpdateTarget();

            try
            {
                column.SetValue(DataRow, value);
            }
            finally
            {
                if (suppressUpdateTarget)
                    ExitSuppressUpdateTarget();
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

        private bool _isEditing;
        public bool IsEditing
        {
            get
            {
                OnGetValue(RowPresenterState.IsEditing);
                return _isEditing;
            }
            private set
            {
                if (_isEditing == value)
                    return;

                if (_isEditing)
                {
                    Debug.Assert(Model.GetEditingRow() == this);
                    Model.SetEditingRow(null);
                }

                _isEditing = value;

                if (_isEditing)
                {
                    Debug.Assert(Model.GetEditingRow() == null);
                    Model.SetEditingRow(this);
                }

                OnUpdated(RowPresenterState.IsEditing);
            }
        }

        private bool _wasEof;

        private bool EofToDataRow()
        {
            if (Kind == RowKind.DataRow)
                return false;

            Debug.Assert(Kind == RowKind.Eof);
            DataPresenter.EofToDataRow();
            return true;
        }

        private void DataRowToEof()
        {
            if (!_wasEof)
                return;

            DataPresenter.DataRowToEof();
        }

        public void BeginEdit()
        {
            if (IsEditing)
                return;

            if (Kind == RowKind.EmptySet)
                throw new InvalidOperationException(Strings.RowPresenter_BeginEdit_EmptySet);

            _wasEof = EofToDataRow();
            Debug.Assert(DataRow != null);

            var editingRow = Model.GetEditingRow();
            if (editingRow != null)
                editingRow.EndEdit();

            if (!_wasEof)
                DataRow.Save();
            IsEditing = true;
        }

        public void EndEdit()
        {
            IsEditing = false;
        }

        public void CancelEdit()
        {
            if (_wasEof)
                DataRowToEof();
            else
                DataRow.Load();
            IsEditing = false;
        }

        internal RowView View { get; set; }
    }
}
