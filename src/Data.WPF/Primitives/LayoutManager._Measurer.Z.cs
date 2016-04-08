using System;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutManager
    {
        partial class _Measurer
        {
            private sealed class Z : _Measurer
            {
                public Z(LayoutManager layoutManager)
                    : base(layoutManager)
                {
                }

                public override int CoerceBlockDimensions()
                {
                    return 1;
                }
            }
        }
    }
}
