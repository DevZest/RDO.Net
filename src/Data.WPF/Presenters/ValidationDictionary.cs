using DevZest.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public static class ValidationDictionary
    {
        private sealed class EmptyDictionary : IValidationDictionary
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

            public IValidationDictionary Add(RowPresenter rowPresenter, IColumnValidationMessages validationMessages)
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

            public IValidationDictionary Remove(RowPresenter rowPresenter)
            {
                return this;
            }

            public IValidationDictionary Seal()
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

        private sealed class Dictionary : Dictionary<RowPresenter, IColumnValidationMessages>, IValidationDictionary
        {
            public bool IsSealed { get; private set; }

            public IValidationDictionary Seal()
            {
                IsSealed = true;
                return this;
            }

            IValidationDictionary IValidationDictionary.Add(RowPresenter rowPresenter, IColumnValidationMessages validationMessages)
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

                IValidationDictionary result = new Dictionary();
                foreach (var keyValuePair in this)
                    result = result.Add(keyValuePair.Key, keyValuePair.Value);
                result = result.Add(rowPresenter, validationMessages);
                return result;               
            }

            IValidationDictionary IValidationDictionary.Remove(RowPresenter rowPresenter)
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

                IValidationDictionary result = new Dictionary();
                foreach (var keyValuePair in this)
                {
                    if (keyValuePair.Key != rowPresenter)
                        result = result.Add(keyValuePair.Key, keyValuePair.Value);
                }
                return result;

            }
        }

        public static IValidationDictionary Empty
        {
            get { return EmptyDictionary.Singleton; }
        }

        public static IColumnValidationMessages GetValidationMessages(this IValidationDictionary directory, RowPresenter rowPresenter)
        {
            if (rowPresenter == null)
                return null;
            return directory.ContainsKey(rowPresenter) ? directory[rowPresenter] : ColumnValidationMessages.Empty;
        }

        internal static IValidationDictionary Where(this IValidationDictionary directory, ValidationSeverity severity)
        {
            var result = ValidationDictionary.Empty;
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
