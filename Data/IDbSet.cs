using DevZest.Data.Primitives;

namespace DevZest.Data
{
    internal interface IDbSet
    {
        DataSourceKind Kind { get; }

        Model Model { get; }

        DbSession DbSession { get; }

        DbQueryStatement QueryStatement { get; }

        DbFromClause FromClause { get; }

        DbQueryStatement SequentialQueryStatement { get; }
    }
}
