namespace DevZest.Data.Presenters
{
    public enum DataLoadState
    {
        Idle,
        Loading,
        Succeeded,
        Failed,
        Cancelling,
        Cancelled
    }
}
