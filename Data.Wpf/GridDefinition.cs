using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public abstract class GridDefinition
    {
        internal GridDefinition(GridTemplate owner, int ordinal, GridLength length)
        {
            Owner = owner;
            Ordinal = ordinal;
            Length = length;
        }

        internal GridTemplate Owner { get; private set; }

        internal int Ordinal { get; private set; }

        public GridLength Length { get; private set; }

        internal void Clear()
        {
            Owner = null;
            Ordinal = 0;
            Length = new GridLength();
        }
    }
}
