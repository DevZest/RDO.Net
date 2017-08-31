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

            public IColumnValidationMessages this[RowPresenter key]
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

            public IEnumerable<IColumnValidationMessages> Values
            {
                get { yield break; }
            }

            public IRowValidationResults Add(RowPresenter rowPresenter, IColumnValidationMessages validationMessages)
            {
                var result = new Dictionary();
                result.Add(rowPresenter, validationMessages);
                return result;
            }

            public bool ContainsKey(RowPresenter key)
            {
                return false;
            }

            public IEnumerator<KeyValuePair<RowPresenter, IColumnValidationMessages>> GetEnumerator()
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

            public bool TryGetValue(RowPresenter key, out IColumnValidationMessages value)
            {
                value = null;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield break;
            }
        }

        private sealed class Dictionary : Dictionary<RowPresenter, IColumnValidationMessages>, IRowValidationResults
        {
            public bool IsSealed { get; private set; }

            public IRowValidationResults Seal()
            {
                IsSealed = true;
                return this;
            }

            IRowValidationResults IRowValidationResults.Add(RowPresenter rowPresenter, IColumnValidationMessages validationMessages)
            {
                if (rowPresenter == null)
                    throw new ArgumentNullException(nameof(rowPresenter));
                if (validationMessages == null || validationMessages.Count == 0)
                    throw new ArgumentNullException(nameof(validationMessages));

                if (!IsSealed)
                {
                    base.Add(rowPresenter, validationMessages);
                    return this;
                }

                IRowValidationResults result = new Dictionary();
                foreach (var keyValuePair in this)
                    result = result.Add(keyValuePair.Key, keyValuePair.Value);
                result = result.Add(rowPresenter, validationMessages);
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

        public static IColumnValidationMessages GetValidationMessages(this IRowValidationResults directory, RowPresenter rowPresenter)
        {
            if (rowPresenter == null)
                return null;
            return directory.ContainsKey(rowPresenter) ? directory[rowPresenter] : ColumnValidationMessages.Empty;
        }

        internal static IRowValidationResults Where(this IRowValidationResults directory, ValidationSeverity severity)
        {
            var result = RowValidationResults.Empty;
            foreach (var rowPresenter in directory.Keys)
            {
                var messages = directory[rowPresenter];
                var filteredMessages = ColumnValidationMessages.Empty;
                for (int i = 0; i < messages.Count; i++)
                {
                    var message = messages[i];
                    if (message.Severity == severity)
                        filteredMessages = filteredMessages.Add(message);
                }
                if (filteredMessages.Count > 0)
                    result = result.Add(rowPresenter, filteredMessages);
            }
            return result.Seal();
        }
    }
}
