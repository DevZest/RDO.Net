using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public static class ValidationResult
    {
        private sealed class EmptyResult : IValidationResult
        {
            public static EmptyResult Singleton = new EmptyResult();
            private EmptyResult()
            {
            }

            public ValidationEntry this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public IValidationMessageGroup this[DataRow key]
            {
                get { throw new ArgumentOutOfRangeException(nameof(key)); }
            }

            public int Count
            {
                get { return 0; }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IValidationResult Seal()
            {
                return this;
            }

            public IValidationResult Add(ValidationEntry validationEntry)
            {
                IValidationResult result = new KeyedCollection();
                return result.Add(validationEntry);
            }

            public bool Contains(DataRow dataRow)
            {
                Check.NotNull(dataRow, nameof(dataRow));
                return false;
            }

            public bool TryGetValue(DataRow key, out IValidationMessageGroup value)
            {
                value = null;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield break;
            }

            IEnumerator<ValidationEntry> IEnumerable<ValidationEntry>.GetEnumerator()
            {
                yield break;
            }

            public override string ToString()
            {
                return this.ToJsonString(true);
            }
        }

        private sealed class KeyedCollection : KeyedCollection<DataRow, ValidationEntry>, IValidationResult
        {
            protected override DataRow GetKeyForItem(ValidationEntry item)
            {
                return item.DataRow;
            }

            public bool IsSealed { get; private set; }

            IValidationMessageGroup IValidationResult.this[DataRow dataRow]
            {
                get { return base[dataRow].Messages; }
            }

            public IValidationResult Seal()
            {
                IsSealed = true;
                return this;
            }

            public override string ToString()
            {
                return this.ToJsonString(true);
            }

            IValidationResult IValidationResult.Add(ValidationEntry value)
            {
                if (value.IsEmpty)
                    throw new ArgumentException("", nameof(value));

                if (!IsSealed)
                {
                    base.Add(value);
                    return this;
                }

                var result = new KeyedCollection();
                foreach (var entry in this)
                    result.Add(entry);
                result.Add(value);
                return result;
            }

            public bool TryGetValue(DataRow dataRow, out IValidationMessageGroup value)
            {
                if (Contains(dataRow))
                {
                    value = this[dataRow].Messages;
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
        }

        public static IValidationResult Empty
        {
            get { return EmptyResult.Singleton; }
        }

        public static IValidationResult ParseJson(DataSet dataSet, string json)
        {
            var jsonParser = new JsonParser(json);
            var result = jsonParser.ParseValidationResult(dataSet);
            jsonParser.ExpectToken(JsonTokenKind.Eof);
            return result;
        }

        public static string ToJsonString(this IValidationResult validationResult, bool isPretty)
        {
            return JsonWriter.New().Write(validationResult).ToString(isPretty);
        }

        public static bool IsValid(this IValidationResult validationResult)
        {
            return !validationResult.HasError();
        }

        public static bool HasError(this IValidationResult validationResult)
        {
            return validationResult.Any(ValidationSeverity.Error);
        }

        public static bool HasWarning(this IValidationResult validationResult)
        {
            return validationResult.Any(ValidationSeverity.Warning);
        }

        private static bool Any(this IValidationResult validationResult, ValidationSeverity severity)
        {
            for (int i = 0; i < validationResult.Count; i++)
            {
                var messages = validationResult[i].Messages;
                for (int j = 0; j < messages.Count; j++)
                {
                    if (messages[j].Severity == severity)
                        return true;
                }
            }
            return false;
        }
    }
}
