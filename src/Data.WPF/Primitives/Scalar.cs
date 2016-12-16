using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class Scalar : IValidationSource<Scalar>
    {
        #region IValidationSource<Scalar>

        int IReadOnlyCollection<Scalar>.Count
        {
            get { return 1; }
        }

        bool IValidationSource<Scalar>.IsSealed
        {
            get { return true; }
        }

        IValidationSource<Scalar> IValidationSource<Scalar>.Add(Scalar value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return value == this ? this : ValidationSource<Scalar>.New(this, value);
        }

        IValidationSource<Scalar> IValidationSource<Scalar>.Clear()
        {
            return ValidationSource<Scalar>.Empty;
        }

        bool IValidationSource<Scalar>.Contains(Scalar scalar)
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

        IValidationSource<Scalar> IValidationSource<Scalar>.Remove(Scalar value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            return value == this ? ValidationSource<Scalar>.Empty : this;
        }

        IValidationSource<Scalar> IValidationSource<Scalar>.Seal()
        {
            return this;
        }

        #endregion
    }
}
