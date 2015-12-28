﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class DataRowPresenter
    {
        internal DataRowPresenter(DataSetPresenter owner, DataRow dataRow)
            : this(owner, dataRow, DataViewRowType.DataRow)
        {
        }

        internal DataRowPresenter(DataSetPresenter owner, DataViewRowType rowType)
            : this(owner, null, rowType)
        {            
        }

        private DataRowPresenter(DataSetPresenter owner, DataRow dataRow, DataViewRowType rowType)
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
            if (RowType != DataViewRowType.DataRow)
                return s_emptyChildren;

            var childItems = Template.ChildItems;
            if (childItems.Count == 0)
                return s_emptyChildren;

            var result = new DataSetPresenter[childItems.Count];
            for (int i = 0; i < childItems.Count; i++)
                //result[i] = new DataSetPresenter(this, childItems[i].Template);
                throw new NotImplementedException();

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

        public DataViewRowType RowType { get; private set; }

        public bool IsFocused { get; internal set; }

        private static UIElement[] s_emptyUIElements = new UIElement[0];
        private UIElement[] _uiElements = null;

        private GridItem GridItemOf(int index)
        {
            Debug.Assert(_uiElements != null && index >= 0 && index < _uiElements.Length);
            var template = Template;
            Debug.Assert(template != null);
            var listItems = template.ListItems;
            var listItemsCount = listItems.Count;
            if (index < listItemsCount)
                return listItems[index];
            else
                return template.ChildItems[index - listItemsCount];
        }

        private int UIElementsCount
        {
            get
            {
                var template = Template;
                return template.ListItems.Count + template.ChildItems.Count;
            }
        }

        private void EnsureUIElementsGenerated()
        {
            if (_uiElements != null)
                return;

            var uiElementsCount = UIElementsCount;
            if (uiElementsCount == 0)
            {
                _uiElements = s_emptyUIElements;
                return;
            }

            _uiElements = new UIElement[uiElementsCount];

            for (int i = 0; i < uiElementsCount; i++)
            {
                var gridItem = GridItemOf(i);
                _uiElements[i] = gridItem.Generate();
                Debug.Assert(_uiElements[i] != null);
            }

        }

        private void EnsureUIElementsRecycled()
        {
            if (_uiElements == null)
                return;

            for (int i = 0; i < _uiElements.Length; i++)
            {
                var gridItem = GridItemOf(i);
                var uiElement = _uiElements[i];
                Debug.Assert(uiElement != null);
                gridItem.Recycle(uiElement);
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
