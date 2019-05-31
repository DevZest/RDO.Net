using System;

namespace DevZest.Data
{
    public static class Extensions
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

        public static KeyMapping Match<T>(this IEntity<T> source, T target)
            where T : CandidateKey
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(source.Model.PrimaryKey, target);
        }

        public static KeyMapping Match<T>(this IEntity<T> source, IEntity<T> target)
            where T : CandidateKey
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(source.Model.PrimaryKey, target.Model.PrimaryKey);
        }

        public static KeyMapping Join<T>(this T sourceKey, IEntity<T> target)
            where T : CandidateKey
        {
            return new KeyMapping(sourceKey, target.Model.PrimaryKey);
        }
    }
}
