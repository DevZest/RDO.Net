using DevZest.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    internal static class RowValidationResults
    {
        private sealed class EmptyDictionary : IRowValidationResults
        {
            public static EmptyDictionary Singleton = new EmptyDictionary();

            private EmptyDictionary()
            {
            }

            public IDataValidationErrors this[RowPresenter key]
            {
                get { throw new KeyNotFoundException(); }
            }

            public int Count
            {
                get { return 0; }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IEnumerable<RowPresenter> Keys
            {
                get { yield break; }
            }

            public IEnumerable<IDataValidationErrors> Values
            {
                get { yield break; }
            }

            public IRowValidationResults Add(RowPresenter rowPresenter, IDataValidationErrors validationErrors)
            {
                var result = new Dictionary();
                result.Add(rowPresenter, validationErrors);
                return result;
            }

            public bool ContainsKey(RowPresenter key)
            {
                return false;
            }

            public IEnumerator<KeyValuePair<RowPresenter, IDataValidationErrors>> GetEnumerator()
            {
                yield break;
            }

            public IRowValidationResults Remove(RowPresenter rowPresenter)
            {
                return this;
            }

            public IRowValidationResults Seal()
            {
                return this;
            }

            public bool TryGetValue(RowPresenter key, out IDataValidationErrors value)
            {
                value = null;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield break;
            }
        }

        private sealed class Dictionary : Dictionary<RowPresenter, IDataValidationErrors>, IRowValidationResults
        {
            public bool IsSealed { get; private set; }

            public IRowValidationResults Seal()
            {
                IsSealed = true;
                return this;
            }

            IRowValidationResults IRowValidationResults.Add(RowPresenter rowPresenter, IDataValidationErrors validationErrors)
            {
                if (rowPresenter == null)
                    throw new ArgumentNullException(nameof(rowPresenter));
                if (validationErrors == null || validationErrors.Count == 0)
                    throw new ArgumentNullException(nameof(validationErrors));

                if (!IsSealed)
                {
                    base.Add(rowPresenter, validationErrors);
                    return this;
                }

                IRowValidationResults result = new Dictionary();
                foreach (var keyValuePair in this)
                    result = result.Add(keyValuePair.Key, keyValuePair.Value);
                result = result.Add(rowPresenter, validationErrors);
                return result;               
            }

            IRowValidationResults IRowValidationResults.Remove(RowPresenter rowPresenter)
            {
                if (!ContainsKey(rowPresenter))
                    return this;

                if (!IsSealed)
                {
                    base.Remove(rowPresenter);
                    return this;
                }

                if (Count == 1)
                    return Empty;

                IRowValidationResults result = new Dictionary();
                foreach (var keyValuePair in this)
                {
                    if (keyValuePair.Key != rowPresenter)
                        result = result.Add(keyValuePair.Key, keyValuePair.Value);
                }
                return result;

            }
        }

        public static IRowValidationResults Empty
        {
            get { return EmptyDictionary.Singleton; }
        }

        public static IDataValidationErrors GetValidationMessages(this IRowValidationResults directory, RowPresenter rowPresenter)
        {
            if (rowPresenter == null)
                return null;
            return directory.ContainsKey(rowPresenter) ? directory[rowPresenter] : DataValidationErrors.Empty;
        }
    }
}
