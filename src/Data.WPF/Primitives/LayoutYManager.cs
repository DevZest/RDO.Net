namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutYManager : LayoutXYManager
    {
        public LayoutYManager(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
        }

        protected override IGridTrackCollection GridTracksMain
        {
            get { return Template.InternalGridRows; }
        }

        protected override IGridTrackCollection GridTracksCross
        {
            get { return Template.InternalGridColumns; }
        }

        public override double ViewportX
        {
            get { return ViewportCross; }
        }

        public override double ViewportY
        {
            get { return ViewportMain; }
        }

        public override double ExtentX
        {
            get { return ExtentCross; }
        }

        public override double ExtentY
        {
            get { return ExtentMain; }
        }

        public override double ScrollOffsetX
        {
            get { return ScrollOffsetCross; }
            set { ScrollOffsetCross = value; }
        }

        public override double ScrollOffsetY
        {
            get { return ScrollOffsetMain; }
            set { ScrollOffsetMain = value; }
        }
    }
}
