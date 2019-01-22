using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.SqlServer
{
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
            foreach (var childModel in model.GetChildModels())
            {
                var relationship = childModel.GetParentRelationship();
                foreach (var mapping in relationship)
                {
                    if (mapping.Target == identityColumn)
                    {
                        var childTable = (IDbTable)childModel.GetDataSource();
                        Debug.Assert(mapping.Source != null);
                        result.Add(childTable.BuildUpdateIdentityStatement(mapping.Source, identityMappings));
                    }
                }
            }
        }

        private static DbSelectStatement BuildUpdateIdentityStatement(this IDbTable dbTable, Column identityColumn, IDbTable identityMappings)
        {
            Debug.Assert(identityColumn != null && identityColumn.GetParent() == dbTable.Model);

            var identityMapping = identityMappings.Model;

            if (identityMapping is Int32IdentityMapping int32IdentityMapping)
            {
                var keyMappings = new ColumnMapping[] { ColumnMapping.UnsafeMap(int32IdentityMapping.OldValue, identityColumn) };
                var columnMappings = new ColumnMapping[] { ColumnMapping.UnsafeMap(int32IdentityMapping.NewValue, identityColumn) };
                return dbTable.BuildUpdateStatement(identityMappings, columnMappings, keyMappings);
            }
            else if (identityMapping is Int64IdentityMapping int64IdentityMapping)
            {
                var keyMappings = new ColumnMapping[] { ColumnMapping.UnsafeMap(int64IdentityMapping.OldValue, identityColumn) };
                var columnMappings = new ColumnMapping[] { ColumnMapping.UnsafeMap(int64IdentityMapping.NewValue, identityColumn) };
                return dbTable.BuildUpdateStatement(identityMappings, columnMappings, keyMappings);
            }
            else if (identityMapping is Int32IdentityMapping int16IdentityMapping)
            {
                var keyMappings = new ColumnMapping[] { ColumnMapping.UnsafeMap(int16IdentityMapping.OldValue, identityColumn) };
                var columnMappings = new ColumnMapping[] { ColumnMapping.UnsafeMap(int16IdentityMapping.NewValue, identityColumn) };
                return dbTable.BuildUpdateStatement(identityMappings, columnMappings, keyMappings);
            }
            else
            {
                Debug.Fail("identityMappsings must be a table of Int32IdentityMapping, Int64IdentityMapping or Int16IdentityMapping.");
                return null;
            }
        }
    }
}
