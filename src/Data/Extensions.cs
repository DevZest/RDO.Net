using DevZest.Data.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    /// <summary>
    /// Provides extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets child DataSet from specified parent DataRow.
        /// </summary>
        /// <typeparam name="T">Type of child model.</typeparam>
        /// <param name="childModel">The child model.</param>
        /// <param name="parentDataRow">The specified parent DataRow.</param>
        /// <returns>The result child DataSet.</returns>
        public static DataSet<T> GetChildDataSet<T>(this T childModel, DataRow parentDataRow) where T : Model, new()
        {
            Verify(childModel, nameof(childModel));
            parentDataRow.VerifyNotNull(nameof(parentDataRow));
            return (DataSet<T>)parentDataRow[childModel];
        }

        /// <summary>
        /// Gets child DataSet from specified parent DataRow ordinal.
        /// </summary>
        /// <typeparam name="T">Type of child model.</typeparam>
        /// <param name="childModel">The child model.</param>
        /// <param name="parentDataRowOrdinal">The specified parent DataRow ordinal.</param>
        /// <returns>The result child DataSet.</returns>
        public static DataSet<T> GetChildDataSet<T>(this T childModel, int parentDataRowOrdinal) where T : Model, new()
        {
            var parentDataSet = Verify(childModel, nameof(childModel));
            return childModel.GetChildDataSet(parentDataSet[parentDataRowOrdinal]);
        }

        private static DataSet Verify<T>(T childEntity, string paramName) where T : class, IEntity, new()
        {
            childEntity.VerifyNotNull(paramName);
            var parentDataSet = childEntity?.Model?.ParentModel?.DataSet;
            if (parentDataSet == null)
                throw new ArgumentException(DiagnosticMessages.EntityExtensions_NullParentDataSet, paramName);
            return parentDataSet;
        }

        /// <summary>
        /// Matches entity with specified key.
        /// </summary>
        /// <typeparam name="T">Key type of the source entity.</typeparam>
        /// <param name="source">The source entity.</param>
        /// <param name="target">The target key.</param>
        /// <returns>The result key mapping.</returns>
        public static KeyMapping Match<T>(this IEntity<T> source, T target)
            where T : CandidateKey
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(source.Model.PrimaryKey, target);
        }

        /// <summary>
        /// Matches two entities with same key.
        /// </summary>
        /// <typeparam name="T">Key type of the entity.</typeparam>
        /// <param name="source">The source entity.</param>
        /// <param name="target">The target entity.</param>
        /// <returns>The result key mapping.</returns>
        public static KeyMapping Match<T>(this IEntity<T> source, IEntity<T> target)
            where T : CandidateKey
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(source.Model.PrimaryKey, target.Model.PrimaryKey);
        }

        /// <summary>
        /// Joins source key with specified entity.
        /// </summary>
        /// <typeparam name="T">Type of the key.</typeparam>
        /// <param name="sourceKey">The source key.</param>
        /// <param name="target">The target entity.</param>
        /// <returns>The result key mapping.</returns>
        public static KeyMapping Join<T>(this T sourceKey, IEntity<T> target)
            where T : CandidateKey
        {
            return new KeyMapping(sourceKey, target.Model.PrimaryKey);
        }

        /// <summary>
        /// Returns the only value of specified database recordset column;
        /// this method throws an exception if no record or more than one record exists.
        /// </summary>
        /// <typeparam name="TEntity">The entity type of database recordset.</typeparam>
        /// <typeparam name="T">The data type of the column.</typeparam>
        /// <param name="dbSet">The database recordset.</param>
        /// <param name="getColumn">Delegate to return the column from Database recordset.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The only value.</returns>
        public static Task<T> SingleAsync<TEntity, T>(this DbSet<TEntity> dbSet, Func<TEntity, T> getColumn, CancellationToken ct = default(CancellationToken))
            where TEntity : class, IEntity, new()
            where T : Column, IColumn<DbReader>, new()
        {
            getColumn.VerifyNotNull(nameof(getColumn));
            return dbSet.ReadSingleAsync(getColumn, false, ct);
        }

        /// <summary>
        /// Returns the only value of specified database recordset column, or a default value if no record exists;
        /// this method throws an exception if more than one records exist.
        /// </summary>
        /// <typeparam name="TEntity">The entity type of database recordset.</typeparam>
        /// <typeparam name="T">The data type of the column.</typeparam>
        /// <param name="dbSet">The database recordset.</param>
        /// <param name="getColumn">Delegate to return the column from Database recordset.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The only or default value.</returns>
        public static Task<T> SingleOrDefaultAsync<TEntity, T>(this DbSet<TEntity> dbSet, Func<TEntity, T> getColumn, CancellationToken ct = default(CancellationToken))
            where TEntity : class, IEntity, new()
            where T : Column, IColumn<DbReader>, new()
        {
            getColumn.VerifyNotNull(nameof(getColumn));
            return dbSet.ReadSingleAsync(getColumn, true, ct);
        }

        private static async Task<T> ReadSingleAsync<TEntity, T>(this DbSet<TEntity> dbSet, Func<TEntity, T> getColumn, bool allowEmpty, CancellationToken ct)
            where TEntity : class, IEntity, new()
            where T : Column, IColumn<DbReader>, new()
        {
            var dbSession = dbSet.DbSession;
            var query = dbSession.CreateQuery<Adhoc>((builder, _) => builder.From(dbSet, out var s).Select(getColumn(s), _.AddColumn<T>()));

            using (var dbReader = await query.ExecuteDbReaderAsync(ct))
            {
                if (!(await dbReader.ReadAsync(ct)))
                {
                    if (allowEmpty)
                        return null;
                    else
                        throw new InvalidOperationException(DiagnosticMessages.Single_NoElement);
                }

                var dataSet = query.MakeEmptyDataSet();
                var result = dataSet._.GetColumn<T>();
                var dataRow = dataSet.AddRow();
                result.Read(dbReader, dataRow);
                if (await dbReader.ReadAsync(ct))
                    throw new InvalidOperationException(DiagnosticMessages.Single_MultipleElements);
                return result;
            }
        }

        internal static TChild Verify<T, TChild>(this T _, Func<T, TChild> getChildModel, string paramName)
            where T : class, IEntity, new()
            where TChild : Model, new()
        {
            getChildModel.VerifyNotNull(paramName);
            var result = getChildModel(_);
            if (result == null)
                throw new ArgumentException(DiagnosticMessages.DataSource_ChildModelGetterReturnsNull, paramName);
            if (result.ParentModel != _.Model)
                throw new ArgumentException(DiagnosticMessages.DataSource_ChildModelGetterReturnsInvalidValue, paramName);
            return result;
        }
    }
}
