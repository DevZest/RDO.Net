using DevZest.Data.Utilities;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

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

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Functionality exposed via property DependentColumnsCount.")]
        int IReadOnlyCollection<Column>.Count
        {
            get { return DependentColumnsCount; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Functionality exposed via method GetDependentColumn.")]
        Column IReadOnlyList<Column>.this[int index]
        {
            get { return GetDependentColumn(index); }
        }

        protected abstract Column GetDependentColumn(int index);

        public abstract bool IsValid(DataRow dataRow);

        public abstract string GetErrorMessage(DataRow dataRow);

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<Column> IEnumerable<Column>.GetEnumerator()
        {
            for (int i = 0; i < DependentColumnsCount; i++)
                yield return GetDependentColumn(i);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < DependentColumnsCount; i++)
                yield return GetDependentColumn(i);
        }
    }
}
