using DevZest.Data.Utilities;
using System.Collections.Generic;
using System.Collections;

namespace DevZest.Data
{
    public abstract class DataValidation : IReadOnlyList<Column>
    {
        protected DataValidation(object id)
        {
            Check.NotNull(id, nameof(id));
            Id = id;
        }

        public object Id { get; private set; }

        public IReadOnlyList<Column> DependentColumns
        {
            get { return this; }
        }

        protected abstract int DependentColumnsCount { get; }

        int IReadOnlyCollection<Column>.Count
        {
            get { return DependentColumnsCount; }
        }

        Column IReadOnlyList<Column>.this[int index]
        {
            get { return GetDependentColumn(index); }
        }

        protected abstract Column GetDependentColumn(int index);

        public abstract bool IsValid(DataRow dataRow);

        public abstract string GetErrorMessage(DataRow dataRow);

        IEnumerator<Column> IEnumerable<Column>.GetEnumerator()
        {
            for (int i = 0; i < DependentColumnsCount; i++)
                yield return GetDependentColumn(i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < DependentColumnsCount; i++)
                yield return GetDependentColumn(i);
        }
    }
}
