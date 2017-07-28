using DevZest.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>Normalizes rows if recursive.</summary>
    internal abstract class RowNormalizer : RowMapper
    {
        protected RowNormalizer(Template template, DataSet dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy)
            : base(template, dataSet, where, orderBy)
        {
            Initialize();
        }

        private List<RowPresenter> _rows;
        public override IReadOnlyList<RowPresenter> Rows
        {
            get { return _rows; }
        }

        protected override void Reload()
        {
            base.Reload();
            Initialize();
        }

        private void Initialize()
        {
            _rows = IsRecursive ? new List<RowPresenter>() : (List<RowPresenter>)base.Rows;
            if (IsRecursive)
            {
                foreach (var row in base.Rows)
                    _rows.Add(row);
            }
            UpdateIndex(0);
        }

        private void UpdateIndex(int startIndex)
        {
            UpdateIndex(startIndex, _rows.Count - 1);
        }

        private void UpdateIndex(int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++)
                _rows[i].RawIndex = i;
        }

        protected virtual void OnRowsChanged()
        {
        }

        internal void Expand(RowPresenter row)
        {
            Debug.Assert(IsRecursive && !row.IsExpanded);

            var nextIndex = row.RawIndex + 1;
            var lastIndex = nextIndex;
            for (int i = 0; i < row.Children.Count; i++)
            {
                var childRow = row.Children[i];
                lastIndex = InsertRecursively(lastIndex, childRow);
            }

            if (lastIndex == nextIndex)
                return;

            UpdateIndex(nextIndex);
            OnRowsChanged();
        }

        private int InsertRecursively(int index, RowPresenter row)
        {
            Debug.Assert(IsRecursive);

            _rows.Insert(index++, row);
            if (row.IsExpanded)
            {
                var children = row.Children;
                foreach (var childRow in children)
                    index = InsertRecursively(index, childRow);
            }
            return index;
        }

        internal virtual void Collapse(RowPresenter row)
        {
            Debug.Assert(IsRecursive && row.IsExpanded);

            var nextIndex = row.RawIndex + 1;
            int count = NextIndexOf(row) - nextIndex;
            if (count == 0)
                return;

            RemoveRange(nextIndex, count);
            UpdateIndex(nextIndex);
            OnRowsChanged();
        }

        private int NextIndexOf(RowPresenter row)
        {
            Debug.Assert(IsRecursive && row != null && row.RawIndex >= 0);

            var depth = row.Depth;
            var result = row.RawIndex + 1;
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
                _rows[startIndex + i].RawIndex = -1;

            _rows.RemoveRange(startIndex, count);
        }

        protected sealed override void OnRowAdded(RowPresenter row, int index)
        {
            if (IsRecursive)
            {
                var parent = row.Parent;
                if (parent != null)
                {
                    if (parent.IsExpanded)
                        index = parent.RawIndex + index + 1;
                    else
                        return;
                }
                _rows.Insert(index, row);
            }
            UpdateIndex(index);
            OnRowsChanged();
        }

        protected sealed override void OnRowRemoved(RowPresenter parent, int index)
        {
            if (IsRecursive)
            {
                if (parent != null)
                {
                    if (parent.IsExpanded)
                        index = parent.RawIndex + index + 1;
                    else
                        return;
                }
                _rows.RemoveAt(index);
            }
            UpdateIndex(index);
            OnRowsChanged();
        }

        protected sealed override void OnRowMoved(RowPresenter row, int oldIndex, int newIndex)
        {
            if (IsRecursive)
            {
                var parent = row.Parent;
                if (parent != null)
                {
                    if (parent.IsExpanded)
                    {
                        oldIndex = parent.RawIndex + oldIndex + 1;
                        newIndex = parent.RawIndex + newIndex + 1;
                    }
                    else
                        return;
                }
                _rows.RemoveAt(oldIndex);
                _rows.Insert(newIndex, row);
            }
            UpdateIndex(Math.Min(oldIndex, newIndex), Math.Max(oldIndex, newIndex));
            OnRowsChanged();
        }
    }
}
