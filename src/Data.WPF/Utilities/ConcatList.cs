using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest
{
    internal interface IConcatList<out T> : IReadOnlyList<T>
    {
        void Sort(Comparison<T> comparision);
    }

    internal static class ConcatListExtensions
    {
        private sealed class ConcatList<T> : List<T>, IConcatList<T>
        {
            public ConcatList(IConcatList<T> list1, IReadOnlyList<T> list2)
                : base(list1)
            {
                AddRange(list2);
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public IConcatList<T> Concat(IReadOnlyList<T> items)
            {
                AddRange(items);
                return this;
            }
        }

        internal static IConcatList<T> Concat<T>(this IConcatList<T> left, IConcatList<T> right)
        {
            Debug.Assert(left != null && right != null && right.Count > 0);

            if (left.Count == 0)
                return right;

            var result = left as ConcatList<T>;
            return result == null ? new ConcatList<T>(left, right) : result.Concat(right);
        }
    }

    internal static class ConcatList<T>
    {
        public static readonly IConcatList<T> Empty = new EmptyConcatList();

        private sealed class EmptyConcatList : IConcatList<T>
        {
            public T this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public int Count
            {
                get { return 0; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public void Sort(Comparison<T> comparison)
            {
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
