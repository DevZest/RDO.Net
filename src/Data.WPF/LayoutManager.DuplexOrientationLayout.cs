namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private abstract class DuplexOrientationLayout : LayoutManager
        {
            public DuplexOrientationLayout(DataView view)
                : base(view)
            {
            }
        }
    }
}
