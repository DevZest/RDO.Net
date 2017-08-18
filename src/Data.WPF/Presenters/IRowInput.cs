namespace DevZest.Data.Presenters
{
    internal interface IRowInput
    {
        IAbstractValidationMessageGroup GetErrors(RowPresenter rowPresenter);
        IValidationMessageGroup GetWarnings(RowPresenter rowPresenter);
    }
}
