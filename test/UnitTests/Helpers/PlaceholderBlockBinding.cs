using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Windows.Helpers
{
    public class PlaceholderBlockBinding : BlockBindingBase<Placeholder>
    {
        public PlaceholderBlockBinding()
        {
        }

        public PlaceholderBlockBinding(double desiredWidth, double desiredHeight)
        {
            DesiredWidth = desiredWidth;
            DesiredHeight = desiredHeight;
        }

        public double DesiredWidth { get; private set; }

        public double DesiredHeight { get; private set; }

        protected override void Setup(Placeholder element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
            element.DesiredWidth = DesiredWidth;
            element.DesiredHeight = DesiredHeight;
        }

        public Action<Placeholder, int, IReadOnlyList<RowPresenter>> OnRefresh { get; set; }

        protected override void Refresh(Placeholder element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
            if (OnRefresh != null)
                OnRefresh(element, blockOrdinal, rows);
        }

        public Action<Placeholder, int, IReadOnlyList<RowPresenter>> OnCleanup { get; set; }

        protected override void Cleanup(Placeholder element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
            if (OnCleanup != null)
                OnCleanup(element, blockOrdinal, rows);
        }
    }
}
