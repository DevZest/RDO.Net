using System;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutManager
    {
        partial class _Measurer
        {
            private sealed class Y : _Measurer
            {
                public Y(LayoutManager layoutManager)
                    : base(layoutManager)
                {
                }

                protected override int CoerceBlockDimensions()
                {
                    return Template.CoerceHorizontalBlockDimensions();
                }
            }
        }
    }
}
