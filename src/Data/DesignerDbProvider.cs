using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public abstract class DesignerDbProvider<T>
        where T : DbSession
    {
        public abstract T Create(string projectPath);
    }
}
