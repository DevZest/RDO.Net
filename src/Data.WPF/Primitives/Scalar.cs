using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class Scalar : IScalarSet
    {
        Scalar IReadOnlyList<Scalar>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        int IReadOnlyCollection<Scalar>.Count
        {
            get { return 1; }
        }

        bool IScalarSet.Contains(Scalar scalar)
        {
            return scalar == this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator<Scalar> IEnumerable<Scalar>.GetEnumerator()
        {
            yield return this;
        }
    }
}
