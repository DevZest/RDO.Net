using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest
{
    internal interface IConcatList<out T> : IReadOnlyList<T>
    {
        bool IsSealed { get; }
        IConcatList<T> Seal();
        IConcatList<T> Sort(Comparison<T> comparision);
    }

    internal static class ConcatListExtensions
    {
        private sealed class ConcatList<T> : List<T>, IConcatList<T>
        {
            private bool _isSealed;
            
            public ConcatList(IConcatList<T> from)
                : base(from)
            {
            }

            public ConcatList(IConcatList<T> list1, IReadOnlyList<T> list2)
                : base(list1)
            {
                AddRange(list2);
            }

            public bool IsSealed
            {
                get { return _isSealed; }
            }

            public IConcatList<T> Seal()
            {
                _isSealed = true;
                return this;
            }

            public IConcatList<T> Concat(IReadOnlyList<T> items)
            {
                if (items == null || items.Count == 0)
                    return this;

                if (_isSealed)
                {
                    AddRange(items);
                    return this;
                }
                else
                    return new ConcatList<T>(this, items);
            }

            IConcatList<T> IConcatList<T>.Sort(Comparison<T> comparision)
            {
                if (!_isSealed)
                {
                    Sort(comparision);
                    return this;
                }
                else
                {
                    var result = new ConcatList<T>(this);
                    result.Sort(comparision);
                    return result;
                }
            }
        }

        internal static IConcatList<T> Concat<T>(this IConcatList<T> left, IConcatList<T> right)
        {
            Debug.Assert(left != null && right != null);

            if (left.Count == 0)
                return right;
            if (right.Count == 0)
                return left;

            var result = left as ConcatList<T>;
            return result == null || result.IsSealed ? new ConcatList<T>(left, right) : result.Concat(right);
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

            public bool IsSealed
            {
                get { return true; }
            }

            public IConcatList<T> Seal()
            {
                return this;
            }

            public int Count
            {
                get { return 0; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public IConcatList<T> Sort(Comparison<T> comparison)
            {
                return this;
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
