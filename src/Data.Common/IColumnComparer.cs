namespace DevZest.Data
{
    public interface IColumnComparer : IDataRowComparer
    {
        Column GetColumn(Model model);
        SortDirection Direction { get; }
    }
}
