using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public sealed class BlockPresenter : ElementPresenter, IReadOnlyList<RowPresenter>
    {
        internal BlockPresenter(Template template)
        {
            _template = template;
        }

        private readonly Template _template;
        public sealed override Template Template
        {
            get { return _template; }
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
