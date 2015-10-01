using System;

namespace DevZest.Data
{
    /// <summary>Model for adhoc queries.</summary>
    /// <seealso cref="DbQueryBuilder.Select{T}(T, Adhoc, string)"/>
    /// <seealso cref="DbAggregateQueryBuilder.Select{T}(T, Adhoc, string)"/>
    public sealed class Adhoc : Model
    {
        static Adhoc()
        {
            RegisterColumnList((Adhoc x) => x.ColumnList);
        }

        private ColumnList<Column> ColumnList { get; set; }

        /// <summary>Adds a new column to this adhoc model, with optional column initializer.</summary>
        /// <typeparam name="T">The type of the column.</typeparam>
        /// <param name="initializer">The column initializer.</param>
        /// <returns>The new column added.</returns>
        /// <overloads>Adds a new column to this adhoc model.</overloads>
        public T AddColumn<T>(Action<T> initializer = null)
            where T : Column, new()
        {
            return ColumnList.Add<T>(initializer);
        }

        /// <summary>Adds a new column to this adhoc model, from existing column.</summary>
        /// <typeparam name="T">The type of the column.</typeparam>
        /// <param name="column">The existing column.</param>
        /// <param name="inheritColumnKey">A value indicates whether the newly added column should inherit <see cref="ColumnKey"/> from the existing column.</param>
        /// <param name="initializer">The additional column initializer.</param>
        /// <returns>The new column added.</returns>
        public T AddColumn<T>(T column, bool inheritColumnKey = false, Action<T> initializer = null)
            where T : Column, new()
        {
            return ColumnList.Add(column, inheritColumnKey, initializer);
        }

        /// <summary>Adds a new column to this adhoc model, from existing column accessor.</summary>
        /// <typeparam name="TModel">The model type of the existing column accessor.</typeparam>
        /// <typeparam name="T">The type of the column.</typeparam>
        /// <param name="accessor">The existing column accessor.</param>
        /// <param name="inheritColumnKey">A value indicates whether the newly added column should inherit <see cref="ColumnKey"/> from the existing column accessor.</param>
        /// <param name="initializer">The additional column initializer.</param>
        /// <returns>The new column added.</returns>
        public T AddColumn<TModel, T>(Accessor<TModel, T> accessor, bool inheritColumnKey = false, Action<T> initializer = null)
            where T : Column, new()
        {
            return ColumnList.Add(accessor, inheritColumnKey, initializer);
        }

        /// <summary>Gets the column at specified index.</summary>
        /// <param name="index">The index.</param>
        /// <returns>The column at specified index.</returns>
        public Column this[int index]
        {
            get { return ColumnList[index]; }
        }
    }
}
