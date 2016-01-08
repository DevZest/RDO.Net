using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class DataRowPresenter
    {
        internal static DataRowPresenter Create(DataSetPresenter owner, DataRow dataRow)
        {
            return new DataRowPresenter(owner, dataRow);
        }

        internal static DataRowPresenter CreateEof(DataSetPresenter owner)
        {
            return new DataRowPresenter(owner, RowType.Eof);
        }

        internal static DataRowPresenter CreateEmptySet(DataSetPresenter owner)
        {
            return new DataRowPresenter(owner, RowType.EmptySet);
        }

        private DataRowPresenter(DataSetPresenter owner, DataRow dataRow)
            : this(owner, dataRow, RowType.DataRow)
        {
        }

        private DataRowPresenter(DataSetPresenter owner, RowType rowType)
            : this(owner, null, rowType)
        {
        }

        private DataRowPresenter(DataSetPresenter owner, DataRow dataRow, RowType rowType)
        {
            Debug.Assert(owner != null);
            Debug.Assert(dataRow == null || owner.Model == dataRow.Model);
            _owner = owner;
            DataRow = dataRow;
            RowType = rowType;
            Children = InitChildDataSetPresenters();
        }

        internal void Dispose()
        {
            _owner = null;
            Children = null;
            EnsureUIElementsRecycled();
        }

        private DataSetPresenter _owner;
        public DataSetPresenter Owner
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

        private static DataSetPresenter[] s_emptyChildren = new DataSetPresenter[0];
        public IReadOnlyList<DataSetPresenter> Children { get; private set; }
        private IReadOnlyList<DataSetPresenter> InitChildDataSetPresenters()
        {
            if (RowType != RowType.DataRow)
                return s_emptyChildren;

            var childEntries = Template.ChildUnits;
            if (childEntries.Count == 0)
                return s_emptyChildren;

            var result = new DataSetPresenter[childEntries.Count];
            for (int i = 0; i < childEntries.Count; i++)
                result[i] = childEntries[i].ChildPresenterConstructor(this);

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

        public void SetValue<T>(Column<T> column, T value)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            throw new NotImplementedException();
        }
    }
}
