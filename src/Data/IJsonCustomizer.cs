namespace DevZest.Data
{
    public interface IJsonCustomizer
    {
        bool IsSerializable(Column column);
        bool IsDeserializable(Column column);
        IJsonConverter GetConverter(Column column);
    }
}
