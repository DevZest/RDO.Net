using DevZest.Data.Windows.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    public static class ScalarSet
    {
        private class EmptyScalarSet : IScalarSet
        {
            public bool Contains(Scalar scalar)
            {
                return false;
            }

            public int Count
            {
                get { return 0; }
            }

            public Scalar this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public IEnumerator<Scalar> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield break;
            }
        }

        private class ListSclarSet : List<Scalar>, IScalarSet
        {
            public ListSclarSet(IList<Scalar> scalars)
                : base(scalars)
            {
            }
        }

        public static readonly IScalarSet Empty = new EmptyScalarSet();

        public static IScalarSet New(params Scalar[] scalars)
        {
            return New(scalars, false);
        }

        private static IScalarSet New(IList<Scalar> scalars, bool isNormalized)
        {
            if (scalars == null || scalars.Count == 0)
                return Empty;

            if (scalars.Count == 1)
            {
                var column = scalars[0];
                return column == null ? Empty : column;
            }

            if (isNormalized)
                return new ListSclarSet(scalars);

            return New(Normalize(scalars), true);
        }

        private static IList<Scalar> Normalize(IList<Scalar> scalars)
        {
            Debug.Assert(scalars.Count > 1);

            List<Scalar> result = null;
            for (int i = 0; i < scalars.Count; i++)
            {
                var column = scalars[i];
                if (column == null)
                {
                    result = CopyFrom(scalars, i);
                    continue;
                }

                if (result != null)
                {
                    if (!result.Contains(column))
                        result.Add(column);
                    continue;
                }

                for (int j = 0; j < i; j++)
                {
                    if (column == scalars[j])
                    {
                        result = CopyFrom(scalars, i);
                        continue;
                    }
                }
            }

            return result == null ? scalars : result;
        }

        private static List<Scalar> CopyFrom(IList<Scalar> scalars, int count)
        {
            var result = new List<Scalar>();
            for (int i = 0; i < count; i++)
                result.Add(scalars[i]);

            return result;
        }

        public static IScalarSet Merge(this IScalarSet scalarSet, IScalarSet value)
        {
            if (scalarSet == null)
                throw new ArgumentNullException(nameof(scalarSet));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var count1 = scalarSet.Count;
            if (count1 == 0)
                return value;
            var count2 = value.Count;
            if (count2 == 0)
                return scalarSet;

            return count1 >= count2 ? DoMerge(scalarSet, value) : DoMerge(value, scalarSet);
        }

        private static IScalarSet DoMerge(IScalarSet x, IScalarSet y)
        {
            Debug.Assert(x.Count >= y.Count);

            for (int i = 0; i < y.Count; i++)
            {
                if (!x.Contains(y[i]))
                    return DoMerge(x, y, i);
            }
            return x;
        }

        private static IScalarSet DoMerge(IScalarSet x, IScalarSet y, int startYIndex)
        {
            var result = new List<Scalar>(x);
            result.Add(y[startYIndex]);
            for (int i = startYIndex + 1; i < y.Count; i++)
            {
                if (!result.Contains(y[i]))
                    result.Add(y[i]);
            }
            return new ListSclarSet(result);
        }
    }
}
