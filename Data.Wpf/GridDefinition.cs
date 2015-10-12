using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public abstract class GridDefinition
    {
        internal GridDefinition(DataSetControl dataSetControl, int ordinal, DataGridLength length)
        {
            DataSetControl = dataSetControl;
            _ordinal = ordinal;
            Length = length;
        }

        public DataSetControl DataSetControl { get; private set; }
        private int _ordinal;

        public DataGridLength Length { get; private set; }

        internal void Clear()
        {
            DataSetControl = null;
            _ordinal = 0;
            Length = new DataGridLength();
        }
    }
}
