using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                return new Z(template, dataSet);
            else
                return new XY(template, dataSet);
        }

        private LayoutManager(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
        }

        internal DataPresenter DataPresenter { get; set; }

        private TemplateItemCollection<DataItem> DataItems
        {
            get { return Template.InternalDataItems; }
        }

        public Size Measure(Size availableSize)
        {
            Template.InitMeasure(availableSize);
            BlockDimensions = Template.CoerceBlockDimensions();
            MeasureAutoSizeDataItems();
            FillBlocks();
            throw new NotImplementedException();
        }

        private void MeasureAutoSizeDataItems()
        {
            foreach (var dataItem in DataItems.AutoSizeItems)
            {
                Debug.Assert(dataItem.BlockDimensions == 1, "Auto size is not allowed with multidimensional DataItem.");
                var element = dataItem[0];
                element.Measure(dataItem.AvailableAutoSize);
                dataItem.UpdateAutoSize(element.DesiredSize);
            }
        }

        private void FillBlocks()
        {
        }

        internal Size Arrange(Size finalSize)
        {
            throw new NotImplementedException();
        }
    }
}
