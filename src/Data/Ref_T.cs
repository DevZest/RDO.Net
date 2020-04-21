using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq.Expressions;
using System.Threading;

namespace DevZest.Data
{
    /// <summary>
    /// Represents foreign key reference.
    /// </summary>
    /// <typeparam name="T">The type of key.</typeparam>
    public abstract class Ref<T> : Model
        where T : CandidateKey
    {
        /// <summary>
        /// Registers a column from existing column mounter.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="TColumn">The type of the column.</typeparam>
        /// <param name="getter">The lambda expression of the column getter.</param>
        /// <param name="fromMounter">The existing column mounter.</param>
        /// <returns>Mounter of the column.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not an valid getter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="fromMounter"/> is null.</exception>
        [PropertyRegistration]
        protected static void Register<TModel, TColumn>(Expression<Func<TModel, TColumn>> getter, Mounter<TColumn> fromMounter)
            where TModel : Model
            where TColumn : Column, new()
        {
            RegisterColumn(getter, fromMounter);
        }

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
