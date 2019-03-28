using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;

namespace DevZest.Data
{
    public static class DataValidationResults
    {
        private sealed class EmptyResults : IDataValidationResults
        {
            public static EmptyResults Singleton = new EmptyResults();
            private EmptyResults()
            {
            }

            public DataValidationResult this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public IDataValidationErrors this[DataRow key]
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

            public IDataValidationResults Seal()
            {
                return this;
            }

            public IDataValidationResults Add(DataValidationResult validationEntry)
            {
                IDataValidationResults result = new KeyedCollection();
                return result.Add(validationEntry);
            }

            public bool Contains(DataRow dataRow)
            {
                dataRow.VerifyNotNull(nameof(dataRow));
                return false;
            }

            public bool TryGetValue(DataRow key, out IDataValidationErrors value)
            {
                value = null;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield break;
            }

            IEnumerator<DataValidationResult> IEnumerable<DataValidationResult>.GetEnumerator()
            {
                yield break;
            }

            public override string ToString()
            {
                return this.ToJsonString(true);
            }
        }

        private sealed class KeyedCollection : KeyedCollection<DataRow, DataValidationResult>, IDataValidationResults
        {
            protected override DataRow GetKeyForItem(DataValidationResult item)
            {
                return item.DataRow;
            }

            public bool IsSealed { get; private set; }

            IDataValidationErrors IDataValidationResults.this[DataRow dataRow]
            {
                get { return base[dataRow].Errors; }
            }

            public IDataValidationResults Seal()
            {
                IsSealed = true;
                return this;
            }

            public override string ToString()
            {
                return this.ToJsonString(true);
            }

            IDataValidationResults IDataValidationResults.Add(DataValidationResult value)
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

            public bool TryGetValue(DataRow dataRow, out IDataValidationErrors value)
            {
                if (Contains(dataRow))
                {
                    value = this[dataRow].Errors;
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
        }

        public static IDataValidationResults Empty
        {
            get { return EmptyResults.Singleton; }
        }

        public static IDataValidationResults ParseJson(DataSet dataSet, string json)
        {
            var jsonReader = new JsonReader(json);
            var result = jsonReader.ParseDataValidationResults(dataSet);
            jsonReader.ExpectToken(JsonTokenKind.Eof);
            return result;
        }

        public static string ToJsonString(this IDataValidationResults validationResults, bool isPretty)
        {
            return JsonWriter.New().Write(validationResults).ToString(isPretty);
        }
    }
}
