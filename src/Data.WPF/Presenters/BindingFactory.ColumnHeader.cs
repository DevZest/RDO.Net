using DevZest.Data.Views;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static ScalarBinding<ColumnHeader> AsColumnHeader(this Column column, object title = null)
        {
            return new ScalarBinding<ColumnHeader>(
                onRefresh: null,
                onCleanup: null,
                onSetup: e =>
                {
                    e.Column = column;
                    e.Content = title ?? column.DisplayName;
                });
        }
    }
}
