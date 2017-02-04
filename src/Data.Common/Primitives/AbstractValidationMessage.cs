using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public abstract class AbstractValidationMessage : IAbstractValidationMessageGroup
    {
        public abstract string Id { get; }

        public abstract string Description { get; }

        public abstract ValidationSeverity Severity { get; }

        public override string ToString()
        {
            return Description;
        }

        #region IAbstractValidationMessageGroup

        bool IAbstractValidationMessageGroup.IsSealed
        {
            get { return true; }
        }

        int IReadOnlyCollection<AbstractValidationMessage>.Count
        {
            get { return 1; }
        }

        AbstractValidationMessage IReadOnlyList<AbstractValidationMessage>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        IAbstractValidationMessageGroup IAbstractValidationMessageGroup.Seal()
        {
            return this;
        }

        IAbstractValidationMessageGroup IAbstractValidationMessageGroup.Add(AbstractValidationMessage value)
        {
            Check.NotNull(value, nameof(value));
            return AbstractValidationMessageGroup.New(this, value);
        }

        IEnumerator<AbstractValidationMessage> IEnumerable<AbstractValidationMessage>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        #endregion
    }
}
