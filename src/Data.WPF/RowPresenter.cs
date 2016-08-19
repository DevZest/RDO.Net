using DevZest.Data.Primitives;
using DevZest.Data.Windows.Controls;
using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class RowPresenter
    {
        internal RowPresenter(RowManager rowManager, DataRow dataRow)
            : this(rowManager, dataRow, -1)
        {
        }

        internal RowPresenter(RowManager rowManager, int index)
            : this(rowManager, null, index)
        {
        }

        private RowPresenter(RowManager rowManager, DataRow dataRow, int index)
        {
            Debug.Assert(rowManager != null);
            _rowManager = rowManager;
            DataRow = dataRow;
            Index = index;
            _rowItemsId = -1;
        }

        internal void Dispose()
        {
            Debug.Assert(View == null, "Row should be virtualized first before dispose.");
            _rowManager = null;
            Index = -1;
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
            get { return !IsPlaceholder && RowManager.Template.IsRecursive; }
        }

        public DataPresenter DataPresenter
        {
            get
            {
                var layoutManager = LayoutManager;
                return layoutManager == null ? null : layoutManager.DataPresenter;
            }
        }

        public Template Template
        {
            get { return RowManager.Template; }
        }

        public DataRow DataRow { get; internal set; }

        public bool IsPlaceholder
        {
            get { return RowManager.Placeholder == this; }
        }

        public bool IsEditing
        {
            get { return RowManager.EditingRow == this; }
        }

        public bool IsAdding
        {
            get { return IsPlaceholder && IsEditing; }
        }

        public RowPresenter Parent { get; private set; }

        public IReadOnlyList<RowPresenter> Children { get; private set; }

        internal void InitializeChildren()
        {
            Debug.Assert(IsRecursive);

            Children = RowManager.RowMappings_GetOrCreateChildren(DataRow).ToList();
            foreach (var child in Children)
                child.InitializeChildren();
        }

        private void Invalidate()
        {
            RowManager.Invalidate(this);
        }

        private int _index;
        public int Index
        {
            get
            {
                var result = _index;
                var placeholder = RowManager.Placeholder;
                if (placeholder != null && placeholder != this && result >= placeholder.Index)
                    result++;
                return result;
            }
            internal set { _index = value; }
        }

        public int Depth
        {
            get { return Parent == null ? 0 : RowManager.GetDepth(Parent.DataRow); }
        }

        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            private set
            {
                _isExpanded = value;
                Invalidate();
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
            get { return _isCurrent; }
            internal set
            {
                if (_isCurrent == value)
                    return;

                _isCurrent = value;
                if (_rowManager != null)  // RowPresenter can be disposed upon here, check to avoid ObjectDisposedException
                    Invalidate();
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value)
                    return;

                if (_isSelected)
                    RowManager.RemoveSelectedRow(this);

                _isSelected = value;

                if (_isSelected)
                    RowManager.AddSelectedRow(this);

                Invalidate();
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

        public void EditValue<T>(Column<T> column, T value)
        {
            VerifyColumn(column, nameof(column));

            if (Depth > 0)
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

        private DataSet DataSet
        {
            get { return Parent == null ? RowManager.DataSet : Parent.DataRow[Template.RecursiveModelOrdinal]; }
        }

        public void BeginEdit()
        {
            if (IsEditing)
                return;

            bool success;
            if (IsPlaceholder)
            {
                DataRow = DataSet.BeginAdd();
                success = DataRow != null;
            }
            else
                success = DataRow.BeginEdit();

            if (success)
                RowManager.EditingRow = this;

            if (!IsEditing)
                throw new InvalidOperationException(Strings.RowPresenter_BeginEditFailed);
        }

        public void EndEdit()
        {
            if (!IsEditing)
                return;

            if (IsPlaceholder)
                DataRow = DataSet.EndAdd();
            else
                DataRow.EndEdit();
            RowManager.EditingRow = null;
        }

        public void CancelEdit()
        {
            if (!IsEditing)
                return;

            if (IsPlaceholder)
            {
                DataSet.CancelAdd();
                DataRow = null;
            }
            else
                DataRow.CancelEdit();
            RowManager.EditingRow = null;
        }

        public void Delete()
        {
            VerifyDisposed();
            if (IsPlaceholder)
                throw new InvalidOperationException(Strings.RowPresenter_DeleteEof);

            DataRow.DataSet.Remove(DataRow);
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
