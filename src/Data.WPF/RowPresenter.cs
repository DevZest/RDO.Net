using DevZest.Data.Primitives;
using DevZest.Data.Windows.Controls;
using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class RowPresenter
    {
        internal RowPresenter(RowMapper rowMapper, DataRow dataRow)
            : this(rowMapper, dataRow, -1)
        {
        }

        internal RowPresenter(RowMapper rowMapper, int placeholderIndex)
            : this(rowMapper, null, placeholderIndex)
        {
        }

        private RowPresenter(RowMapper rowMapper, DataRow dataRow, int absoluteIndex)
        {
            Debug.Assert(rowMapper != null);
            _rowMapper = rowMapper;
            DataRow = dataRow;
            RawIndex = absoluteIndex;
        }

        internal void Dispose()
        {
            Debug.Assert(View == null, "Row should be virtualized first before dispose.");
            _rowMapper = null;
            Parent = null;
        }

        internal bool IsDisposed
        {
            get { return _rowMapper == null; }
        }

        private void VerifyDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private RowMapper _rowMapper;

        private RowMapper RowMapper
        {
            get
            {
                VerifyDisposed();
                return _rowMapper;
            }
        }

        private RowNormalizer RowNormalizer
        {
            get { return RowMapper as RowNormalizer; }
        }

        internal RowManager RowManager
        {
            get { return RowMapper as RowManager; }
        }

        internal ElementManager ElementManager
        {
            get { return RowMapper as ElementManager; }
        }

        internal LayoutManager LayoutManager
        {
            get { return RowMapper as LayoutManager; }
        }

        private bool IsRecursive
        {
            get { return !IsPlaceholder && Template.IsRecursive; }
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
            get { return IsDisposed ? null : _rowMapper.Template; }
        }

        public DataRow DataRow { get; internal set; }

        public bool IsPlaceholder
        {
            get { return RowManager == null ? false : RowManager.Placeholder == this; }
        }

        public bool IsEditing
        {
            get { return RowManager.CurrentRow == this && RowManager.IsEditing; }
        }

        public bool IsAdding
        {
            get { return IsPlaceholder && IsEditing; }
        }

        public RowPresenter Parent { get; internal set; }

        private List<RowPresenter> _children;

        public IReadOnlyList<RowPresenter> Children
        {
            get
            {
                if (_children != null)
                    return _children;
                return Array<RowPresenter>.Empty;
            }
        }

        internal void InitializeChildren()
        {
            Debug.Assert(IsRecursive);

            _children = _rowMapper.GetOrCreateChildren(this);
            foreach (var child in Children)
                child.InitializeChildren();
        }

        internal void InsertChild(int index, RowPresenter child)
        {
            Debug.Assert(index >= 0 && index <= Children.Count);
            if (_children == null)
                _children = new List<RowPresenter>();
            _children.Insert(index, child);
        }

        internal void RemoveChild(int index)
        {
            Debug.Assert(index >= 0 && index < Children.Count);
            _children.RemoveAt(index);
            if (_children.Count == 0)
                _children = null;
        }

        private void Invalidate()
        {
            var rowManager = RowManager;
            if (rowManager != null)
                RowManager.Invalidate(this);
        }

        internal int RawIndex { get; set; }

        public int Index
        {
            get
            {
                if (IsDisposed)
                    return -1;
                var result = RawIndex;
                var placeholder = RowManager.Placeholder;
                if (placeholder != null && placeholder != this && result >= placeholder.Index)
                    result++;
                return result;
            }
        }

        public int Depth
        {
            get { return Parent == null ? 0 : RowMapper.GetDepth(Parent.DataRow) + 1; }
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

            RowNormalizer.Expand(this);
            IsExpanded = true;
        }

        public void Collapse()
        {
            VerifyRecursive();

            if (!IsExpanded)
                return;

            RowNormalizer.Collapse(this);
            IsExpanded = false;
        }

        private void VerifyRecursive()
        {
            if (!IsRecursive)
                throw new InvalidOperationException(Strings.RowPresenter_VerifyRecursive);
        }

        public bool IsCurrent
        {
            get { return RowManager.CurrentRow == this; }
        }

        public bool IsSelected
        {
            get { return RowManager.IsSelected(this); }
            set
            {
                if (IsPlaceholder)
                    return;

                var oldValue = IsSelected;
                if (oldValue == value)
                    return;

                if (oldValue)
                    RowManager.RemoveSelectedRow(this);
                if (value)
                    RowManager.AddSelectedRow(this);

                Invalidate();
            }
        }

        public T GetValue<T>(Column<T> column)
        {
            VerifyColumn(column, nameof(column));

            if (Depth != RowMapper.GetDepth(column.GetParentModel()))
                column = (Column<T>)DataRow.Model.GetColumns()[column.Ordinal];

            return DataRow == null ? default(T) : column[DataRow];
        }

        private void VerifyColumn(Column column, string paramName)
        {
            if (column == null)
                throw new ArgumentNullException(paramName);

            if (column.GetParentModel() != RowMapper.DataSet.Model)
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
                column.SetValue(DataRow, value);
            }
        }

        private void BeginEdit()
        {
            if (IsEditing)
                return;

            if (RowManager.CurrentRow != this)
                throw new InvalidOperationException();
            RowManager.BeginEdit();
        }

        public void EditValue<T>(Column<T> column, T value)
        {
            VerifyColumn(column, nameof(column));

            if (Depth > 0)
                column = (Column<T>)DataRow.Model.GetColumns()[column.Ordinal];

            BeginEdit();
            column[DataRow] = value;
        }

        public DataSet DataSet
        {
            get { return Parent == null ? RowManager.DataSet : Parent.DataRow[Template.RecursiveModelOrdinal]; }
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

        internal RowItemCollection RowItems
        {
            get { return Template.InternalRowItems; }
        }

        internal void InitElementPanel(RowElementPanel elementPanel)
        {
            Debug.Assert(ElementCollection == null);

            ElementCollection = ElementCollectionFactory.Create(elementPanel);
            InitializeElements();
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
