namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private abstract class DuplexRepeat : LayoutManager
        {
            public DuplexRepeat(DataSetPresenter presenter)
                : base(presenter)
            {
            }
        }
    }
}
