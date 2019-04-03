using DevZest.Data.Views;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<RowHeader> BindToRowHeader(this Model source)
        {
            return new RowBinding<RowHeader>(onRefresh: null);
        }
    }
}
