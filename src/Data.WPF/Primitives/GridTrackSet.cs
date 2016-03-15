using System;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class GridTrackSet
    {
        protected abstract class EmptyGridTrackSet<T> : IGridTrackSet<T>
            where T : GridTrack
        {
            public int Count
            {
                get { return 0; }
            }

            public T this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            GridTrack IGridTrackSet.this[int index]
            {
                get { return this[index]; }
            }
        }

        protected abstract class ListGridTrackSet<T> : IGridTrackSet<T>
            where T : GridTrack
        {
            protected ListGridTrackSet(IGridTrackSet<T> x, IGridTrackSet<T> y)
            {
                Debug.Assert(x != null && x.Count > 0);
                Debug.Assert(y != null && y.Count > 0);
                _list = Merge(x, y);
            }

            private static T[] Merge(IGridTrackSet<T> x, IGridTrackSet<T> y)
            {
                Debug.Assert(x != null && x.Count > 0);
                Debug.Assert(y != null && y.Count > 0);
                var result = new T[x.Count + y.Count];
                int indexX = 0;
                int indexY = 0;
                for (int i = 0; i < result.Length; i++)
                {
                    T value;
                    if (indexX == x.Count)
                        value = y[indexY++];
                    else if (indexY == y.Count)
                        value = x[indexX++];
                    else if (x[indexX].Ordinal < y[indexY].Ordinal)
                        value = x[indexX++];
                    else
                    {
                        Debug.Assert(x[indexX].Ordinal > y[indexY].Ordinal);
                        value = y[indexY++];
                    }
                    result[i] = value;
                }
                

                return result;
            }

            T[] _list;

            public int Count
            {
                get { return _list.Length; }
            }

            public T this[int index]
            {
                get { return _list[index]; }
            }

            GridTrack IGridTrackSet.this[int index]
            {
                get { return this[index]; }
            }
        }
    }
}
