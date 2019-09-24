using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Manipulates collection of scalar validation errors.
    /// </summary>
    public static class ScalarValidationErrors
    {
        private class EmptyGroup : IScalarValidationErrors
        {
            public static EmptyGroup Singleton = new EmptyGroup();
            private EmptyGroup()
            {
            }

            public int Count
            {
                get { return 0; }
            }

            public ScalarValidationError this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IScalarValidationErrors Add(ScalarValidationError value)
            {
                return value.VerifyNotNull(nameof(value));
            }

            public IScalarValidationErrors Seal()
            {
                return this;
            }

            public IEnumerator<ScalarValidationError> GetEnumerator()
            {
                return EmptyEnumerator<ScalarValidationError>.Singleton;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return EmptyEnumerator<ScalarValidationError>.Singleton;
            }
        }

        private class ListGroup : IScalarValidationErrors
        {
            private bool _isSealed;
            private List<ScalarValidationError> _list = new List<ScalarValidationError>();

            public ListGroup(ScalarValidationError value1, ScalarValidationError value2)
            {
                Debug.Assert(value1 != null && value2 != null);
                Add(value1);
                Add(value2);
            }

            private ListGroup()
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

            public ScalarValidationError this[int index]
            {
                get { return _list[index]; }
            }

            public IScalarValidationErrors Seal()
            {
                _isSealed = true;
                return this;
            }

            public IScalarValidationErrors Add(ScalarValidationError value)
            {
                value.VerifyNotNull(nameof(value));

                if (!IsSealed)
                {
                    _list.Add(value);
                    return this;
                }

                if (Count == 0)
                    return value;
                else
                {
                    var result = new ListGroup();
                    for (int i = 0; i < Count; i++)
                        result.Add(this[i]);
                    result.Add(value);
                    return result;
                }
            }

            public IEnumerator<ScalarValidationError> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the empty collection of scalar validation errors.
        /// </summary>
        public static IScalarValidationErrors Empty
        {
            get { return EmptyGroup.Singleton; }
        }

        internal static IScalarValidationErrors New(ScalarValidationError value1, ScalarValidationError value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new ListGroup(value1, value2);
        }

        /// <summary>
        /// Creates a collection of scalar validation errors.
        /// </summary>
        /// <param name="values">The scalar validation errors.</param>
        /// <returns>The collection of scalar validation errors.</returns>
        public static IScalarValidationErrors New(params ScalarValidationError[] values)
        {
            values.VerifyNotNull(nameof(values));

            if (values.Length == 0)
                return Empty;

            IScalarValidationErrors result = values.VerifyNotNull(0, nameof(values));
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values.VerifyNotNull(i, nameof(values)));
            return result;
        }

        /// <summary>
        /// Combines two <see cref="IScalarValidationErrors"/>.
        /// </summary>
        /// <param name="source">The source <see cref="IScalarValidationErrors"/>.</param>
        /// <param name="other">The other <see cref="IScalarValidationErrors"/>.</param>
        /// <returns>The result <see cref="IScalarValidationErrors"/>.</returns>
        public static IScalarValidationErrors Add(this IScalarValidationErrors source, IScalarValidationErrors other)
        {
            other.VerifyNotNull(nameof(other));

            for (int i = 0; i < other.Count; i++)
                source = source.Add(other.VerifyNotNull(i, nameof(other)));

            return source;
        }
    }
}
