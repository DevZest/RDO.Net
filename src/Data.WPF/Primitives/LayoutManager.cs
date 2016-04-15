using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract partial class LayoutManager : ElementManager
    {
        internal static LayoutManager Create(DataPresenter dataPresenter)
        {
            var result = LayoutManager.Create(dataPresenter.Template, dataPresenter.DataSet);
            result.DataPresenter = dataPresenter;
            return result;
        }

        internal static LayoutManager Create(Template template, DataSet dataSet)
        {
            if (!template.Orientation.HasValue)
                return new LayoutManagerZ(template, dataSet);
            else
                return new LayoutManagerXY(template, dataSet);
        }

        protected LayoutManager(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
        }

        internal DataPresenter DataPresenter { get; private set; }

        private TemplateItemCollection<DataItem> DataItems
        {
            get { return Template.InternalDataItems; }
        }

        private TemplateItemCollection<BlockItem> BlockItems
        {
            get { return Template.InternalBlockItems; }
        }

        private TemplateItemCollection<RowItem> RowItems
        {
            get { return Template.InternalRowItems; }
        }

        public Size Measure(Size availableSize)
        {
            Template.InitMeasure(availableSize);
            BlockDimensions = Template.CoerceBlockDimensions();
            MeasureAutoSizeDataItems();
            IsPreparingMeasure = true;
            PrepareMeasure();
            IsPreparingMeasure = false;
            return FinalizeMeasure();
        }

        private void MeasureAutoSizeDataItems()
        {
            foreach (var dataItem in DataItems.AutoSizeItems)
            {
                Debug.Assert(dataItem.BlockDimensions == 1, "Auto size is not allowed with multidimensional DataItem.");
                var element = dataItem[0];
                element.Measure(dataItem.AvailableAutoSize);
                dataItem.UpdateAutoSize(element.DesiredSize, null);
            }
        }

        private bool IsPreparingMeasure;

        protected abstract void PrepareMeasure();

        internal Size Measure(BlockView blockView, Size constraintSize)
        {
            if (IsPreparingMeasure)
                PrepareMeasure(blockView);
            else
                FinalizeMeasure(blockView);
            return constraintSize;
        }

        private void PrepareMeasure(BlockView blockView)
        {
            foreach (var blockItem in BlockItems.AutoSizeItems)
            {
                var element = blockView[blockItem];
                element.Measure(blockItem.AvailableAutoSize);
                blockItem.UpdateAutoSize(element.DesiredSize, blockView);
            }

            for (int i = 0; i < blockView.Count; i++)
                blockView[i].View.Measure(Size.Empty);
        }

        private void FinalizeMeasure(BlockView blockView)
        {
            foreach (var blockItem in BlockItems)
            {
                var element = blockView[blockItem];
                element.Measure(blockItem.GridRange.GetMeasuredSize(blockView));
            }

            if (blockView.Count > 0)
            {
                var size = Template.RowRange.GetMeasuredSize(blockView);
                for (int i = 0; i < blockView.Count; i++)
                    blockView[i].View.Measure(size);
            }
        }

        internal Size Measure(RowPresenter row, Size constraintSize)
        {
            if (IsPreparingMeasure)
                PrepareMeasure(row);
            else
                FinalizeMeasure(row);
            return constraintSize;
        }

        private void PrepareMeasure(RowPresenter row)
        {
            if (RowItems.AutoSizeItems.Count == 0)
                return;

            var blockView = BlockViews[row];
            Debug.Assert(blockView != null);
            foreach (var rowItem in RowItems.AutoSizeItems)
            {
                var element = row.Elements[rowItem.Ordinal];
                element.Measure(rowItem.AvailableAutoSize);
                rowItem.UpdateAutoSize(element.DesiredSize, blockView);
            }
        }

        private void FinalizeMeasure(RowPresenter row)
        {
            if (RowItems.Count == 0)
                return;

            var blockView = BlockViews[row];
            Debug.Assert(blockView != null);
            foreach (var rowItem in RowItems)
            {
                var element = row.Elements[rowItem.Ordinal];
                element.Measure(rowItem.GridRange.GetMeasuredSize(blockView));
            }
        }

        private Size FinalizeMeasure()
        {
            foreach (var dataItem in DataItems)
            {
                for (int i = 0; i < BlockDimensions; i++)
                {
                    var element = dataItem[0];
                    element.Measure(dataItem.GridRange.GetMeasuredSize(null));
                }
            }

            if (BlockViews.Count > 0)
            {
                var blockRange = Template.BlockRange;
                for (int i = 0; i < BlockViews.Count; i++)
                {
                    var blockView = BlockViews[i];
                    blockView.Measure(blockRange.GetMeasuredSize(blockView));
                }
            }

            return MeasuredSize;
        }

        protected abstract Size MeasuredSize { get; }

        internal Size Arrange(Size finalSize)
        {
            throw new NotImplementedException();
        }
    }
}
