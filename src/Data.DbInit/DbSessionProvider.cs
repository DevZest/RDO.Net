using DevZest.Data.Primitives;

namespace DevZest.Data.DbInit
{
    /// <summary>
    /// Provides <see cref="DbSession"/> objects.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="DbSession"/>.</typeparam>
    public abstract class DbSessionProvider<T>
        where T : DbSession
    {
        /// <summary>
        /// Creates instance of <see cref="DbSession"/>.
        /// </summary>
        /// <param name="projectPath">The source code project path.</param>
        /// <returns>The new instance of <see cref="DbSession"/>.</returns>
        public abstract T Create(string projectPath);
    }
}
