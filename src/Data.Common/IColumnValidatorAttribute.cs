namespace DevZest.Data
{
    public interface IColumnValidatorAttribute
    {
        IValidator GetValidator(Column column);
    }
}
