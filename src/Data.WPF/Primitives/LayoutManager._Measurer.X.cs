using System;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutManager
    {
        partial class _Measurer
        {
            private sealed class X : _Measurer
            {
                public X(LayoutManager layoutManager)
                    : base(layoutManager)
                {
                }

                public override int CoerceBlockDimensions()
                {
                    return Template.CoerceVerticalBlockDimensions();
                }
            }
        }
    }
}
