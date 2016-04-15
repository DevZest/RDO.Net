using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutManagerZ : LayoutManager
    {
        internal LayoutManagerZ(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
            RefreshBlock();
        }

        private void RefreshBlock()
        {
            if (CurrentRow != null && Blocks.Count == 0)
                BlockViews.RealizeFirstUnpinned(CurrentRow.Ordinal);
        }

        protected override void OnSetState(DataPresenterState dataPresenterState)
        {
            base.OnSetState(dataPresenterState);
            if (dataPresenterState == DataPresenterState.CurrentRow)
            {
                BlockViews.VirtualizeAll();
                RefreshBlock();
            }
        }

        protected override void PrepareMeasureBlocks()
        {
            RefreshBlock();
            if (BlockViews.Count == 1)
                BlockViews[0].Measure(Size.Empty);  // Available size is ignored when preparing blocks
        }

        internal override Size GetMeasuredSize(DataItem dataItem, int blockDimension)
        {
            return dataItem.GridRange.MeasuredSize;
        }

        internal override Rect GetArrangeRect(DataItem dataItem, int blockDimension)
        {
            var gridRange = dataItem.GridRange;
            return new Rect(gridRange.MeasuredPoint, gridRange.MeasuredSize);
        }

        protected override void FinalizeMeasureBlocks()
        {
            if (BlockViews.Count == 1)
                BlockViews[0].Measure(Template.BlockRange.MeasuredSize);
        }

        protected override Size MeasuredSize
        {
            get { return Template.Range().MeasuredSize; }
        }
    }
}
