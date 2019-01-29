using System.Collections.Generic;
using System.Diagnostics;
using DevZest.Data.Primitives;

namespace DevZest.Data.SqlServer
{
    internal sealed class ModelAliasManager : Dictionary<Model, string>, IModelAliasManager
    {
        internal static IModelAliasManager Create(DbFromClause fromClause)
        {
            var manager = new Visitor();
            fromClause.Accept(manager);
            return manager._aliases;
        }

        private sealed class Visitor : DbFromClauseVisitor
        {
            private readonly Dictionary<string, int> _suffixes = new Dictionary<string, int>();
            public readonly ModelAliasManager _aliases = new ModelAliasManager();

            public override void Visit(DbJoinClause join)
            {
                join.Left.Accept(this);
                join.Right.Accept(this);
            }

            public override void Visit(DbSelectStatement select)
            {
                AddAliase(select.Model);
            }

            public override void Visit(DbTableClause table)
            {
                AddAliase(table.Model);
            }

            public override void Visit(DbUnionStatement union)
            {
                AddAliase(union.Model);
            }

            private void AddAliase(Model model)
            {
                Debug.Assert(!_aliases.ContainsKey(model));
                _aliases.Add(model, _suffixes.GetUniqueName(GetDbAlias(model)));
            }
        }

        internal static string GetDbAlias(Model model)
        {
            if (IsTempTable(model))
                return ((IDbTable)model.GetDataSource()).Name;

            var result = model.GetDbAlias();
            var sourceJsonParam = model.GetSourceJsonParam();
            return ReferenceEquals(sourceJsonParam, null) ? result : "@" + result;
        }

        private static bool IsTempTable(Model model)
        {
            return model.GetDataSource()?.Kind == DataSourceKind.DbTempTable;
        }
    }
}
