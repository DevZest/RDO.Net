using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Encapsulates DataRow and its validation errors.
    /// </summary>
    public struct DataValidationResult
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DataValidationResult"/>.
        /// </summary>
        /// <param name="dataRow">The DataRow.</param>
        /// <param name="errors">The validation errors of the DataRow.</param>
        public DataValidationResult(DataRow dataRow, IDataValidationErrors errors)
        {
            dataRow.VerifyNotNull(nameof(dataRow));
            errors.VerifyNotNull(nameof(errors));
            if (errors.Count == 0)
                throw new ArgumentException(DiagnosticMessages.ValidationEntry_EmptyMessages, nameof(errors));
            DataRow = dataRow;
            Errors = errors.Seal();
        }

        /// <summary>
        /// Gets the DataRow.
        /// </summary>
        public readonly DataRow DataRow;

        /// <summary>
        /// Gets the validation errors of the DataRow.
        /// </summary>
        public readonly IDataValidationErrors Errors;

        /// <summary>
        /// Gets a value indicating whether this struct is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return DataRow == null; }
        }

        /// <summary>
        /// Serializes this data validation result into JSON.
        /// </summary>
        /// <param name="isPretty">Determines whether serialized JSON should be indented.</param>
        /// <param name="customizer">The customizer for JSON serialization.</param>
        /// <returns></returns>
        public string ToJsonString(bool isPretty, IJsonCustomizer customizer = null)
        {
            return JsonWriter.Create(customizer).Write(this).ToString(isPretty);
        }

        /// <summary>
        /// Deserializes JSON string into <see cref="DataValidationResult"/>
        /// </summary>
        /// <param name="dataSet">The DataSet which contains the data for validation.</param>
        /// <param name="json">The input JSON string.</param>
        /// <param name="customizer">The customizer for JSON deserialization.</param>
        /// <returns>The deserialized object.</returns>
        public static DataValidationResult ParseJson(DataSet dataSet, string json, IJsonCustomizer customizer = null)
        {
            var jsonReader = JsonReader.Create(json, customizer);
            var result = jsonReader.ParseDataValidationResult(dataSet);
            jsonReader.ExpectEof();
            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToJsonString(true);
        }
    }
}
