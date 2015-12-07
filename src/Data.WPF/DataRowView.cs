using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class DataRowView : IReadOnlyList<DataSetView>
    {
        internal DataRowView(DataSetView owner, DataRow dataRow)
        {
            Debug.Assert(owner != null);
            Debug.Assert(dataRow == null || owner.Model == dataRow.Model);
            Owner = owner;
            DataRow = dataRow;
            InitChildDataSetViews();
        }

        internal void Dispose()
        {
            Owner = null;
            _childDataSetViews = s_emptyChildSetViews;
        }

        private DataSetView _owner;
        public DataSetView Owner
        {
            get { return _owner; }
            private set
            {
                if (value == null)
                    DisposeUIElements();
                _owner = value;
                if (value != null)
                    InitUIElements();
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

        private static DataSetView[] s_emptyChildSetViews = new DataSetView[0];

        DataSetView[] _childDataSetViews;

        private void InitChildDataSetViews()
        {
            var childTemplates = Template.ChildTemplates;
            if (childTemplates == null || childTemplates.Count == 0)
            {
                _childDataSetViews = s_emptyChildSetViews;
                return;
            }

            _childDataSetViews = new DataSetView[childTemplates.Count];
            for (int i = 0; i < childTemplates.Count; i++)
                _childDataSetViews[i] = new DataSetView(this, childTemplates[i]);
        }

        public IEnumerator<DataSetView> GetEnumerator()
        {
            foreach (var dataSetView in _childDataSetViews)
                yield return dataSetView;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _childDataSetViews.GetEnumerator();
        }

        public int Count
        {
            get { return _childDataSetViews.Length; }
        }

        public DataSetView this[int index]
        {
            get { return _childDataSetViews[index]; }
        }

        public bool IsCurrent
        {
            get { return Owner == null ? false : Owner.IsCurrent(Owner.IndexOf(this)); }
        }

        public bool IsEof
        {
            get { return DataRow == null; }
        }

        public bool IsSelected
        {
            get { return Owner == null ? false : Owner.IsSelected(Owner.IndexOf(this)); }
        }

        public T GetValue<T>(Column<T> column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return IsEof ? default(T) : column[DataRow];
        }

        public void SetValue<T>(Column<T> column, T value)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            throw new NotImplementedException();
        }

        private static UIElement[] s_emptyUIElements = new UIElement[0];
        private UIElement[] _uiElements = s_emptyUIElements;
        private void DisposeUIElements()
        {
            var template = Template;
            if (template == null)
                return;
            for (int i = 0; i < _uiElements.Length; i++)
            {
                var uiElement = _uiElements[i];
                if (uiElement != null)
                    template.ListItems[i].Recycle(uiElement);
            }
            _uiElements = s_emptyUIElements;
        }

        private void InitUIElements()
        {
            var listItems = Template.ListItems;
            if (listItems == null || listItems.Count == 0)
            {
                _uiElements = s_emptyUIElements;
                return;
            }

            _uiElements = new UIElement[listItems.Count];
        }
    }
}
