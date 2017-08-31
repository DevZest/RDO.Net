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
    public static class DataRowValidationResults
    {
        private sealed class EmptyResults : IDataRowValidationResults
        {
            public static EmptyResults Singleton = new EmptyResults();
            private EmptyResults()
            {
            }

            public DataRowValidationResult this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public IColumnValidationMessages this[DataRow key]
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

            public IDataRowValidationResults Seal()
            {
                return this;
            }

            public IDataRowValidationResults Add(DataRowValidationResult validationEntry)
            {
                IDataRowValidationResults result = new KeyedCollection();
                return result.Add(validationEntry);
            }

            public bool Contains(DataRow dataRow)
            {
                Check.NotNull(dataRow, nameof(dataRow));
                return false;
            }

            public bool TryGetValue(DataRow key, out IColumnValidationMessages value)
            {
                value = null;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield break;
            }

            IEnumerator<DataRowValidationResult> IEnumerable<DataRowValidationResult>.GetEnumerator()
            {
                yield break;
            }

            public override string ToString()
            {
                return this.ToJsonString(true);
            }
        }

        private sealed class KeyedCollection : KeyedCollection<DataRow, DataRowValidationResult>, IDataRowValidationResults
        {
            protected override DataRow GetKeyForItem(DataRowValidationResult item)
            {
                return item.DataRow;
            }

            public bool IsSealed { get; private set; }

            IColumnValidationMessages IDataRowValidationResults.this[DataRow dataRow]
            {
                get { return base[dataRow].Messages; }
            }

            public IDataRowValidationResults Seal()
            {
                IsSealed = true;
                return this;
            }

            public override string ToString()
            {
                return this.ToJsonString(true);
            }

            IDataRowValidationResults IDataRowValidationResults.Add(DataRowValidationResult value)
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

            public bool TryGetValue(DataRow dataRow, out IColumnValidationMessages value)
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

        public static IDataRowValidationResults Empty
        {
            get { return EmptyResults.Singleton; }
        }

        public static IDataRowValidationResults ParseJson(DataSet dataSet, string json)
        {
            var jsonParser = new JsonParser(json);
            var result = jsonParser.ParseValidationResult(dataSet);
            jsonParser.ExpectToken(JsonTokenKind.Eof);
            return result;
        }

        public static string ToJsonString(this IDataRowValidationResults validationResults, bool isPretty)
        {
            return JsonWriter.New().Write(validationResults).ToString(isPretty);
        }

        public static bool IsValid(this IDataRowValidationResults validationResults)
        {
            return !validationResults.HasError();
        }

        public static bool HasError(this IDataRowValidationResults validationResults)
        {
            return validationResults.Any(ValidationSeverity.Error);
        }

        public static bool HasWarning(this IDataRowValidationResults validationResults)
        {
            return validationResults.Any(ValidationSeverity.Warning);
        }

        private static bool Any(this IDataRowValidationResults validationResults, ValidationSeverity severity)
        {
            for (int i = 0; i < validationResults.Count; i++)
            {
                var messages = validationResults[i].Messages;
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
