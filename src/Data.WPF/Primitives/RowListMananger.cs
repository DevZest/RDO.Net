using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class RowListMananger : RowMapper
    {
        protected RowListMananger(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
            : base(template, dataSet, where, orderBy)
        {
        }

        private List<RowPresenter> _rows;
        public override IReadOnlyList<RowPresenter> Rows
        {
            get { return _rows; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            _rows = IsRecursive ? new List<RowPresenter>() : (List<RowPresenter>)base.Rows;
            if (IsRecursive)
            {
                int index = 0;
                foreach (var row in base.Rows)
                    Insert(index++, row);
            }
        }

        private void Insert(int index, RowPresenter row)
        {
            Debug.Assert(row != null);

            _rows.Insert(index, row);
            row.Index = index;
        }

        private void UpdateIndex(int startIndex)
        {
            Debug.Assert(Rows != base.Rows);

            for (int i = startIndex; i < _rows.Count; i++)
                _rows[i].Index = i;
        }

        internal void Expand(RowPresenter row)
        {
            Debug.Assert(IsRecursive && !row.IsExpanded);

            var nextIndex = row.Index + 1;
            for (int i = 0; i < row.Children.Count; i++)
            {
                var childRow = row.Children[i];
                nextIndex = InsertRecursively(nextIndex, childRow);
            }
            UpdateIndex(nextIndex);
            OnRowsChanged();
        }

        private int InsertRecursively(int index, RowPresenter row)
        {
            Debug.Assert(IsRecursive && !row.IsPlaceholder);

            Insert(index++, row);
            if (row.IsExpanded)
            {
                var children = row.Children;
                foreach (var childRow in children)
                    index = InsertRecursively(index, childRow);
            }
            return index;
        }

        internal void Collapse(RowPresenter row)
        {
            Debug.Assert(IsRecursive && row.IsExpanded);

            var nextIndex = row.Index + 1;
            int count = NextIndexOf(row) - nextIndex;
            if (count == 0)
                return;

            RemoveRange(nextIndex, count);
            UpdateIndex(nextIndex);
            OnRowsChanged();
        }

        private int NextIndexOf(RowPresenter row)
        {
            Debug.Assert(IsRecursive && row != null && row.Index >= 0);

            var depth = row.Depth;
            var result = row.Index + 1;
            for (; result < _rows.Count; result++)
            {
                if (_rows[result].Depth <= depth)
                    break;
            }
            return result;
        }

        private void RemoveRange(int startIndex, int count)
        {
            Debug.Assert(count > 0);

            for (int i = 0; i < count; i++)
                _rows[startIndex + i].Index = -1;

            _rows.RemoveRange(startIndex, count);
        }

        private void RemoveAt(int index)
        {
            _rows[index].Index = -1;
            _rows.RemoveAt(index);
        }
    }
}
