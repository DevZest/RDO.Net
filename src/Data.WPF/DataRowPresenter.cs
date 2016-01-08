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
            Owner = owner;
            DataRow = dataRow;
            RowType = rowType;
            Children = InitChildDataSetPresenters();
        }

        internal void Dispose()
        {
            Owner = null;
            Children = null;
            EnsureUIElementsRecycled();
        }

        public DataSetPresenter Owner { get; private set; }

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

        public bool IsCurrent
        {
            get { return Owner == null ? false : Owner.IndexOf(this) == Owner.Current; }
        }

        public bool IsSelected
        {
            get { return Owner == null ? false : Owner.IsSelected(this); }
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
