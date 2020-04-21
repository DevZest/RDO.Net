using System;

namespace DevZest.Data
{
    /// <summary>Model for adhoc queries.</summary>
    /// <seealso cref="DbQueryBuilder.Select{T}(T, Adhoc, string)"/>
    /// <seealso cref="DbAggregateQueryBuilder.Select{T}(T, Adhoc, string)"/>
    public class Adhoc : Model
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
        /// <param name="inheritColumnKey">A value indicates whether the newly added column should inherit <see cref="ColumnId"/> from the existing column.</param>
        /// <param name="initializer">The additional column initializer.</param>
        /// <returns>The new column added.</returns>
        public T AddColumn<T>(T column, bool inheritColumnKey = false, Action<T> initializer = null)
            where T : Column, new()
        {
            return ColumnList.Add(column, inheritColumnKey, initializer);
        }

        /// <summary>Adds a new column to this adhoc model, from existing column property.</summary>
        /// <typeparam name="T">The type of the column.</typeparam>
        /// <param name="mounter">The existing column mounter.</param>
        /// <param name="inheritColumnKey">A value indicates whether the newly added column should inherit <see cref="ColumnId"/> from the existing column property.</param>
        /// <param name="initializer">The additional column initializer.</param>
        /// <returns>The new column added.</returns>
        public T AddColumn<T>(Mounter<T> mounter, bool inheritColumnKey = false, Action<T> initializer = null)
            where T : Column, new()
        {
            return ColumnList.Add(mounter, inheritColumnKey, initializer);
        }

        /// <summary>Gets the column at specified index.</summary>
        /// <param name="index">The index.</param>
        /// <returns>The column at specified index.</returns>
        public Column this[int index]
        {
            get { return ColumnList[index]; }
        }

        /// <summary>Gets the column collection.</summary>
        public new ColumnCollection Columns
        {
            get { return base.Columns; }
        }

        /// <summary>Gets the column by name.</summary>
        /// <typeparam name="T">Type of the returned column.</typeparam>
        /// <param name="name">The name of the column.</param>
        /// <returns>The column with specified name.</returns>
        public T GetColumn<T>(string name)
            where T : Column
        {
            return (T)Columns[name];
        }

        /// <summary>Gets the column by index.</summary>
        /// <typeparam name="T">Type of the returned column.</typeparam>
        /// <param name="index">The index of the column in <see cref="Columns"/> collection.</param>
        /// <returns>The column with specified index.</returns>
        public T GetColumn<T>(int index)
            where T : Column
        {
            return (T)Columns[index];
        }

        /// <summary>Gets the first column.</summary>
        /// <typeparam name="T">Type of the returned column.</typeparam>
        /// <returns>The first column in <see cref="Columns"/> collection.</returns>
        public T GetColumn<T>()
            where T : Column
        {
            return GetColumn<T>(0);
        }
    }
}
