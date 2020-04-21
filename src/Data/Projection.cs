using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq.Expressions;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a projection, which inherits subset of columns from model.
    /// </summary>
    public abstract class Projection : Model
    {
        /// <summary>
        /// Registers a column from existing column mounter.
        /// </summary>
        /// <typeparam name="T">The type of projection which the column is registered on.</typeparam>
        /// <typeparam name="TColumn">The type of the column.</typeparam>
        /// <param name="getter">The lambda expression of the column getter.</param>
        /// <param name="fromMounter">The existing column mounter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not an valid getter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="fromMounter"/> is null.</exception>
        [PropertyRegistration]
        protected static void Register<T, TColumn>(Expression<Func<T, TColumn>> getter, Mounter<TColumn> fromMounter)
            where T : Projection
            where TColumn : Column, new()
        {
            RegisterColumn(getter, fromMounter);
        }
    }
}
