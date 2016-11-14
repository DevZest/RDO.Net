namespace DevZest.Data.Primitives
{
    public static class ValidationMessageJson
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, ValidationMessage validationMessage)
        {
            validationMessage.WriteJson(jsonWriter);
            return jsonWriter;
        }
    }
}
