using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data.Windows
{
    public sealed class BlockPresenter : IReadOnlyList<RowPresenter>
    {
        internal BlockPresenter()
        {
        }

        internal BlockView BlockView { get; set; }

        public int Ordinal
        {
            get { return BlockView == null ? -1 : BlockView.Ordinal; }
        }

        public RowPresenter this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return BlockView[index];
            }
        }

        public int Count
        {
            get { return BlockView == null ? 0 : BlockView.Count; }
        }

        public IEnumerator<RowPresenter> GetEnumerator()
        {
            if (BlockView != null)
                return BlockView.GetEnumerator();
            else
                return EmptyEnumerator<RowPresenter>.Singleton;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
