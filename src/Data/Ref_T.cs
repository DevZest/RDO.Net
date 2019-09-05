using DevZest.Data.Annotations.Primitives;
using System.Threading;

namespace DevZest.Data
{
    /// <summary>
    /// Represents foreign key reference.
    /// </summary>
    /// <typeparam name="T">The type of key.</typeparam>
    public abstract class Ref<T> : Projection
        where T : CandidateKey
    {
        /// <summary>
        /// Creates the foreign key.
        /// </summary>
        /// <returns>The created foreign key.</returns>
        [CreateKey]
        protected abstract T CreateForeignKey();

        private T _foreignKey;
        /// <summary>
        /// Gets the foreign key.
        /// </summary>
        public T ForeignKey
        {
            get { return LazyInitializer.EnsureInitialized(ref _foreignKey, () => CreateForeignKey()); }
        }
    }

}
