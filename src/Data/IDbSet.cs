using DevZest.Data.Primitives;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a database recordset.
    /// </summary>
    public interface IDbSet
    {
        /// <summary>
        /// Gets the data source kind.
        /// </summary>
        DataSourceKind Kind { get; }

        /// <summary>
        /// Gets the model of this database recordset.
        /// </summary>
        Model Model { get; }

        /// <summary>
        /// Gets the database session.
        /// </summary>
        DbSession DbSession { get; }

        /// <summary>
        /// Gets the query statement.
        /// </summary>
        DbQueryStatement QueryStatement { get; }

        /// <summary>
        /// Gets the SQL FROM clause.
        /// </summary>
        DbFromClause FromClause { get; }

        /// <summary>
        /// Gets the sequential query statement.
        /// </summary>
        DbQueryStatement SequentialQueryStatement { get; }

        /// <summary>
        /// Saves this database recordset into DataSet
        /// </summary>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The result DataSet.</returns>
        Task<DataSet> ToDataSetAsync(CancellationToken ct);
    }
}
