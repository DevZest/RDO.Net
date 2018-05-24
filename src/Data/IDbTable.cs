using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    internal interface IDbTable : IDbSet
    {
        string Name { get; }
    }

    internal static class IDbTableExtensions
    {
        internal static IList<DbSelectStatement> BuildUpdateIdentityStatement(this IDbTable dbTable, DbTable<IdentityMapping> identityMappings)
        {
            var dbTableModel = dbTable.Model;
            Column identityColumn = dbTableModel.GetIdentity(false).Column;
            Debug.Assert(identityColumn != null);

            var result = new List<DbSelectStatement>();
            BuildUpdateIdentityStatementRecursively(result, dbTable, identityColumn, identityMappings);
            return result;
        }

        private static void BuildUpdateIdentityStatementRecursively(IList<DbSelectStatement> result, IDbTable dbTable, Column identityColumn, DbTable<IdentityMapping> identityMappings)
        {
            result.Add(dbTable.BuildUpdateIdentityStatement(identityColumn, identityMappings));

            var model = dbTable.Model;
            foreach (var childModel in model.ChildModels)
            {
                var relationship = childModel.ParentRelationship;
                foreach (var mapping in relationship)
                {
                    if (mapping.Target == identityColumn)
                    {
                        var childTable = (IDbTable)childModel.DataSource;
                        Debug.Assert(mapping.Source != null);
                        result.Add(childTable.BuildUpdateIdentityStatement(mapping.Source, identityMappings));
                    }
                }
            }
        }


        private static DbSelectStatement BuildUpdateIdentityStatement(this IDbTable dbTable, Column identityColumn, DbTable<IdentityMapping> identityMappings)
        {
            var dbTableModel = dbTable.Model;
            Debug.Assert(identityColumn != null && identityColumn.ParentModel == dbTableModel);
            var keyMappings = new ColumnMapping[] { new ColumnMapping(identityMappings._.OldValue, identityColumn) };
            var columnMappings = new ColumnMapping[] { new ColumnMapping(identityMappings._.NewValue, identityColumn) };
            return identityMappings.QueryStatement.BuildUpdateStatement(dbTableModel, columnMappings, keyMappings);
        }
    }
}
