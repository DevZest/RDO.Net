using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class RowBinding : Binding, IConcatList<RowBinding>
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

        internal abstract void FlushInput(UIElement element);

        internal bool ShouldRefresh(bool isReload, UIElement element)
        {
            return isReload ? true : ShouldRefresh(element);
        }

        internal abstract bool ShouldRefresh(UIElement element);

        internal abstract bool HasPreValidatorError { get; }

        internal abstract IValidationSource<Column> ValidationSource { get; }

        internal abstract void OnRowDisposed(RowPresenter rowPresenter);

        internal abstract bool HasAsyncValidator { get; }

        internal abstract void RunAsyncValidator(RowPresenter rowPresenter);
    }
}
