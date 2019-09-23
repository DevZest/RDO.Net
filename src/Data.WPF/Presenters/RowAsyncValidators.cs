using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Manipulates collection of row async validators.
    /// </summary>
    public static class RowAsyncValidators
    {
        private sealed class EmptyRowAsyncValidators : IRowAsyncValidators
        {
            public readonly static EmptyRowAsyncValidators Singleton = new EmptyRowAsyncValidators();

            private EmptyRowAsyncValidators()
            {
            }

            public RowAsyncValidator this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public int Count
            {
                get { return 0; }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IRowAsyncValidators Add(RowAsyncValidator value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                return value;
            }

            public IEnumerator<RowAsyncValidator> GetEnumerator()
            {
                return EmptyEnumerator<RowAsyncValidator>.Singleton;
            }

            public RowAsyncValidator this[IColumns sourceColumns]
            {
                get { return null; }
            }

            public IRowAsyncValidators Seal()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class KeyedRowAsyncValidators : IRowAsyncValidators
        {
            private sealed class KeyedCollection : KeyedCollection<IColumns, RowAsyncValidator>
            {
                protected override IColumns GetKeyForItem(RowAsyncValidator item)
                {
                    return item.SourceColumns;
                }
            }

            private bool _isSealed;
            private KeyedCollection _list = new KeyedCollection();

            public KeyedRowAsyncValidators(RowAsyncValidator value1, RowAsyncValidator value2)
            {
                Debug.Assert(value1 != null && value2 != null);
                Add(value1);
                Add(value2);
            }

            private KeyedRowAsyncValidators()
            {
            }

            public bool IsSealed
            {
                get { return _isSealed; }
            }

            public int Count
            {
                get { return _list.Count; }
            }

            public RowAsyncValidator this[int index]
            {
                get { return _list[index]; }
            }

            public IRowAsyncValidators Seal()
            {
                _isSealed = true;
                return this;
            }

            public IRowAsyncValidators Add(RowAsyncValidator value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (!IsSealed)
                {
                    _list.Add(value);
                    return this;
                }

                Debug.Assert(Count > 0);
                var result = new KeyedRowAsyncValidators();
                for (int i = 0; i < Count; i++)
                    result.Add(this[i]);
                result.Add(value);
                return result;
            }

            public RowAsyncValidator this[IColumns sourceColumns]
            {
                get { return _list.Contains(sourceColumns) ? _list[sourceColumns] : null; }
            }

            public IEnumerator<RowAsyncValidator> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the empty collection of row async validators.
        /// </summary>
        public static IRowAsyncValidators Empty
        {
            get { return EmptyRowAsyncValidators.Singleton; }
        }

        internal static IRowAsyncValidators New(RowAsyncValidator value1, RowAsyncValidator value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new KeyedRowAsyncValidators(value1, value2);
        }

        /// <summary>
        /// Creates a collection of row async validators.
        /// </summary>
        /// <param name="values">The validators.</param>
        /// <returns>The collection of row async validators.</returns>
        public static IRowAsyncValidators New(params RowAsyncValidator[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (values.Length == 0)
                return Empty;

            IRowAsyncValidators result = values.VerifyNotNull(0, nameof(values));
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values.VerifyNotNull(i, nameof(values)));
            return result;
        }
    }
}
