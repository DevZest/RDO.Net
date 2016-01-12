using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class RowView
    {
        internal static RowView Create(DataView owner, DataRow dataRow)
        {
            return new RowView(owner, dataRow, RowType.DataRow);
        }

        internal static RowView CreateEof(DataView owner)
        {
            return new RowView(owner, null, RowType.Eof);
        }

        internal static RowView CreateEmptySet(DataView owner)
        {
            return new RowView(owner, null, RowType.EmptySet);
        }

        private RowView(DataView owner, DataRow dataRow, RowType rowType)
        {
            Debug.Assert(owner != null);
            Debug.Assert(dataRow == null || owner.Model == dataRow.Model);
            _owner = owner;
            Initialize(dataRow, rowType);
        }

        internal void Initialize(DataRow dataRow, RowType rowType)
        {
            DataRow = dataRow;
            RowType = rowType;
            Children = InitChildViews();
        }

        internal void Dispose()
        {
            _owner = null;
            Children = null;
            EnsureUIElementsRecycled();
        }

        private DataView _owner;
        public DataView Owner
        {
            get
            {
                if (_owner == null)
                    throw new ObjectDisposedException(GetType().FullName);

                return _owner;
            }
        }

        public GridTemplate Template
        {
            get { return Owner == null ? null : Owner.Template; }
        }

        public DataRow DataRow { get; private set; }

        public Model Model
        {
            get { return Owner == null ? null : Owner.Model; }
        }

        private static DataView[] s_emptyChildren = new DataView[0];
        public IReadOnlyList<DataView> Children { get; private set; }
        private IReadOnlyList<DataView> InitChildViews()
        {
            if (RowType != RowType.DataRow)
                return s_emptyChildren;

            var childEntries = Template.ChildUnits;
            if (childEntries.Count == 0)
                return s_emptyChildren;

            var result = new DataView[childEntries.Count];
            for (int i = 0; i < childEntries.Count; i++)
                result[i] = childEntries[i].ChildViewConstructor(this);

            return result;
        }

        private void OnGetValue(RowViewBindingSource bindingSource)
        {
            Owner.OnGetValue(bindingSource);
        }

        private void OnUpdated(RowViewBindingSource bindingSource)
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
                OnGetValue(RowViewBindingSource.Index);
                return DataRow == null ? Owner.Count - 1 : DataRow.Index;
            }
        }

        private bool _isCurrent;
        public bool IsCurrent
        {
            get
            {
                OnGetValue(RowViewBindingSource.IsCurrent);
                return _isCurrent;
            }
            set
            {
                if (_isCurrent == value)
                    return;

                _isCurrent = value;
                OnUpdated(RowViewBindingSource.IsCurrent);
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                OnGetValue(RowViewBindingSource.IsSelected);
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

                OnUpdated(RowViewBindingSource.IsSelected);
            }
        }

        public RowType RowType { get; private set; }

        public bool IsFocused { get; internal set; }

        private static UIElement[] s_emptyUIElements = new UIElement[0];
        private UIElement[] _uiElements = null;

        private int UIElementsCount
        {
            get { return Template.ListUnits.Count; }
        }

        private void EnsureUIElementsRecycled()
        {
            if (_uiElements == null)
                return;

            for (int i = 0; i < _uiElements.Length; i++)
            {
                var uiElement = _uiElements[i];
                if (uiElement == null)
                    continue;

                Template.ListUnits[i].Recycle(uiElement);
            }
            _uiElements = null;
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
                OnGetValue(RowViewBindingSource.IsEditing);
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

                OnUpdated(RowViewBindingSource.IsEditing);
            }
        }

        private bool _wasEof;

        private bool EofToDataRow()
        {
            if (RowType == RowType.DataRow)
                return false;

            Debug.Assert(RowType == RowType.Eof);
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

            if (RowType == RowType.EmptySet)
                throw new InvalidOperationException(Strings.RowView_BeginEdit_EmptySet);

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
    }
}
