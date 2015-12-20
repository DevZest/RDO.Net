namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private abstract class DuplexOrientationLayout : LayoutManager
        {
            public DuplexOrientationLayout(DataSetPresenter presenter)
                : base(presenter)
            {
            }
        }
    }
}
