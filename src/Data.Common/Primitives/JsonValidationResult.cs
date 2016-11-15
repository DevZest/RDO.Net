using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonValidationResult
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, ValidationResult validationResult)
        {
            return jsonWriter.WriteArray(validationResult.Entries, (writer, entry) => writer.Write(entry));
        }

        public static ValidationResult ParseValidationResult(this JsonParser jsonParser, DataSet dataSet)
        {
            IReadOnlyList<ValidationEntry> validationEntries = jsonParser.ParseValidationEntries(dataSet);
            return ValidationResult.New(validationEntries);
        }
    }
}
