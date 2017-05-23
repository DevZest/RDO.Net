using DevZest.Data.Utilities;
using System;
using System.Diagnostics;

namespace DevZest.Data
{
    public abstract class DataRowSort
    {
        internal static DataRowSort Create<T>(Func<T, DataRow, DataRow, int> comparer)
            where T : Model
        {
            Debug.Assert(comparer != null && comparer.Target == null);
            return new DelegateSort<T>(comparer);
        }

        internal static DataRowSort Create<T>(Func<DataRowComparing, T, DataRowCompared> comparer)
            where T : Model
        {
            Debug.Assert(comparer != null && comparer.Target == null);
            return new FluentDelegateSort<T>(comparer);
        }

        private DataRowSort()
        {
        }

        public abstract int Evaluate(DataRow x, DataRow y);

        public abstract Type ModelType { get; }

        private sealed class DelegateSort<T> : DataRowSort
            where T : Model
        {
            public DelegateSort(Func<T, DataRow, DataRow, int> comparer)
            {
                Debug.Assert(comparer != null && comparer.Target == null);
                _comparer = comparer;
            }

            private readonly Func<T, DataRow, DataRow, int> _comparer;

            public override int Evaluate(DataRow x, DataRow y)
            {
                Check.NotNull(x, nameof(x));
                Check.NotNull(y, nameof(y));

                var _ = (T)x.Model;
                return _comparer(_, x, y);
            }

            public override Type ModelType
            {
                get { return typeof(T); }
            }
        }

        private sealed class FluentDelegateSort<T> : DataRowSort
            where T : Model
        {
            public FluentDelegateSort(Func<DataRowComparing, T, DataRowCompared> comparer)
            {
                Debug.Assert(comparer != null && comparer.Target == null);
                _comparer = comparer;
            }

            private readonly Func<DataRowComparing, T, DataRowCompared> _comparer;

            public override int Evaluate(DataRow x, DataRow y)
            {
                Check.NotNull(x, nameof(x));
                Check.NotNull(y, nameof(y));

                var _ = (T)x.Model;
                return _comparer(new DataRowComparing(x, y), _).Result;
            }

            public override Type ModelType
            {
                get { return typeof(T); }
            }
        }
    }
}
