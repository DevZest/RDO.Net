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
            Ordinal = ordinal;
            Length = length;
        }

        public DataSetControl DataSetControl { get; private set; }

        internal int Ordinal { get; private set; }

        public DataGridLength Length { get; private set; }

        internal void Clear()
        {
            DataSetControl = null;
            Ordinal = 0;
            Length = new DataGridLength();
        }
    }
}
