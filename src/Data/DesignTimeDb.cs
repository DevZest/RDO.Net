using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public abstract class DesignTimeDb<T>
        where T : DbSession
    {
        public abstract T Create(string projectPath);
    }
}
