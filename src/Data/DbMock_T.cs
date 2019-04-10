using DevZest.Data.Primitives;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace DevZest.Data
{
    public abstract class DbMock<T> : DbGenerator, IDbGenerator<T>
        where T : DbSession
    {
        public new T Db
        {
            get { return (T)base.Db; }
        }

        public async Task<T> GenerateAsync(T db, IProgress<DbGenerationProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            _isDbGeneration = true;
            await InternalInitializeAsync(db, nameof(db), progress, ct);
            return db;
        }

        public async Task<T> InitializeAsync(T db, IProgress<DbGenerationProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            await InternalInitializeAsync(db, nameof(db), progress, ct);
            return db;
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable)
            where TModel : Model, new()
        {
            AddTable(dbTable, null);
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable, Func<DataSet<TModel>> getDataSet)
            where TModel : Model, new()
        {
            getDataSet.VerifyNotNull(nameof(getDataSet));

            AddTable(dbTable, async (ct) => {
                var dataSet = getDataSet();
                if (dataSet != null)
                {
                    dbTable._.SuspendIdentity();
                    try
                    {
                        await dbTable.InsertAsync(dataSet, ct);
                    }
                    finally
                    {
                        dbTable._.ResumeIdentity();
                    }
                }
            });
        }

        private object _tag;
        private bool _isDbGeneration;

        internal sealed override void OnInitializing()
        {
            if (!_isDbGeneration)
                _tag = Db.CreateMockDb();
        }

        internal sealed override string GetTableName(string name)
        {
            return _isDbGeneration ? name : Db.GetMockTableName(name, _tag);
        }
    }
}
