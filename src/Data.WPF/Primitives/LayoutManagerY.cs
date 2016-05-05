namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutManagerY : LayoutManagerXY
    {
        public LayoutManagerY(Template template, DataSet dataSet)
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

        public override double OffsetX
        {
            get { return OffsetCross; }
            set { OffsetCross = value; }
        }

        public override double OffsetY
        {
            get { return OffsetMain; }
            set { OffsetMain = value; }
        }
    }
}
