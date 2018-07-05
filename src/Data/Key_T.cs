using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Linq.Expressions;

namespace DevZest.Data
{
    public abstract class Key<T> : Model<T>
        where T : PrimaryKey
    {
        /// <summary>
        /// Registers a column from existing column property, without inheriting its <see cref="ColumnId"/> value.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="TColumn">The type of the column.</typeparam>
        /// <param name="getter">The lambda expression of the column getter.</param>
        /// <param name="fromMounter">The existing column mounter.</param>
        /// <returns>Mounter of the column.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not an valid getter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="fromMounter"/> is null.</exception>
        [MounterRegistration]
        protected static void RegisterColumn<TModel, TColumn>(Expression<Func<TModel, TColumn>> getter, Mounter<TColumn> fromMounter)
            where TModel : Model
            where TColumn : Column, new()
        {
            var initializer = getter.Verify(nameof(getter));
            fromMounter.VerifyNotNull(nameof(fromMounter));

            var result = s_columnManager.Register(getter, mounter => CreateColumn(mounter, fromMounter, initializer));
            result.OriginalDeclaringType = fromMounter.OriginalDeclaringType;
            result.OriginalName = fromMounter.OriginalName;
        }

        private static TColumn CreateColumn<TModel, TColumn>(Mounter<TModel, TColumn> mounter, Mounter<TColumn> fromMounter, Action<TColumn> initializer)
            where TModel : Model
            where TColumn : Column, new()
        {
            var result = Column.Create<TColumn>(fromMounter.OriginalDeclaringType, fromMounter.OriginalName);
            result.Construct(mounter.Parent, mounter.DeclaringType, mounter.Name, ColumnKind.ModelProperty, fromMounter.Initializer, initializer);
            return result;
        }
    }
}
