using DevZest.Data.Primitives;
using System.Diagnostics;

namespace DevZest.Data
{
    public abstract class DbTableInsert<T> : Executable<int>
        where T : Model, new()
    {
        protected DbTableInsert(DbTable<T> into)
        {
            Debug.Assert(into != null);
            _into = into;
        }

        private readonly DbTable<T> _into;
        protected DbTable<T> Into
        {
            get { return _into; }
        }

        protected DbSession DbSession
        {
            get { return Into.DbSession; }
        }

    }
}
