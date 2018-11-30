using DevZest.Data.Primitives;
using System;
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
            db.VerifyNotNull(nameof(db));
            db.VerifyNotMocked();
            if (Db != null)
                throw new InvalidOperationException(DiagnosticMessages.MockDb_InitializeTwice);

            await InternalInitializeAsync(db, null, ct);
            return db;
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable)
            where TModel : Model, new()
        {
            AddMockTable(dbTable, null);
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable, Func<DataSet<TModel>> getDataSet)
            where TModel : Model, new()
        {
            getDataSet.VerifyNotNull(nameof(getDataSet));

            AddMockTable(dbTable, async (ct) => {
                var dataSet = getDataSet();
                if (dataSet != null)
                    await dbTable.Insert(dataSet).ExecuteAsync(ct);
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
