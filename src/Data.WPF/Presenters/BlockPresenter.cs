using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Contains block level presentation logic which can be consumed by view elements.
    /// </summary>
    public sealed class BlockPresenter : ElementPresenter, IReadOnlyList<RowPresenter>
    {
        internal BlockPresenter(Template template)
        {
            _template = template;
        }

        private readonly Template _template;
        /// <inheritdoc/>
        public sealed override Template Template
        {
            get { return _template; }
        }

        internal BlockView BlockView { get; set; }

        /// <summary>
        /// Gets the ordinal of the block.
        /// </summary>
        public int Ordinal
        {
            get { return BlockView == null ? -1 : BlockView.Ordinal; }
        }

        /// <summary>
        /// Gets <see cref="RowPresenter"/> at specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        /// <returns>The result <see cref="RowPresenter"/>.</returns>
        public RowPresenter this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return BlockView[index];
            }
        }

        /// <summary>
        /// Gets the count of contained rows.
        /// </summary>
        public int Count
        {
            get { return BlockView == null ? 0 : BlockView.Count; }
        }

        /// <summary>
        /// Returns an enumerator that lists all of the <see cref="RowPresenter"/> in this block.
        /// </summary>
        /// <returns>An enumerator that lists all of the <see cref="RowPresenter"/> in this block.</returns>
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
