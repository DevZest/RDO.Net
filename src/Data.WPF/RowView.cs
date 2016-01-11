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
            return new RowView(owner, dataRow);
        }

        internal static RowView CreateEof(DataView owner)
        {
            return new RowView(owner, RowType.Eof);
        }

        internal static RowView CreateEmptySet(DataView owner)
        {
            return new RowView(owner, RowType.EmptySet);
        }

        private RowView(DataView owner, DataRow dataRow)
            : this(owner, dataRow, RowType.DataRow)
        {
        }

        private RowView(DataView owner, RowType rowType)
            : this(owner, null, rowType)
        {
        }

        private RowView(DataView owner, DataRow dataRow, RowType rowType)
        {
            Debug.Assert(owner != null);
            Debug.Assert(dataRow == null || owner.Model == dataRow.Model);
            _owner = owner;
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

        private void OnGetProperty(RowProperty rowProperty)
        {
            Owner.OnGetRowProperty(rowProperty);
        }

        private void OnUpdated(RowProperty rowProperty)
        {
            if (Owner.ShouldFireRowUpdatedEvent(rowProperty))
                OnUpdated();
        }

        public event EventHandler Updated;

        internal void OnUpdated()
        {
            if (_updateTargetSuppressed)
                return;

            var updated = Updated;
            if (updated != null)
                updated(this, EventArgs.Empty);
        }

        public int Index
        {
            get
            {
                OnGetProperty(RowProperty.Index);
                return DataRow == null ? Owner.Count - 1 : DataRow.Index;
            }
        }

        private bool _isCurrent;
        public bool IsCurrent
        {
            get
            {
                OnGetProperty(RowProperty.IsCurrent);
                return _isCurrent;
            }
            set
            {
                if (_isCurrent == value)
                    return;

                _isCurrent = value;
                OnUpdated(RowProperty.IsCurrent);
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                OnGetProperty(RowProperty.IsSelected);
                return _isSelected;
            }
            set
            {
                if (_isSelected == value)
                    return;

                if (_isSelected)
                    Owner._selectedRows.Remove(this);

                _isSelected = value;

                if (_isSelected)
                    Owner._selectedRows.Add(this);

                OnUpdated(RowProperty.IsSelected);
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

            var dataRow = ExpectDataRow();

            if (suppressUpdateTarget)
                EnterSuppressUpdateTarget();

            try
            {
                column[dataRow] = value;
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

            var dataRow = ExpectDataRow();

            if (suppressUpdateTarget)
                EnterSuppressUpdateTarget();

            try
            {
                column.SetValue(dataRow, value);
            }
            finally
            {
                if (suppressUpdateTarget)
                    ExitSuppressUpdateTarget();
            }
        }

        private DataRow ExpectDataRow()
        {
            var dataRow = DataRow;
            if (dataRow == null)
                throw new InvalidOperationException(Strings.RowView_ExpectDataRow);
            return dataRow;
        }

        public ReadOnlyCollection<ValidationMessage> ValidationMessages
        {
            get { return ExpectDataRow().ValidationMessages; }
        }

        public ReadOnlyCollection<ValidationMessage> MergedValidationMessages
        {
            get { return ExpectDataRow().MergedValidationMessages; }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get
            {
                OnGetProperty(RowProperty.IsEditing);
                return _isEditing;
            }
            private set
            {
                if (_isEditing == value)
                    return;

                _isEditing = value;
                OnUpdated(RowProperty.IsEditing);
            }
        }

        public void BeginEdit()
        {
        }

        public void EndEdit()
        {
        }

        public void CancelEdit()
        {
        }
    }
}
