using System;

namespace DevZest.Data
{
    public static class EntityExtensions
    {
        public static DataSet<T> GetChildDataSet<T>(this T childEntity, DataRow parentDataRow) where T : class, IEntity, new()
        {
            Verify(childEntity, nameof(childEntity));
            parentDataRow.VerifyNotNull(nameof(parentDataRow));
            return (DataSet<T>)parentDataRow[childEntity.Model];
        }

        public static DataSet<T> GetChildDataSet<T>(this T childEntity, int parentDataRowOrdinal) where T : class, IEntity, new()
        {
            var parentDataSet = Verify(childEntity, nameof(childEntity));
            return childEntity.GetChildDataSet(parentDataSet[parentDataRowOrdinal]);
        }

        private static DataSet Verify<T>(T childEntity, string paramName) where T : class, IEntity, new()
        {
            childEntity.VerifyNotNull(paramName);
            var parentDataSet = childEntity?.Model?.ParentModel?.DataSet;
            if (parentDataSet == null)
                throw new ArgumentException(DiagnosticMessages.EntityExtensions_NullParentDataSet, paramName);
            return parentDataSet;
        }
    }
}
