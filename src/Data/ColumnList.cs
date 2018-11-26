using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    /// <summary>Base class of <see cref="ColumnList{TColumn}"/> class.</summary>
    public abstract class ColumnList : ModelMember, IReadOnlyList<Column>
    {
        /// <summary>Gets the number of columns in this<see cref="ColumnList"/>.</summary>
        public abstract int Count { get; }

        internal abstract Column GetColumn(int index);

        /// <summary>Gets the column at specified index.</summary>
        /// <param name="index">The specified index.</param>
        /// <returns>The column at specified index.</returns>
        public Column this[int index]
        {
            get { return GetColumn(index); }
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        public IEnumerator<Column> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return GetColumn(i);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return GetColumn(i);
        }

        internal abstract void Initialize(ColumnList sourceColumnList);
    }

    /// <summary>Represents a list of columns.</summary>
    /// <typeparam name="TColumn">The type of the columns.</typeparam>
    /// <remarks>This class is used for model which contains variable number of columns.</remarks>
    /// <seealso cref="Model.RegisterColumnList{TModel, T}(System.Linq.Expressions.Expression{Func{TModel, ColumnList{T}}})"/>
    public sealed class ColumnList<TColumn> : ColumnList, IReadOnlyList<TColumn>
        where TColumn : Column
    {
        List<Func<ColumnList<TColumn>, TColumn>> _constructors = new List<Func<ColumnList<TColumn>, TColumn>>();
        List<TColumn> _columns = new List<TColumn>();

        /// <inheritdoc/>
        public override int Count
        {
            get { return _columns.Count; }
        }

        internal override Column GetColumn(int index)
        {
            return this[index];
        }

        /// <inheritdoc/>
        public new TColumn this[int index]
        {
            get { return _columns[index]; }
        }

        public TColumn Add(Func<TColumn> createColumn, Action<TColumn> initializer = null)
        {
            createColumn.VerifyNotNull(nameof(createColumn));
            VerifyDesignMode();
            return Add(x => CreateColumn<TColumn>(createColumn, x, null, null, null, initializer));
        }

        /// <summary>Add a new column into this column list.</summary>
        /// <typeparam name="T">The type of the column.</typeparam>
        /// <param name="initializer">The column initializer.</param>
        /// <returns>The new column added.</returns>
        /// <overloads>Add a new column into this column list.</overloads>
        public T Add<T>(Action<T> initializer = null)
            where T : TColumn, new()
        {
            VerifyDesignMode();
            return Add(x => CreateColumn(x, null, null, null, initializer));
        }

        /// <summary>Add a new column into this column list, from existing column.</summary>
        /// <typeparam name="T">The type of the column.</typeparam>
        /// <param name="column">The existing column.</param>
        /// <param name="inheritColumnKey"><see langword="true"/> to inherit <see cref="ColumnId"/> from existing column, otherwise <see langword="false"/>.</param>
        /// <param name="initializer">The column initializer.</param>
        /// <returns>The new column added.</returns>
        public T Add<T>(T column, bool inheritColumnKey = false, Action<T> initializer = null)
            where T : TColumn, new()
        {
            VerifyDesignMode();
            column.VerifyNotNull(nameof(column));
            var baseInitializer = ColumnInitializerManager<T>.GetInitializer(column);
            if (inheritColumnKey)
                return Add(x => CreateColumn(x, column.OriginalDeclaringType, column.OriginalName, baseInitializer, initializer));
            else
                return Add(x => CreateColumn(x, null, null, baseInitializer, initializer));
        }

        /// <summary>Add a new column into this column list, from existing column property.</summary>
        /// <typeparam name="T">The type of the column.</typeparam>
        /// <param name="fromMounter">The existing column mounter.</param>
        /// <param name="inheritOriginalId"><see langword="true"/> to inherit <see cref="ColumnId"/> from existing column property,
        /// otherwise <see langword="false"/>.</param>
        /// <param name="initializer">The column initializer.</param>
        /// <returns>The new column added.</returns>
        public T Add<T>(Mounter<T> fromMounter, bool inheritOriginalId = false, Action<T> initializer = null)
            where T : TColumn, new()
        {
            VerifyDesignMode();
            fromMounter.VerifyNotNull(nameof(fromMounter));
            if (inheritOriginalId)
                return Add(x => CreateColumn(x, fromMounter.OriginalDeclaringType, fromMounter.OriginalName, fromMounter.Initializer, initializer));
            else
                return Add(x => CreateColumn(x, null, null, fromMounter.Initializer, initializer));
        }

        private static T CreateColumn<T>(ColumnList<TColumn> columnList, Type originalDeclaringType, string originalName, Action<T> baseInitializer, Action<T> initializer)
            where T : TColumn, new()
        {
            return CreateColumn(() => new T(), columnList, originalDeclaringType, originalName, baseInitializer, initializer);
        }

        private static T CreateColumn<T>(Func<T> create, ColumnList<TColumn> columnList, Type originalDeclaringType, string originalName, Action<T> baseInitializer, Action<T> initializer)
            where T : TColumn
        {
            var name = columnList.GetNewColumnName();
            if (originalDeclaringType == null)
            {
                originalDeclaringType = columnList.DeclaringType;
                originalName = name;
            }
            var result = Column.Create(create, originalDeclaringType, originalName);
            if (result != null)
                result.Construct(columnList.ParentModel, columnList.DeclaringType, name, ColumnKind.ColumnListItem, baseInitializer, initializer);
            return result;
        }

        private T Add<T>(Func<ColumnList<TColumn>, T> constructor)
            where T : TColumn
        {
            Debug.Assert(constructor != null);

            _constructors.Add(constructor);
            var result = constructor(this);
            if (result == null)
                return null;
            _columns.Add(result);
            return result;
        }

        private string GetNewColumnName()
        {
            return string.Format("{0}_{1}", Name, _columns.Count);
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        public new IEnumerator<TColumn> GetEnumerator()
        {
            return _columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _columns.GetEnumerator();
        }

        internal override void Initialize(ColumnList prototype)
        {
            var source = (ColumnList<TColumn>)prototype;

            foreach (var constructor in source._constructors)
            {
                _constructors.Add(constructor);
                var result = constructor(this);
                _columns.Add(result);
            }
        }
    }
}
