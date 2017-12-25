using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public static class ScalarAsyncValidators
    {
        private sealed class EmptyScalarAsyncValidators : IScalarAsyncValidators
        {
            public readonly static EmptyScalarAsyncValidators Singleton = new EmptyScalarAsyncValidators();

            private EmptyScalarAsyncValidators()
            {
            }

            public ScalarAsyncValidator this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public ScalarAsyncValidator this[IScalars sourceScalars]
            {
                get { return null; }
            }

            public int Count
            {
                get { return 0; }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IScalarAsyncValidators Add(ScalarAsyncValidator value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                return value;
            }

            public IEnumerator<ScalarAsyncValidator> GetEnumerator()
            {
                return EmptyEnumerator<ScalarAsyncValidator>.Singleton;
            }

            public IScalarAsyncValidators Seal()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class KeyedScalarAsyncValidators : IScalarAsyncValidators
        {
            private sealed class KeyedCollection : KeyedCollection<IScalars, ScalarAsyncValidator>
            {
                protected override IScalars GetKeyForItem(ScalarAsyncValidator item)
                {
                    return item.SourceScalars;
                }
            }

            private bool _isSealed;
            private KeyedCollection _list = new KeyedCollection();

            public KeyedScalarAsyncValidators(ScalarAsyncValidator value1, ScalarAsyncValidator value2)
            {
                Debug.Assert(value1 != null && value2 != null);
                Add(value1);
                Add(value2);
            }

            private KeyedScalarAsyncValidators()
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

            public ScalarAsyncValidator this[int index]
            {
                get { return _list[index]; }
            }

            public IScalarAsyncValidators Seal()
            {
                _isSealed = true;
                return this;
            }

            public IScalarAsyncValidators Add(ScalarAsyncValidator value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (!IsSealed)
                {
                    _list.Add(value);
                    return this;
                }

                Debug.Assert(Count > 0);
                var result = new KeyedScalarAsyncValidators();
                for (int i = 0; i < Count; i++)
                    result.Add(this[i]);
                result.Add(value);
                return result;
            }

            public ScalarAsyncValidator this[IScalars sourceScalars]
            {
                get { return _list.Contains(sourceScalars) ? _list[sourceScalars] : null; }
            }

            public IEnumerator<ScalarAsyncValidator> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        public static IScalarAsyncValidators Empty
        {
            get { return EmptyScalarAsyncValidators.Singleton; }
        }

        internal static IScalarAsyncValidators New(ScalarAsyncValidator value1, ScalarAsyncValidator value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new KeyedScalarAsyncValidators(value1, value2);
        }

        public static IScalarAsyncValidators New(params ScalarAsyncValidator[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (values.Length == 0)
                return Empty;

            IScalarAsyncValidators result = values[0].CheckNotNull(nameof(values), 0);
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values[i].CheckNotNull(nameof(values), 0));
            return result;
        }

        public static IScalarAsyncValidators Where(this IScalarAsyncValidators asyncValidators, Func<ScalarAsyncValidator, bool> predict)
        {
            if (asyncValidators == null)
                throw new ArgumentNullException(nameof(asyncValidators));

            var result = ScalarAsyncValidators.Empty;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                if (predict(asyncValidator))
                    result = result.Add(asyncValidator);
            }
            return result.Seal();
        }

        public static void Each(this IScalarAsyncValidators asyncValidators, Action<ScalarAsyncValidator> action)
        {
            if (asyncValidators == null)
                throw new ArgumentNullException(nameof(asyncValidators));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            for (int i = 0; i < asyncValidators.Count; i++)
                action(asyncValidators[i]);
        }
    }
}
