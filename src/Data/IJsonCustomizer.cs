namespace DevZest.Data
{
    public interface IJsonCustomizer
    {
        IJsonConverter GetConverter(Column column);
    }
}
