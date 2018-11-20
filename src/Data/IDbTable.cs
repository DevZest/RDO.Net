using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public interface IDbTable : IDbSet
    {
        string Name { get; }
        bool DesignMode { get; }
    }

    internal static class IDbTableExtensions
    {
        internal static IList<DbSelectStatement> BuildUpdateIdentityStatement(this IDbTable dbTable, IDbTable identityMappings)
        {
            var dbTableModel = dbTable.Model;
            Column identityColumn = dbTableModel.GetIdentity(false).Column;
            Debug.Assert(identityColumn != null);

            var result = new List<DbSelectStatement>();
            BuildUpdateIdentityStatementRecursively(result, dbTable, identityColumn, identityMappings);
            return result;
        }

        private static void BuildUpdateIdentityStatementRecursively(IList<DbSelectStatement> result, IDbTable dbTable, Column identityColumn, IDbTable identityMappings)
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

        private static DbSelectStatement BuildUpdateIdentityStatement(this IDbTable dbTable, Column identityColumn, IDbTable identityMappings)
        {
            var dbTableModel = dbTable.Model;
            Debug.Assert(identityColumn != null && identityColumn.ParentModel == dbTableModel);

            var identityMapping = identityMappings.Model;

            if (identityMapping is Int32IdentityMapping int32IdentityMapping)
            {
                var keyMappings = new ColumnMapping[] { new ColumnMapping(int32IdentityMapping.OldValue, identityColumn) };
                var columnMappings = new ColumnMapping[] { new ColumnMapping(int32IdentityMapping.NewValue, identityColumn) };
                return identityMappings.QueryStatement.BuildUpdateStatement(dbTableModel, columnMappings, keyMappings);
            }
            else if (identityMapping is Int64IdentityMapping int64IdentityMapping)
            {
                var keyMappings = new ColumnMapping[] { new ColumnMapping(int64IdentityMapping.OldValue, identityColumn) };
                var columnMappings = new ColumnMapping[] { new ColumnMapping(int64IdentityMapping.NewValue, identityColumn) };
                return identityMappings.QueryStatement.BuildUpdateStatement(dbTableModel, columnMappings, keyMappings);
            }
            else if (identityMapping is Int32IdentityMapping int16IdentityMapping)
            {
                var keyMappings = new ColumnMapping[] { new ColumnMapping(int16IdentityMapping.OldValue, identityColumn) };
                var columnMappings = new ColumnMapping[] { new ColumnMapping(int16IdentityMapping.NewValue, identityColumn) };
                return identityMappings.QueryStatement.BuildUpdateStatement(dbTableModel, columnMappings, keyMappings);
            }
            else
            {
                Debug.Fail("identityMappsings must be a table of Int32IdentityMapping, Int64IdentityMapping or Int16IdentityMapping.");
                return null;
            }
        }
    }
}
