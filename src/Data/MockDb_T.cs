using DevZest.Data.Primitives;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace DevZest.Data
{
    public abstract class MockDb<T> : MockDb, IDbGenerator<T>
        where T : DbSession
    {
        public new T Db
        {
            get { return (T)base.Db; }
        }

        public async Task<T> GenerateAsync(T db, IProgress<MockDbProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            _isDbGenerator = true;
            await InternalInitializeAsync(db, nameof(db), progress, ct);
            return db;
        }

        public async Task<T> InitializeAsync(T db, IProgress<MockDbProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            await InternalInitializeAsync(db, nameof(db), progress, ct);
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
                {
                    dbTable._.SuspendIdentity();
                    try
                    {
                        await dbTable.Insert(dataSet).ExecuteAsync(ct);
                    }
                    finally
                    {
                        dbTable._.ResumeIdentity();
                    }
                }
            });
        }

        private object _tag;
        private bool _isDbGenerator;

        internal sealed override void CreateMockDb()
        {
            if (!_isDbGenerator)
                _tag = Db.CreateMockDb();
        }

        internal sealed override string GetMockTableName(string name)
        {
            return _isDbGenerator ? name : Db.GetMockTableName(name, _tag);
        }
    }
}
