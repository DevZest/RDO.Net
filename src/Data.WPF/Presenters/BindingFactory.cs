namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static T WithSerializableColumns<T>(this T rowBinding, params Column[] columns)
            where T : RowBinding
        {
            rowBinding.SerializableColumns = columns.VerifyNotNull(nameof(columns)).VerifyNoNullItem(nameof(columns));
            return rowBinding;
        }
    }
}
