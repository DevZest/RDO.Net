using DevZest.Data.Primitives;
using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class RowPresenter
    {
        internal RowPresenter(RowManager rowManager, DataRow dataRow)
        {
            Debug.Assert(rowManager != null);
            _rowManager = rowManager;
            DataRow = dataRow;
            _rowItemsId = -1;
        }

        internal void Dispose()
        {
            Debug.Assert(View == null, "Row should be virtualized first before dispose.");
            _rowManager = null;
            _ordinal = -1;
            _rowItemsId = -1;
        }

        private void VerifyDisposed()
        {
            if (_rowManager == null)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private RowManager _rowManager;
        internal RowManager RowManager
        {
            get
            {
                VerifyDisposed();
                return _rowManager;
            }
        }

        internal ElementManager ElementManager
        {
            get { return RowManager as ElementManager; }
        }

        internal LayoutManager LayoutManager
        {
            get { return RowManager as LayoutManager; }
        }

        private bool IsHierarchical
        {
            get { return !IsEof && RowManager.IsHierarchical; }
        }

        public DataPresenter DataPresenter
        {
            get
            {
                var layoutManager = LayoutManager;
                return layoutManager == null ? null : layoutManager.DataPresenter;
            }
        }

        internal Template Template
        {
            get { return RowManager.Template; }
        }

        internal DataRow DataRow { get; set; }

        public bool IsEof
        {
            get { return DataRow == null; }
        }

        public RowPresenter HierarchicalParent
        {
            get
            {
                if (!IsHierarchical)
                    return null;

                var parentDataRow = DataRow.ParentDataRow;
                return parentDataRow == null ? null : RowManager.RowMappings_GetRow(parentDataRow);
            }
        }

        public int HierarchicalChildrenCount
        {
            get
            {
                if (!IsHierarchical)
                    return 0;

                OnGetState(RowPresenterState.HierarchicalChildren);
                return ChildDataSet.Count;
            }
        }

        private DataSet ChildDataSet
        {
            get
            {
                Debug.Assert(IsHierarchical);
                return DataRow[Template.HierarchicalModelOrdinal];
            }
        }

        public RowPresenter GetHierarchicalChild(int index)
        {
            if (index < 0 || index >= HierarchicalChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            OnGetState(RowPresenterState.HierarchicalChildren);
            return RowManager.RowMappings_GetRow(ChildDataSet[index]);
        }

        public DataPresenter this[SubviewItem subviewItem]
        {
            get
            {
                if (subviewItem == null)
                    throw new ArgumentNullException(nameof(subviewItem));
                if (subviewItem.Template != Template)
                    throw new ArgumentException(Strings.RowPresenter_InvalidSubviewItem);
                return subviewItem[this];
            }
        }

        private void OnGetState(RowPresenterState rowPresenterState)
        {
            RowManager.OnGetState(this, rowPresenterState);
        }

        private void OnSetState(RowPresenterState rowPresenterState)
        {
            RowManager.OnSetState(this, rowPresenterState);
        }

        private int _ordinal = -1;
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
            get { return IsHierarchical ? DataRow.Model.GetHierarchicalLevel() : 0; }
        }

        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get
            {
                OnGetState(RowPresenterState.IsExpanded);
                return _isExpanded;
            }
            private set
            {
                _isExpanded = value;
                OnSetState(RowPresenterState.IsExpanded);
            }
        }

        public void Expand()
        {
            VerifyHierarchical();

            if (IsExpanded)
                return;

            RowManager.Expand(this);
            IsExpanded = true;
        }

        public void Collapse()
        {
            VerifyHierarchical();

            if (!IsExpanded)
                return;

            RowManager.Collapse(this);
            IsExpanded = false;
        }

        private void VerifyHierarchical()
        {
            if (!IsHierarchical)
                throw new InvalidOperationException(Strings.RowPresenter_VerifyHierarchical);
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
                if (_rowManager !=null)  // RowPresenter can be disposed upon here, check to avoid ObjectDisposedException
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
            VerifyColumn(column, nameof(column));

            if (HierarchicalLevel > 0)
                column = (Column<T>)DataRow.Model.GetColumns()[column.Ordinal];

            return DataRow == null ? default(T) : column[DataRow];
        }

        private void VerifyColumn(Column column, string paramName)
        {
            if (column == null)
                throw new ArgumentNullException(paramName);

            if (column.GetParentModel() != RowManager.DataSet.Model)
                throw new ArgumentException(Strings.RowPresenter_VerifyColumn, paramName);
        }

        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }

        public object this[Column column]
        {
            get
            {
                VerifyColumn(column, nameof(column));

                if (HierarchicalLevel > 0)
                    column = DataRow.Model.GetColumns()[column.Ordinal];
                return DataRow == null ? GetDefault(column.DataType) : column.GetValue(DataRow);
            }
            set
            {
                VerifyColumn(column, nameof(column));

                if (HierarchicalLevel > 0)
                    column = DataRow.Model.GetColumns()[column.Ordinal];

                BeginEdit();
                SuppressViewUpdate();

                try
                {
                    column.SetValue(DataRow, value);
                }
                finally
                {
                    ResumeViewUpdate();
                }
            }
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

        public void SetValue<T>(Column<T> column, T value)
        {
            VerifyColumn(column, nameof(column));

            if (HierarchicalLevel > 0)
                column = (Column<T>)DataRow.Model.GetColumns()[column.Ordinal];

            BeginEdit();
            SuppressViewUpdate();

            try
            {
                column[DataRow] = value;
            }
            finally
            {
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

        private RowPresenter EditingRow
        {
            get
            {
                Debug.Assert(DataRow != null);
                return RowManager.EditingRow;
            }
            set
            {
                RowManager.EditingRow = value;
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

                EditingRow = value ? this : null;
                OnSetState(RowPresenterState.IsEditing);
            }
        }

        public void BeginEdit()
        {
            if (IsEditing)
                return;

            BeginEdit(false);
        }

        private bool _isNew;
        internal void BeginEdit(bool isNew)
        {
            _isNew = isNew;

            bool isEof = IsEof;
            if (isEof)
                RowManager.BeginEditEof();

            Debug.Assert(DataRow != null);

            var editingRow = EditingRow;
            if (editingRow == this)
                return;
            if (editingRow != null)
                editingRow.EndEdit();

            if (!isEof && !_isNew)
                DataRow.Save();
            IsEditing = true;
        }

        public void EndEdit()
        {
            if (!IsEditing)
                return;

            _isNew = false;
            IsEditing = false;
        }

        public void CancelEdit()
        {
            if (!IsEditing)
                return;

            bool isNew = _isNew;
            _isNew = false;
            IsEditing = false;  // IsEditing must be changed first because it requires non null DataRow

            if (isNew)
                Delete();
            else if (RowManager.EditingEofRow == this)
                RowManager.CancelEditEof();
            else
                DataRow.Load();
        }

        public void Delete()
        {
            VerifyDisposed();
            if (IsEof)
                throw new InvalidOperationException(Strings.RowPresenter_DeleteEof);

            DataRow.DataSet.RemoveAt(DataRow.Index);
        }

        public RowPresenter InsertChildRow(int ordinal)
        {
            VerifyHierarchical();

            if (ordinal < 0 || ordinal > HierarchicalChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(ordinal));

            var childRow = ordinal == HierarchicalChildrenCount ? null : GetHierarchicalChild(ordinal);
            var childDataSet = ChildDataSet;
            var index = childRow == null ? childDataSet.Count : childRow.DataRow.Index;
            childDataSet.Insert(index, new DataRow());
            var result = GetHierarchicalChild(ordinal);
            result.BeginEdit(true);
            return result;
        }

        internal RowView View { get; set; }

        internal IElementCollection ElementCollection { get; set; }
        internal IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
        }

        internal void SetElementsPanel(RowElementPanel elementsPanel)
        {
            Debug.Assert(elementsPanel != null);

            if (ElementCollection != null)
                Cleanup();
            Initialize(elementsPanel);
        }

        private int _rowItemsId;
        private void SelectRowItemGroup()
        {
            var newValue = Template.RowItemGroupSelector(this);
            if (newValue < 0 || newValue >= Template.RowItemGroups.Count)
                throw new InvalidOperationException();

            if (_rowItemsId == newValue)
                return;

            var bindingContext = BindingContext.Current;
            bindingContext.Enter(this);
            try
            {
                if (_rowItemsId >= 0)
                    ClearElements();

                _rowItemsId = newValue;
                InitializeElements();
            }
            finally
            {
                bindingContext.Exit();
            }
        }

        internal RowItemCollection RowItems
        {
            get { return Template.InternalRowItemGroups[_rowItemsId]; }
        }

        internal void Initialize(FrameworkElement elementsPanel)
        {
            Debug.Assert(ElementCollection == null);

            ElementCollection = ElementCollectionFactory.Create(elementsPanel);
            SelectRowItemGroup();
        }

        private void InitializeElements()
        {
            var rowItems = RowItems;
            for (int i = 0; i < rowItems.Count; i++)
            {
                var rowItem = rowItems[i];
                var element = rowItem.Generate();
                element.SetRowPresenter(this);
                ElementCollection.Add(element);
                rowItem.Initialize(element);
            }
        }

        internal void Cleanup()
        {
            Debug.Assert(ElementCollection != null);

            ClearElements();
            _rowItemsId = -1;
        }

        private void ClearElements()
        {
            var rowItems = RowItems;
            Debug.Assert(Elements.Count == rowItems.Count);
            for (int i = 0; i < rowItems.Count; i++)
            {
                var rowItem = rowItems[i];
                var element = Elements[i];
                rowItem.Cleanup(element);
                element.SetRowPresenter(null);
            }
            ElementCollection.RemoveRange(0, Elements.Count);
        }

        internal void RefreshElements()
        {
            if (Elements == null)
                return;

            SelectRowItemGroup();
            var rowItems = RowItems;
            Debug.Assert(Elements.Count == rowItems.Count);
            for (int i = 0; i < rowItems.Count; i++)
            {
                var rowItem = rowItems[i];
                var element = Elements[i];
                rowItem.UpdateTarget(element);
            }
        }

        internal int BlockOrdinal
        {
            get { return Ordinal / ElementManager.BlockDimensions; }
        }
    }
}
