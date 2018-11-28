using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public static class DbSessionExtensions
    {
        private sealed class TableCreator : MockDb
        {
            internal override void CreateMockDb()
            {
            }

            internal override string GetMockTableName(string name)
            {
                return name;
            }

            protected override void Initialize()
            {
                foreach (var property in GetTableProperties(Db))
                {
                    var dbTable = (IDbTable)property.GetValue(Db);
                    AddMockTable(dbTable, null);
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetTableProperties(DbSession dbSession)
        {
            var properties = dbSession.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                if (!typeof(IDbTable).IsAssignableFrom(property.PropertyType))
                    continue;

                if (property.GetGetMethod() == null || property.GetIndexParameters().Length > 0)
                    continue;

                yield return property;
            }
        }

        public static int GetTotalNumberOfTables(this DbSession dbSession)
        {
            return GetTableProperties(dbSession).Count();
        }

        public static bool IsMocked(this DbSession dbSession)
        {
            return dbSession.Mock != null;
        }

        internal static void VerifyNotMocked(this DbSession dbSession)
        {
            if (dbSession.IsMocked())
                throw new InvalidOperationException(DiagnosticMessages.DbSession_VerifyNotMocked);
        }

        public static async Task CreateTablesAsync(this DbSession dbSession, IProgress<string> progress, CancellationToken ct = default(CancellationToken))
        {
            dbSession.VerifyNotMocked();
            await new TableCreator().InternalInitializeAsync(dbSession, progress, ct);
            dbSession.Mock = null;
        }
    }
}
