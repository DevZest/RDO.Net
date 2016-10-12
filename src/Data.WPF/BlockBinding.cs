using DevZest.Data.Windows.Primitives;
using System.Windows;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Windows
{
    public sealed class BlockBinding<T> : BlockBindingBase<T>
        where T : UIElement, new()
    {
        private Action<T, int, IReadOnlyCollection<RowPresenter>> _onSetup;
        public Action<T, int, IReadOnlyCollection<RowPresenter>> OnSetup
        {
            get { return _onSetup; }
            set
            {
                VerifyNotSealed();
                _onSetup = value;
            }
        }

        protected sealed override void Setup(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
            if (OnSetup != null)
                OnSetup(element, blockOrdinal, rows);
        }

        private Action<T, int, IReadOnlyCollection<RowPresenter>> _onRefresh;
        public Action<T, int, IReadOnlyCollection<RowPresenter>> OnRefresh
        {
            get { return _onRefresh; }
            set
            {
                VerifyNotSealed();
                _onRefresh = value;
            }
        }

        protected sealed override void Refresh(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
            if (OnRefresh != null)
                OnRefresh(element, blockOrdinal, rows);
        }

        private Action<T, int, IReadOnlyCollection<RowPresenter>> _onCleanup;
        public Action<T, int, IReadOnlyCollection<RowPresenter>> OnCleanup
        {
            get { return _onCleanup; }
            set
            {
                VerifyNotSealed();
                _onCleanup = value;
            }
        }

        protected sealed override void Cleanup(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
            if (OnCleanup != null)
                OnCleanup(element, blockOrdinal, rows);
        }
    }
}
