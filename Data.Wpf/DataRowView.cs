using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows
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
            InitChildDataSetViews();
        }

        public DataRow DataRow { get; private set; }

        public Model Model
        {
            get { return Owner.Model; }
        }

        private static DataSetView[] s_emptyChildSetViews = new DataSetView[0];

        DataSetView[] _childDataSetViews;

        private void InitChildDataSetViews()
        {
            var childTemplates = Template.ChildTemplates;
            if (childTemplates == null || childTemplates.Length == 0)
            {
                _childDataSetViews = s_emptyChildSetViews;
                return;
            }

            _childDataSetViews = new DataSetView[childTemplates.Length];
            for (int i = 0; i < childTemplates.Length; i++)
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
    }
}
