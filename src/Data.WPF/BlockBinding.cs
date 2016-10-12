using DevZest.Data.Windows.Primitives;
using System.Windows;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Windows
{
    public class BlockBinding<T> : BlockBindingBase<T>
        where T : UIElement, new()
    {
        public Action<T, int, IReadOnlyCollection<RowPresenter>> OnSetup { get; set; }

        protected sealed override void Setup(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
            if (OnSetup != null)
                OnSetup(element, blockOrdinal, rows);
        }

        public Action<T, int, IReadOnlyCollection<RowPresenter>> OnRefresh { get; set; }

        protected sealed override void Refresh(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
            if (OnRefresh != null)
                OnRefresh(element, blockOrdinal, rows);
        }

        public Action<T, int, IReadOnlyCollection<RowPresenter>> OnCleanup { get; set; }

        protected sealed override void Cleanup(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
            if (OnCleanup != null)
                OnCleanup(element, blockOrdinal, rows);
        }
    }
}
