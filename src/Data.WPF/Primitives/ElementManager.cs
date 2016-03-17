using System;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ElementManager : RowManager
    {
        internal ElementManager(DataSet dataSet)
            : base(dataSet)
        {
        }

        internal sealed override void InvalidateView()
        {
        }
    }
}
