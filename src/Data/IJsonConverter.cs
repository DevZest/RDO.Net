namespace DevZest.Data
{
    public interface IJsonConverter
    {
        JsonValue Serialize(Column column, int rowOrdinal);

        void Deserialize(Column column, int rowOrdinal, JsonValue value);
    }
}
