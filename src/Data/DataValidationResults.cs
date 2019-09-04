using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;

namespace DevZest.Data
{
    /// <summary>
    /// Contains static methods to handle <see cref="IDataValidationResults"/> objects.
    /// </summary>
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

        /// <summary>
        /// Gets an empty <see cref="IDataValidationResults"/> dictionary.
        /// </summary>
        public static IDataValidationResults Empty
        {
            get { return EmptyResults.Singleton; }
        }

        /// <summary>
        /// Deserializes JSON string into <see cref="IDataValidationResults"/> object.
        /// </summary>
        /// <param name="dataSet">The DataSet which contains data for validation.</param>
        /// <param name="json">The JSON string.</param>
        /// <param name="customizer">The customizer for deserialization.</param>
        /// <returns>The deserialized object.</returns>
        public static IDataValidationResults ParseJson(DataSet dataSet, string json, IJsonCustomizer customizer = null)
        {
            var jsonReader = JsonReader.Create(json, customizer);
            var result = jsonReader.ParseDataValidationResults(dataSet);
            jsonReader.ExpectEof();
            return result;
        }

        /// <summary>
        /// Serializes <see cref="IDataValidationResults"/> dictionary into JSON string.
        /// </summary>
        /// <param name="validationResults">The <see cref="IDataValidationResults"/> dictionary.</param>
        /// <param name="isPretty">Determines whether serialized JSON string should be indented.</param>
        /// <param name="customizer">The customizer for JSON serialization.</param>
        /// <returns>The serialized JSON string.</returns>
        public static string ToJsonString(this IDataValidationResults validationResults, bool isPretty, IJsonCustomizer customizer = null)
        {
            return JsonWriter.Create(customizer).Write(validationResults).ToString(isPretty);
        }
    }
}
