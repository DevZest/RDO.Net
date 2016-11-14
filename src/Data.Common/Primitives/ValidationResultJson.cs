namespace DevZest.Data.Primitives
{
    public static class ValidationResultJson
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, ValidationResult validationResult)
        {
            validationResult.WriteJson(jsonWriter);
            return jsonWriter;
        }
    }
}
