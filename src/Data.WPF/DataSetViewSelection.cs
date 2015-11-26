using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    internal abstract class DataSetViewSelection : IReadOnlyList<int>
    {
        public abstract int this[int index] { get; }

        public abstract int Count { get; }

        public abstract int Current { get; }

        public DataSetViewSelection Select(int index, SelectionMode selectionMode)
        {
            if (selectionMode == SelectionMode.Single)
                return SelectSingle(index);
            else if (selectionMode == SelectionMode.Multiple)
                return SelectMultiple(index);
            else
                return SelectExtended(index);
        }

        protected abstract DataSetViewSelection SelectSingle(int index);

        protected abstract DataSetViewSelection SelectMultiple(int index);

        protected abstract DataSetViewSelection SelectExtended(int index);

        public DataSetViewSelection Coerce(int totalCount)
        {
            return totalCount == 0 ? Empty : CoerceOverride(totalCount);
        }

        protected abstract DataSetViewSelection CoerceOverride(int totalCount);

        public abstract bool IsSelected(int index);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        public static DataSetViewSelection Empty = new EmptySelection();

        private sealed class EmptySelection : DataSetViewSelection
        {
            public override int Count
            {
                get { return 0; }
            }

            public override int this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public override int Current
            {
                get { return -1; }
            }

            protected override DataSetViewSelection SelectSingle(int index)
            {
                return new SingleSelection(index);
            }

            protected override DataSetViewSelection SelectMultiple(int index)
            {
                throw new NotImplementedException();
            }

            protected override DataSetViewSelection SelectExtended(int index)
            {
                return new ExtendedSelection(index, index);
            }

            protected override DataSetViewSelection CoerceOverride(int totalCount)
            {
                return SelectSingle(0);
            }

            public override bool IsSelected(int index)
            {
                return false;
            }
        }

        private sealed class SingleSelection : DataSetViewSelection
        {
            public SingleSelection(int current)
            {
                _current = current;
            }

            private int _current;

            public override int Count
            {
                get { return 1; }
            }

            public override int this[int index]
            {
                get
                {
                    if (index != 0)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return _current;
                }
            }

            public override int Current
            {
                get { return _current; }
            }

            protected override DataSetViewSelection SelectSingle(int index)
            {
                _current = index;
                return this;
            }

            protected override DataSetViewSelection SelectMultiple(int index)
            {
                return new MultipleSelection(this, index);
            }

            protected override DataSetViewSelection SelectExtended(int index)
            {
                return new ExtendedSelection(_current, index);
            }

            protected override DataSetViewSelection CoerceOverride(int totalCount)
            {
                if (_current >= totalCount)
                    _current = totalCount - 1;
                return this;
            }

            public override bool IsSelected(int index)
            {
                return _current == index;
            }
        }

        private sealed class ExtendedSelection : DataSetViewSelection
        {
            public ExtendedSelection(int current, int extended)
            {
                _current = current;
                _extended = extended;
            }

            int _current;
            int _extended;

            public override int Current
            {
                get { return _current; }
            }

            public override int Count
            {
                get { return Math.Abs(_extended - _current) + 1; }
            }

            public override int this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    return Math.Min(_current, _extended) + index;
                }
            }

            protected override DataSetViewSelection SelectSingle(int index)
            {
                return new SingleSelection(index);
            }

            protected override DataSetViewSelection SelectExtended(int index)
            {
                _extended = index;
                return this;
            }

            protected override DataSetViewSelection SelectMultiple(int index)
            {
                return new MultipleSelection(this, index);
            }

            protected override DataSetViewSelection CoerceOverride(int totalCount)
            {
                if (_current >= totalCount)
                    _current = totalCount - 1;
                if (_extended >= totalCount)
                    _extended = totalCount - 1;

                return this;
            }

            public override bool IsSelected(int index)
            {
                return index >= Math.Min(_current, _extended) && index <= Math.Max(_current, _extended);
            }
        }

        private sealed class MultipleSelection : DataSetViewSelection
        {
            public MultipleSelection(DataSetViewSelection selection, int index)
            {
                for (int i = 0; i < selection.Count; i++)
                    SelectMultiple(selection[i]);
            }

            private int _current;
            private List<int> _selectedList = new List<int>();
            private HashSet<int> _selectedSet = new HashSet<int>();

            public override int Current
            {
                get { return _current; }
            }

            public override int Count
            {
                get { return _selectedList.Count; }
            }

            public override int this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    return _selectedList[index];
                }
            }

            protected override DataSetViewSelection SelectSingle(int index)
            {
                return new SingleSelection(index);
            }

            protected override DataSetViewSelection SelectMultiple(int index)
            {
                if (_selectedSet.Contains(index))
                {
                    _selectedList.Remove(index);
                    _selectedSet.Remove(index);
                }
                else
                {
                    _selectedList.Add(index);
                    _selectedSet.Add(index);
                }
                _current = index;

                return this;
            }

            protected override DataSetViewSelection SelectExtended(int index)
            {
                return new ExtendedSelection(_current, index);
            }

            protected override DataSetViewSelection CoerceOverride(int totalCount)
            {
                if (_current >= totalCount)
                    _current = totalCount - 1;

                for (int i = 0; i < Count; i++)
                {
                    var index = this[i];
                    if (index >= totalCount)
                    {
                        _selectedList.RemoveAt(i);
                        _selectedSet.Remove(index);
                    }
                }

                return this;
            }

            public override bool IsSelected(int index)
            {
                return _selectedSet.Contains(index);
            }
        }
    }
}

