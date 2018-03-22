namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static T WithSerializableColumns<T>(this T rowBinding, params Column[] columns)
            where T : RowBinding
        {
            columns.CheckNotNull(nameof(columns));
            rowBinding.SerializableColumns = columns;
            return rowBinding;
        }
    }
}
