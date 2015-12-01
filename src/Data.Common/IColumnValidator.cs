
namespace DevZest.Data
{
    public interface IColumnValidator
    {
        IValidator GetValidator(Column column);
    }
}
