using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public abstract class DbGenerator<T> : MockDb
        where T : DbSession
    {
        public async Task<T> GenerateAsync(T db, IProgress<MockDbProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            await InternalInitializeAsync(db, nameof(db), progress, ct);
            return db;
        }

        internal override void CreateMockDb()
        {
        }

        internal override string GetMockTableName(string name)
        {
            return name;
        }

        protected sealed override void Initialize()
        {
            foreach (var property in GetTableProperties(Db))
            {
                var dbTable = (IDbTable)property.GetValue(Db);
                AddMockTable(dbTable, null);
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
    }
}
