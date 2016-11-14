namespace DevZest.Data.Primitives
{
    public static class ValidationResultJson
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, ValidationResult validationResult)
        {
            return jsonWriter.WriteArray(validationResult.Entries, (writer, entry) => writer.Write(entry));
        }
    }
}
