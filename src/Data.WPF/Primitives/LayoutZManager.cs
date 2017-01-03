using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutZManager : LayoutManager
    {
        internal LayoutZManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, Func<IEnumerable<ValidationMessage<Scalar>>> validateScalars)
            : base(template, dataSet, where, orderBy, validateScalars, true)
        {
        }

        protected override void PrepareMeasureContainers()
        {
            if (CurrentContainerView != null)
                CurrentContainerView.Measure(Size.Empty);  // Available size is ignored when preparing blocks
        }

        protected override Point GetLocation(ScalarBinding scalarBinding, int blockDimension)
        {
            Debug.Assert(blockDimension == 0);
            return scalarBinding.GridRange.MeasuredLocation;
        }

        protected override Size GetSize(ScalarBinding scalarBinding)
        {
            return scalarBinding.GridRange.MeasuredSize;
        }

        internal override Thickness GetClip(ScalarBinding scalarBinding, int blockDimension)
        {
            return new Thickness();
        }

        protected override Point GetLocation(ContainerView containerView)
        {
            return Template.Range().GetLocation(Template.BlockRange);
        }

        protected override Size GetSize(ContainerView containerView)
        {
            return Template.BlockRange.MeasuredSize;
        }

        internal override Thickness GetClip(ContainerView containerView)
        {
            return new Thickness();
        }

        protected override Point GetLocation(BlockView blockView, BlockBinding blockBinding)
        {
            return Template.BlockRange.GetLocation(blockBinding.GridRange);
        }

        protected override Size GetSize(BlockView blockView, BlockBinding blockBinding)
        {
            return blockBinding.GridRange.MeasuredSize;
        }

        internal override Thickness GetClip(BlockView blockView, BlockBinding blockBinding)
        {
            return new Thickness();
        }

        protected override Point GetLocation(BlockView blockView, int blockDimension)
        {
            Debug.Assert(blockDimension == 0);
            return Template.BlockRange.GetLocation(Template.RowRange);
        }

        protected override Size GetSize(BlockView blockView, int blockDimension)
        {
            return Template.RowRange.MeasuredSize;
        }

        internal override Thickness GetClip(int blockDimension)
        {
            return new Thickness();
        }

        protected override Point GetLocation(RowView rowView, RowBinding rowBinding)
        {
            return Template.RowRange.GetLocation(rowBinding.GridRange);
        }

        protected override Size GetSize(RowView rowView, RowBinding rowBinding)
        {
            return rowBinding.GridRange.MeasuredSize;
        }

        internal override Thickness GetClip(RowView rowView, RowBinding rowBinding)
        {
            return new Thickness();
        }

        protected override Size MeasuredSize
        {
            get { return Template.Range().MeasuredSize; }
        }

        internal override IEnumerable<GridLineFigure> GridLineFigures
        {
            get
            {
                foreach (var gridLine in Template.GridLines)
                {
                    var startPoint = gridLine.StartGridPoint.ToPoint(Template);
                    var endPoint = gridLine.EndGridPoint.ToPoint(Template);
                    yield return new GridLineFigure(gridLine, startPoint, endPoint);
                }
            }
        }
    }
}
