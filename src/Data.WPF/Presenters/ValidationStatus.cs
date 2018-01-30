namespace DevZest.Data.Presenters
{
    public enum ValidationStatus
    {
        Invisible = 0,
        FlushingError,
        DataError,
        Validating,
        Validated
    }
}
