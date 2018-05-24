using DevZest.Data.Primitives;
using System;
using DevZest.Data.Utilities;
using System.Threading.Tasks;
using System.Threading;

namespace DevZest.Data
{
    public abstract class MockDb<T> : MockDb
        where T : DbSession
    {
        public new T Db
        {
            get { return (T)base.Db; }
        }

        public object Tag { get; private set; }

        public async Task<T> InitializeAsync(T db, CancellationToken ct = default(CancellationToken))
        {
            Check.NotNull(db, nameof(db));
            db.VerifyNotMocked();
            if (Db != null)
                throw new InvalidOperationException(DiagnosticMessages.MockDb_InitializeTwice);

            await InternalInitializeAsync(db, null, ct);
            return db;
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable)
            where TModel : Model, new()
        {
            Action action = null;
            AddMockTable(dbTable, action);
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable, params DataSet<TModel>[] dataSets)
            where TModel : Model, new()
        {
            AddMockTable(dbTable, () => {
                foreach (var dataSet in dataSets)
                {
                    if (dataSet != null)
                        dbTable.Insert(dataSet).ExecuteAsync().Wait();
                }
            });
        }

        internal sealed override void CreateMockDb()
        {
            Tag = Db.CreateMockDb();
        }

        internal sealed override string GetMockTableName(string name)
        {
            return Db.GetMockTableName(name, Tag);
        }
    }
}
