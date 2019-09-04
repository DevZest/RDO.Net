using DevZest.Data.Primitives;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace DevZest.Data
{
    /// <summary>
    /// Base class to mock a database.
    /// </summary>
    /// <typeparam name="T">The type of database session.</typeparam>
    public abstract class DbMock<T> : DbInitializer<T>
        where T : DbSession
    {
        /// <inheritdoc />
        public sealed override async Task<T> GenerateAsync(T db, IProgress<DbInitProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            db.VerifyNotNull(nameof(db));

            _isDbGeneration = true;
            await InitializeAsync(db, nameof(db), progress, ct);
            return db;
        }

        /// <summary>
        /// Mocks the databased.
        /// </summary>
        /// <param name="db">The database to be mocked.</param>
        /// <param name="progress">The mocking progress to report.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The mocked database.</returns>
        /// <remarks>Derived class should define a static factory method, which implemented as calling this method.</remarks>
        protected async Task<T> MockAsync(T db, IProgress<DbInitProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            db.VerifyNotNull(nameof(db));

            await InitializeAsync(db, nameof(db), progress, ct);
            return db;
        }

        /// <summary>
        /// Mocks the database table.
        /// </summary>
        /// <typeparam name="TModel">Model type of the database table.</typeparam>
        /// <param name="dbTable">The database table should be mocked.</param>
        protected void Mock<TModel>(DbTable<TModel> dbTable)
            where TModel : Model, new()
        {
            AddTable(dbTable, null);
        }

        /// <summary>
        /// Mocks the database table with data.
        /// </summary>
        /// <typeparam name="TModel">Model type of the database table.</typeparam>
        /// <param name="dbTable">The database table should be mocked.</param>
        /// <param name="getDataSet">The delegate to return DataSet which contains the data.</param>
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

        internal sealed override async Task OnInitializingAsync(CancellationToken ct)
        {
            if (!_isDbGeneration)
                _tag = await Db.CreateMockDbAsync(ct);
        }

        internal sealed override string GetTableName(string name)
        {
            return _isDbGeneration ? name : Db.GetMockTableName(name, _tag);
        }
    }
}
