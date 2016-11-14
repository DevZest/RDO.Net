namespace DevZest.Data.Primitives
{
    public static class ValidationEntryJson
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, ValidationEntry validationEntry)
        {
            validationEntry.WriteJson(jsonWriter);
            return jsonWriter;
        }
    }
}
