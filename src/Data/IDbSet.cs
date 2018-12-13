using DevZest.Data.Primitives;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public interface IDbSet
    {
        DataSourceKind Kind { get; }

        Model Model { get; }

        DbSession DbSession { get; }

        DbQueryStatement QueryStatement { get; }

        DbFromClause FromClause { get; }

        DbQueryStatement SequentialQueryStatement { get; }

        Task<DataSet> ToDataSetAsync(CancellationToken ct);
    }
}
