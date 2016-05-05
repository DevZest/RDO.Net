namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutXManager : LayoutXYManager
    {
        public LayoutXManager(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
        }

        protected override IGridTrackCollection GridTracksMain
        {
            get { return Template.InternalGridColumns; }
        }

        protected override IGridTrackCollection GridTracksCross
        {
            get { return Template.InternalGridRows; }
        }

        public override double ViewportX
        {
            get { return ViewportMain; }
        }

        public override double ViewportY
        {
            get { return ViewportCross; }
        }

        public override double ExtentX
        {
            get { return ExtentMain; }
        }

        public override double ExtentY
        {
            get { return ExtentCross; }
        }

        public override double OffsetX
        {
            get { return OffsetMain; }
            set { OffsetMain = value; }
        }

        public override double OffsetY
        {
            get { return OffsetCross; }
            set { OffsetCross = value; }
        }
    }
}
