using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutManager
    {
        private abstract partial class _Measurer
        {
            public static _Measurer Create(LayoutManager layoutManager)
            {
                var orientation = layoutManager.Template.Orientation;
                if (!orientation.HasValue)
                    return new Z(layoutManager);
                else if (orientation.GetValueOrDefault() == Orientation.Horizontal)
                    return new X(layoutManager);
                else
                    return new Y(layoutManager);
            }

            protected _Measurer(LayoutManager layoutManager)
            {
                Debug.Assert(layoutManager != null);
                LayoutManager = layoutManager;
            }

            private readonly LayoutManager LayoutManager;

            private Template Template
            {
                get { return LayoutManager.Template; }
            }

            private TemplateItemCollection<DataItem> DataItems
            {
                get { return Template.InternalDataItems; }
            }

            public abstract int CoerceBlockDimensions();

            public void MeasureAutoSizeDataItems()
            {
                foreach (var dataItem in DataItems.AutoSizeItems)
                {
                    Debug.Assert(dataItem.BlockDimensions == 1, "Auto size is not allowed with multidimensional DataItem.");
                    var element = dataItem[0];
                    element.Measure(dataItem.AvailableAutoSize);
                    dataItem.UpdateAutoSize(element.DesiredSize);
                }
            }

            public void FitBlocks()
            {
                throw new NotImplementedException();
            }
        }
    }
}
