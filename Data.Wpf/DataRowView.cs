using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Wpf
{
    public sealed class DataRowView : IReadOnlyList<DataSetView>
    {
        public DataSetView Owner { get; private set; }

        public GridTemplate Template
        {
            get { return Owner.Template; }
        }

        internal void Initialize(DataSetView owner, DataRow dataRow)
        {
            Debug.Assert(owner != null);
            Debug.Assert(dataRow != null && owner.Model == dataRow.Model);
            Owner = owner;
            DataRow = dataRow;
            InitChildSetViews();
        }

        public DataRow DataRow { get; private set; }

        public Model Model
        {
            get { return Owner.Model; }
        }

        private static DataSetView[] s_emptyChildSetViews = new DataSetView[0];

        DataSetView[] _childSetViews;

        private void InitChildSetViews()
        {
            var childSetItems = Template.ChildSetItems;
            if (childSetItems.Count == 0)
            {
                _childSetViews = s_emptyChildSetViews;
                return;
            }

            _childSetViews = new DataSetView[childSetItems.Count];
            for (int i = 0; i < childSetItems.Count; i++)
                _childSetViews[i] = new DataSetView(this, childSetItems[i].Template);
        }

        public IEnumerator<DataSetView> GetEnumerator()
        {
            foreach (var dataSetView in _childSetViews)
                yield return dataSetView;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _childSetViews.GetEnumerator();
        }

        public int Count
        {
            get { return _childSetViews.Length; }
        }

        public DataSetView this[int index]
        {
            get { return _childSetViews[index]; }
        }
    }
}
