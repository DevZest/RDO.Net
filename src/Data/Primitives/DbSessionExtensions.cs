using System;
using System.Collections.Generic;
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
            public TableCreator(string[] names)
            {
                _names = ToHashSet(names);
            }

            private HashSet<string> ToHashSet(string[] names)
            {
                if (names == null || names.Length == 0)
                    return null;

                HashSet<string> result = null;
                foreach (var name in names)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        if (result == null)
                            result = new HashSet<string>();
                        result.Add(name);
                    }
                }

                return result;
            }

            internal override void CreateMockDb()
            {
            }

            internal override string GetMockTableName(string name)
            {
                return name;
            }

            private readonly HashSet<string> _names;

            protected override void Initialize()
            {
                foreach (var property in GetTableProperties(Db))
                {
                    var dbTable = (IDbTable)property.GetValue(Db);
                    if (_names == null || _names.Contains(dbTable.Name))
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

        public static Task CreateTablesAsync(this DbSession dbSession, IProgress<MockDbProgress> progress, CancellationToken ct = default(CancellationToken))
        {
            return CreateTablesAsync(dbSession, progress, null, ct);
        }

        public static async Task CreateTablesAsync(this DbSession dbSession, IProgress<MockDbProgress> progress, string[] names, CancellationToken ct = default(CancellationToken))
        {
            dbSession.VerifyNotMocked();
            await new TableCreator(names).InternalInitializeAsync(dbSession, progress, ct);
            dbSession.Mock = null;
        }
    }
}
