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

        public new string Name
        {
            get { return base.Name; }
        }
    }

    /// <summary>Represents a list of columns.</summary>
    /// <typeparam name="T">The type of the columns.</typeparam>
    /// <remarks>This class is used for model which contains variable number of columns.</remarks>
    /// <seealso cref="Model.RegisterColumnList{TModel, T}(System.Linq.Expressions.Expression{Func{TModel, ColumnList{T}}})"/>
    public sealed class ColumnList<T> : ColumnList, IReadOnlyList<T>
        where T : Column
    {
        List<Func<ColumnList<T>, T>> _constructors = new List<Func<ColumnList<T>, T>>();
        List<T> _columns = new List<T>();

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
        public new T this[int index]
        {
            get { return _columns[index]; }
        }

        public T Add(Func<T> createColumn, Action<T> initializer = null)
        {
            createColumn.VerifyNotNull(nameof(createColumn));
            VerifyDesignMode();
            return Add(x => CreateColumn<T>(createColumn, x, null, null, null, initializer));
        }

        /// <summary>Add a new column into this column list.</summary>
        /// <typeparam name="TColumn">The type of the column.</typeparam>
        /// <param name="initializer">The column initializer.</param>
        /// <returns>The new column added.</returns>
        /// <overloads>Add a new column into this column list.</overloads>
        public TColumn Add<TColumn>(Action<TColumn> initializer = null)
            where TColumn : T, new()
        {
            VerifyDesignMode();
            return Add(x => CreateColumn(x, null, null, null, initializer));
        }

        /// <summary>Add a new column into this column list, from existing column.</summary>
        /// <typeparam name="TColumn">The type of the column.</typeparam>
        /// <param name="column">The existing column.</param>
        /// <param name="inheritColumnKey"><see langword="true"/> to inherit <see cref="ColumnId"/> from existing column, otherwise <see langword="false"/>.</param>
        /// <param name="initializer">The column initializer.</param>
        /// <returns>The new column added.</returns>
        public TColumn Add<TColumn>(TColumn column, bool inheritColumnKey = false, Action<TColumn> initializer = null)
            where TColumn : T, new()
        {
            VerifyDesignMode();
            column.VerifyNotNull(nameof(column));
            var baseInitializer = ColumnInitializerManager<TColumn>.GetInitializer(column);
            if (inheritColumnKey)
                return Add(x => CreateColumn(x, column.OriginalDeclaringType, column.OriginalName, baseInitializer, initializer));
            else
                return Add(x => CreateColumn(x, null, null, baseInitializer, initializer));
        }

        /// <summary>Add a new column into this column list, from existing column property.</summary>
        /// <typeparam name="TColumn">The type of the column.</typeparam>
        /// <param name="fromMounter">The existing column mounter.</param>
        /// <param name="inheritOriginalId"><see langword="true"/> to inherit <see cref="ColumnId"/> from existing column property,
        /// otherwise <see langword="false"/>.</param>
        /// <param name="initializer">The column initializer.</param>
        /// <returns>The new column added.</returns>
        public TColumn Add<TColumn>(Mounter<TColumn> fromMounter, bool inheritOriginalId = false, Action<TColumn> initializer = null)
            where TColumn : T, new()
        {
            VerifyDesignMode();
            fromMounter.VerifyNotNull(nameof(fromMounter));
            if (inheritOriginalId)
                return Add(x => CreateColumn(x, fromMounter.OriginalDeclaringType, fromMounter.OriginalName, fromMounter.Initializer, initializer));
            else
                return Add(x => CreateColumn(x, null, null, fromMounter.Initializer, initializer));
        }

        private static TColumn CreateColumn<TColumn>(ColumnList<T> columnList, Type originalDeclaringType, string originalName, Action<TColumn> baseInitializer, Action<TColumn> initializer)
            where TColumn : T, new()
        {
            return CreateColumn(() => new TColumn(), columnList, originalDeclaringType, originalName, baseInitializer, initializer);
        }

        private static TColumn CreateColumn<TColumn>(Func<TColumn> create, ColumnList<T> columnList, Type originalDeclaringType, string originalName, Action<TColumn> baseInitializer, Action<TColumn> initializer)
            where TColumn : T
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

        private TColumn Add<TColumn>(Func<ColumnList<T>, TColumn> constructor)
            where TColumn : T
        {
            Debug.Assert(constructor != null);

            _constructors.Add(constructor);
            var result = constructor(this);
            if (result == null)
                return null;
            if (result.IsLocal)
                throw new InvalidOperationException(DiagnosticMessages.ColumnList_AddLocalColumn);
            _columns.Add(result);
            return result;
        }

        private string GetNewColumnName()
        {
            return string.Format("{0}_{1}", Name, _columns.Count);
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        public new IEnumerator<T> GetEnumerator()
        {
            return _columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _columns.GetEnumerator();
        }

        internal override void Initialize(ColumnList prototype)
        {
            var source = (ColumnList<T>)prototype;

            foreach (var constructor in source._constructors)
            {
                _constructors.Add(constructor);
                var result = constructor(this);
                _columns.Add(result);
            }
        }
    }
}
