using System;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        [Flags]
        private enum AutoSizeDirection
        {
            None = 0,
            X = 0x1,
            Y = 0x2
        }
    }
}
