using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class RowBinding : TwoWayBinding, IConcatList<RowBinding>
    {
        #region IConcatList<RowBinding>

        int IReadOnlyCollection<RowBinding>.Count
        {
            get { return 1; }
        }

        RowBinding IReadOnlyList<RowBinding>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        void IConcatList<RowBinding>.Sort(Comparison<RowBinding> comparision)
        {
        }

        IEnumerator<RowBinding> IEnumerable<RowBinding>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        #endregion

        internal abstract UIElement Setup(RowPresenter rowPresenter);

        internal sealed override void VerifyRowRange(GridRange rowRange)
        {
            if (!rowRange.Contains(GridRange))
                throw new InvalidOperationException(Strings.RowBinding_OutOfRowRange(Ordinal));
        }

        internal abstract void OnRowDisposed(RowPresenter rowPresenter);
    }
}
