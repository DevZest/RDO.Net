using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

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
            _owner = owner;
            Initialize(dataRow, rowType);
        }

        internal void Initialize(DataRow dataRow, RowKind rowType)
        {
            DataRow = dataRow;
            Kind = rowType;
            Children = InitChildPresenters();
        }

        internal void Dispose()
        {
            Debug.Assert(Form == null, "Row should be virtualized first before dispose.");
            _owner = null;
            Children = null;
        }

        private DataPresenter _owner;
        public DataPresenter Owner
        {
            get
            {
                if (_owner == null)
                    throw new ObjectDisposedException(GetType().FullName);

                return _owner;
            }
        }

        private GridTemplate Template
        {
            get { return Owner.Template; }
        }

        private LayoutManager LayoutManager
        {
            get { return Owner.LayoutManager; }
        }

        public DataRow DataRow { get; private set; }

        public Model Model
        {
            get { return Owner == null ? null : Owner.Model; }
        }

        public IReadOnlyList<DataPresenter> Children { get; private set; }
        private IReadOnlyList<DataPresenter> InitChildPresenters()
        {
            if (Kind != RowKind.DataRow)
                return EmptyArray<DataPresenter>.Singleton;

            var childEntries = Template.ChildItems;
            if (childEntries.Count == 0)
                return EmptyArray<DataPresenter>.Singleton;

            var result = new DataPresenter[childEntries.Count];
            for (int i = 0; i < childEntries.Count; i++)
                result[i] = childEntries[i].ChildPresenterConstructor(this);

            return result;
        }

        private void OnGetValue(RowPresenterState bindingSource)
        {
            Owner.OnGetValue(bindingSource);
        }

        private void OnUpdated(RowPresenterState bindingSource)
        {
            if (Owner.IsConsumed(bindingSource))
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
                return DataRow == null ? Owner.Count - 1 : DataRow.Index;
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
                    Owner.RemoveSelectedRow(this);

                _isSelected = value;

                if (_isSelected)
                    Owner.AddSelectedRow(this);

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
            Owner.EofToDataRow();
            return true;
        }

        private void DataRowToEof()
        {
            if (!_wasEof)
                return;

            Owner.DataRowToEof();
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

        internal RowForm Form { get; set; }
    }
}
