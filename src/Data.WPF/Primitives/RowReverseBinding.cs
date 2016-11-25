namespace DevZest.Data.Windows.Primitives
{
    public abstract class RowReverseBinding : ReverseBinding
    {
        internal abstract IColumnSet Columns { get; }
    }
}
