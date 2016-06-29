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
            _index = -1;
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

        private bool IsRecursive
        {
            get { return !IsEof && RowManager.IsRecursive; }
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
            get { return DataRow == null || DataRow == DataRow.Placeholder; }
        }

        public RowPresenter RecursiveParent
        {
            get
            {
                if (!IsRecursive)
                    return null;

                var parentDataRow = DataRow.ParentDataRow;
                return parentDataRow == null ? null : RowManager.RowMappings_GetRow(parentDataRow);
            }
        }

        public int RecursiveChildrenCount
        {
            get
            {
                if (!IsRecursive)
                    return 0;

                OnGetState(RowPresenterState.RecursiveChildren);
                return ChildDataSet.Count;
            }
        }

        private DataSet ChildDataSet
        {
            get
            {
                Debug.Assert(IsRecursive);
                return DataRow[Template.RecursiveModelOrdinal];
            }
        }

        public RowPresenter GetRecursiveChild(int index)
        {
            if (index < 0 || index >= RecursiveChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            OnGetState(RowPresenterState.RecursiveChildren);
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

        private int _index = -1;
        public int Index
        {
            get
            {
                OnGetState(RowPresenterState.Index);
                if (RowManager.IsRecursive)
                    return _index;
                else
                    return IsEof ? RowManager.Rows.Count - 1 : DataRow.Index;
            }
            internal set
            {
                _index = value;
            }
        }

        public int Depth
        {
            get { return IsRecursive ? DataRow.Model.GetDepth() : 0; }
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
            VerifyRecursive();

            if (IsExpanded)
                return;

            RowManager.Expand(this);
            IsExpanded = true;
        }

        public void Collapse()
        {
            VerifyRecursive();

            if (!IsExpanded)
                return;

            RowManager.Collapse(this);
            IsExpanded = false;
        }

        private void VerifyRecursive()
        {
            if (!IsRecursive)
                throw new InvalidOperationException(Strings.RowPresenter_VerifyRecursive);
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

            if (Depth > 0)
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

                if (Depth > 0)
                    column = DataRow.Model.GetColumns()[column.Ordinal];
                return DataRow == null ? GetDefault(column.DataType) : column.GetValue(DataRow);
            }
            set
            {
                VerifyColumn(column, nameof(column));

                if (Depth > 0)
                    column = DataRow.Model.GetColumns()[column.Ordinal];

                if (AutoBeginEdit)
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

        private bool AutoBeginEdit
        {
            get { return RowManager.AutoBeginEdit; }
        }

        public void SetValue<T>(Column<T> column, T value)
        {
            VerifyColumn(column, nameof(column));

            if (Depth > 0)
                column = (Column<T>)DataRow.Model.GetColumns()[column.Ordinal];

            if (AutoBeginEdit)
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
            get { throw new NotImplementedException(); }
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
            get { return GetEditingRow(Model) == this; }
        }

        private readonly static ConditionalWeakTable<Model, RowPresenter> s_editingRows = new ConditionalWeakTable<Model, RowPresenter>();

        private static RowPresenter GetEditingRow(Model model)
        {
            RowPresenter result;
            return s_editingRows.TryGetValue(model, out result) ? result : null;
        }

        private static void ClearEditingRow(Model model)
        {
            s_editingRows.Remove(model);
        }

        private static void BeginEdit(Model model, RowPresenter value)
        {
            Debug.Assert(value != null);

            var oldValue = GetEditingRow(model);
            Debug.Assert(oldValue != value);

            if (oldValue != null)
                oldValue.CancelEdit();

            if (value != null)
                s_editingRows.Add(model, value);
        }

        private DataSet DataSet
        {
            get { return RowManager.DataSet; }
        }

        private Model Model
        {
            get { return DataSet.Model; }
        }

        public void BeginEdit()
        {
            if (IsEditing)
                return;

            BeginEdit(Model, this);
            if (IsEof)
            {
                DataRow = DataRow.Placeholder;
                DataSet.BeginAdd();
            }
            else
                DataRow.BeginEdit();
            RowManager.EditingRow = this;
        }

        public void EndEdit()
        {
            if (!IsEditing)
                return;

            if (IsEof)
            {
                DataSet.EndAdd();
                DataRow = null;
            }
            else
                DataRow.EndEdit();
            ClearEditingRow(Model);
            RowManager.EditingRow = null;
        }

        public void CancelEdit()
        {
            if (!IsEditing)
                return;

            if (IsEof)
            {
                DataSet.CancelAdd();
                DataRow = null;
            }
            else
                DataRow.CancelEdit();
            ClearEditingRow(Model);
            RowManager.EditingRow = null;
        }

        public void Delete()
        {
            VerifyDisposed();
            if (IsEof)
                throw new InvalidOperationException(Strings.RowPresenter_DeleteEof);

            DataRow.DataSet.Remove(DataRow);
        }

        public RowPresenter InsertChildRow(int ordinal, Action<DataRow> updateAction = null)
        {
            VerifyRecursive();

            var childDataSet = ChildDataSet;
            if (ordinal < 0 || ordinal > childDataSet.Count)
                throw new ArgumentOutOfRangeException(nameof(ordinal));

            var dataRow = new DataRow();
            childDataSet.Insert(ordinal, dataRow, updateAction);
            return RowManager.RowMappings_GetRow(dataRow);
        }

        internal RowView View { get; set; }

        internal IElementCollection ElementCollection { get; set; }
        internal IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
        }

        internal void SetElementPanel(RowElementPanel elementPanel)
        {
            if (ElementCollection != null)
                Cleanup();

            if (elementPanel != null)
                InitElementPanel(elementPanel);
        }

        private int _rowItemsId;
        private void SelectRowItemGroup()
        {
            var newValue = Template.RowItemGroupSelector(this);
            if (newValue < 0 || newValue >= Template.RowItemGroups.Count)
                throw new InvalidOperationException();

            if (_rowItemsId == newValue)
                return;

            if (_rowItemsId >= 0)
                ClearElements();

            _rowItemsId = newValue;
            InitializeElements();
        }

        internal RowItemCollection RowItems
        {
            get { return Template.InternalRowItemGroups[_rowItemsId]; }
        }

        internal void InitElementPanel(RowElementPanel elementPanel)
        {
            Debug.Assert(ElementCollection == null);

            ElementCollection = ElementCollectionFactory.Create(elementPanel);
            SelectRowItemGroup();
        }

        private void InitializeElements()
        {
            var rowItems = RowItems;
            for (int i = 0; i < rowItems.Count; i++)
            {
                var rowItem = rowItems[i];
                rowItem.Mount(this, x => ElementCollection.Add(x));
            }
        }

        internal void Cleanup()
        {
            Debug.Assert(ElementCollection != null);

            ClearElements();
            ElementCollection = null;
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
                rowItem.Unmount(element);
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
                rowItem.Refresh(element);
            }
        }

        internal int BlockOrdinal
        {
            get { return Index / ElementManager.BlockDimensions; }
        }

        internal int BlockDimension
        {
            get { return Index % ElementManager.BlockDimensions; }
        }
    }
}
