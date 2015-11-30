using DevZest.Data.Utilities;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data
{
    public abstract class ValidationRule : IReadOnlyList<Column>
    {
        protected ValidationRule(object id)
        {
            Check.NotNull(id, nameof(id));
            Id = id;
        }

        public object Id { get; private set; }

        public IReadOnlyList<Column> Columns
        {
            get { return this; }
        }

        protected abstract int ColumnsCount { get; }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Functionality exposed via property Columns.")]
        int IReadOnlyCollection<Column>.Count
        {
            get { return ColumnsCount; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Functionality exposed via property Columns.")]
        Column IReadOnlyList<Column>.this[int index]
        {
            get { return GetColumn(index); }
        }

        protected abstract Column GetColumn(int index);

        public abstract bool IsValid(DataRow dataRow);

        public abstract string GetErrorMessage(DataRow dataRow);

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Functionality exposed via property Columns.")]
        IEnumerator<Column> IEnumerable<Column>.GetEnumerator()
        {
            for (int i = 0; i < ColumnsCount; i++)
                yield return GetColumn(i);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Functionality exposed via property Columns.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < ColumnsCount; i++)
                yield return GetColumn(i);
        }
    }
}
