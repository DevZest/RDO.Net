using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a column with strongly typed data.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    public abstract partial class Column<T> : Column, IColumn
    {
        private interface IValueManager
        {
            bool IsPrimaryKey { get; }
            bool ShouldSerialize { get; }
            T this[int ordinal] { get; set; }
            void Insert(int ordinal, T value);
            void RemoveAt(int ordinal);
        }

        private abstract class ValueManager : List<T>, IValueManager
        {
            public static ValueManager Create(Column<T> column)
            {
                Debug.Assert(column.IsConcrete);
                if (column.Expression == null)
                    return new ListValueManager(column);
                else
                    return new ComputationValueManager(column);
            }

            public static ValueManager EnsureConcrete(Column<T> column)
            {
                Debug.Assert(column.IsExpression && column.IsConcrete);
                var result = new ComputationValueManager(column);
                var expression = column.Expression;
                var dataSet = column.ParentModel.DataSet;
                foreach (var dataRow in dataSet)
                    result.Add(expression[dataRow]);
                return result;
            }

            private sealed class ListValueManager : ValueManager
            {
                public ListValueManager(Column<T> column)
                    : base(column)
                {
                    _isPrimaryKey = column.GetIsPrimaryKey();
                }

                private bool _isPrimaryKey;
                public override bool IsPrimaryKey
                {
                    get { return _isPrimaryKey; }
                }

                public override bool ShouldSerialize
                {
                    get { return true; }
                }
            }

            private sealed class ComputationValueManager : ValueManager
            {
                public ComputationValueManager(Column<T> column)
                    : base(column)
                {
                }

                public override bool IsPrimaryKey
                {
                    get { return false; }
                }

                public override bool ShouldSerialize
                {
                    get { return false; }
                }
            }

            protected ValueManager(Column<T> column)
            {
                _column = column;
            }

            private Column<T> _column;

            private Model Model
            {
                get { return _column.ParentModel; }
            }


            public abstract bool IsPrimaryKey { get; }

            public abstract bool ShouldSerialize { get; }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Column{T}"/> object.
        /// </summary>
        protected Column()
        {
        }

        internal sealed override void InitValueManager()
        {
            if (IsConcrete)
                _valueManager = ValueManager.Create(this);
        }

        internal sealed override void InitAsChild(Column parentColumn)
        {
            if (IsExpression)
                throw new InvalidOperationException(Strings.Column_ComputationNotAllowedForChildColumn(this.Name));
            SetComputation((Column<T>)parentColumn, false, false);
        }

        private IValueManager _valueManager;

        public sealed override bool IsPrimaryKey
        {
            get { return _valueManager == null ? GetIsPrimaryKey() : _valueManager.IsPrimaryKey; }
        }

        private bool GetIsPrimaryKey()
        {
            var parentModel = ParentModel;
            if (parentModel == null)
                return false;
            var primaryKey = parentModel.PrimaryKey;
            if (primaryKey == null)
                return false;
            return primaryKey.Contains(this);
        }

        /// <summary>Gets the expression of this column.</summary>
        /// <value>The expression of this column.</value>
        public ColumnExpression<T> Expression { get; internal set; }

        public sealed override ColumnExpression GetExpression()
        {
            return Expression;
        }

        public sealed override IColumns BaseColumns
        {
            get { return IsExpression ? Expression.BaseColumns : this; }
        }

        /// <summary>Gets or sets the value of this column from provided <see cref="DataRow"/> object.</summary>
        /// <param name="dataRow">The provided <see cref="DataRow"/> object.</param>
        /// <returns>The value of this column from provided <see cref="DataRow"/> object.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="dataRow"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">This column does not belong to provided <paramref name="dataRow"/>.</exception>
        /// <exception cref="InvalidOperationException">This column is read only when setting the value.</exception>
        /// <seealso cref="IsReadOnly(DataRow)"/>
        public T this[DataRow dataRow]
        {
            get
            {
                if (ParentModel == null && Expression != null)
                    return Expression[dataRow];
                var translatedDataRow = VerifyDataRow(dataRow, nameof(dataRow), true);
                return InternalGetValue(dataRow, translatedDataRow);
            }
            set
            {
                VerifyDataRow(dataRow, nameof(dataRow), false);
                InternalSetValue(dataRow, value);
            }
        }

        private T InternalGetValue(DataRow dataRow, DataRow translatedDataRow)
        {
            if (_valueManager == null)
                return Expression[dataRow];
            else if (translatedDataRow == ParentModel.EditingRow)
                return Expression != null ? Expression[dataRow] : _editingValue;
            else
                return _valueManager[translatedDataRow.Ordinal];
        }

        private void InternalSetValue(DataRow dataRow, T value)
        {
            Debug.Assert(dataRow != null);
            if (InternalIsReadOnly(dataRow))
                throw new InvalidOperationException(Strings.Column_SetReadOnlyValue(this));

            SetValueCore(dataRow, value);
        }

        private void SetValueCore(DataRow dataRow, T value)
        {
            if (dataRow == ParentModel.EditingRow)
                _editingValue = value;
            else
                UpdateValue(dataRow, value);
        }

        private void UpdateValue(DataRow dataRow, T value)
        {
            var ordinal = dataRow.Ordinal;
            bool areEqual = AreEqual(_valueManager[ordinal], value);
            if (!areEqual)
            {
                _valueManager[ordinal] = value;
                dataRow.OnValueChanged(this);
            }
        }

        private DataRow VerifyDataRow(DataRow dataRow, string paramName, bool allowTranslate)
        {
            Check.NotNull(dataRow, paramName);
            if (ParentModel == null)
                throw new ArgumentException(Strings.Column_InvalidDataRow, paramName);

            if (dataRow == ParentModel.EditingRow)
                return dataRow;

            if (allowTranslate)
            {
                var translatedDataRow = Translate(dataRow);
                if (translatedDataRow == null)
                    throw new ArgumentException(Strings.Column_InvalidDataRow, paramName);
                return translatedDataRow;
            }
            else
            {
                if (dataRow.Model != ParentModel)
                    throw new ArgumentException(Strings.Column_InvalidDataRow, paramName);
                return dataRow;
            }
        }

        private DataRow Translate(DataRow dataRow)
        {
            Debug.Assert(ParentModel != null);

            // If current model is ancestor of DataRow model, returns the ancestor DataRow.
            // This allows direct reference of ancestor column as expression.
            for (var result = dataRow; result != null; result = result.ParentDataRow)
            {
                if (result.Model == ParentModel)
                    return result;
            }
            return null;
        }

        /// <summary>Gets a value indicates whether this column is readonly for provided <see cref="DataRow"/> object.</summary>
        /// <param name="dataRow">The <see cref="DataRow"/> object.</param>
        /// <returns><see langword="true"/> if this column is readonly for provided <paramref name="dataRow"/>, otherwise <see langword="false"/>.</returns>
        public bool IsReadOnly(DataRow dataRow)
        {
            VerifyDataRow(dataRow, nameof(dataRow), false);
            return InternalIsReadOnly(dataRow);
        }

        private bool InternalIsReadOnly(DataRow dataRow)
        {
            if (IsExpression)
                return true;
            return IsPrimaryKey ? dataRow.IsPrimaryKeySealed : false;
        }

        /// <summary>Gets or sets the value of this column from provided <see cref="DataRow"/> ordinal.</summary>
        /// <param name="ordinal">The provided <see cref="DataRow"/> ordinal.</param>
        /// <returns>The value of this column from provided <see cref="DataRow"/> ordinal.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <see cref="DataRow"/> ordinal is out of range.</exception>
        /// <exception cref="InvalidOperationException">This column is readonly when setting the value.</exception>
        /// <seealso cref="IsReadOnly(int)"/>
        public T this[int ordinal]
        {
            get { return this[null, ordinal]; }
            set { this[null, ordinal] = value; }
        }

        public T this[DataRow parentDataRow, int index]
        {
            get
            {
                var model = GetModel();
                if (model == null)
                    throw new ArgumentOutOfRangeException(nameof(index));
                var dataRow = GetDataRow(model, parentDataRow, index);
                return InternalGetValue(dataRow, dataRow);
            }
            set
            {
                var model = GetModel();
                if (model == null)
                    throw new ArgumentOutOfRangeException(nameof(index));
                var dataRow = GetDataRow(ParentModel, parentDataRow, index);
                InternalSetValue(dataRow, value);
            }
        }

        private Model GetModel()
        {
            if (ParentModel != null)
                return ParentModel;
            else if (ScalarSourceModels != null && ScalarSourceModels.Count == 1)
                return ScalarSourceModels.Single();
            else
                return null;
        }

        private static DataRow GetDataRow(Model model, DataRow parentDataRow, int index)
        {
            var dataSet = GetDataSet(model, parentDataRow);
            return dataSet[index];
        }

        private static DataSet GetDataSet(Model parentModel, DataRow parentDataRow)
        {
            return parentDataRow == null ? parentModel.DataSet : parentDataRow[parentModel];
        }

        internal sealed override void InsertRow(DataRow dataRow)
        {
            if (_valueManager != null)
                InsertRow(dataRow, GetDefaultValue());
        }

        internal void InsertRow(DataRow dataRow, T value)
        {
            Debug.Assert(_valueManager != null);
            _valueManager.Insert(dataRow.Ordinal, value);
        }

        internal sealed override void RemoveRow(DataRow dataRow)
        {
            if (_valueManager != null)
                _valueManager.RemoveAt(dataRow.Ordinal);
        }

        /// <inheritdoc/>
        public override bool IsExpression
        {
            get { return Expression != null; }
        }

        /// <inheritdoc/>
        public sealed override Type DataType
        {
            get { return typeof(T); }
        }

        private DbExpression _dbExpression;
        /// <inheritdoc/>
        public sealed override DbExpression DbExpression
        {
            get
            {
                return _dbExpression ?? (_dbExpression = IsAbsoluteExpression ? Expression.GetDbExpression() : new DbColumnExpression(this));
            }
        }

        public sealed override DbExpression DbComputedExpression
        {
            get { return IsDbComputed ? Expression.GetDbExpression() : null; }
        }

        /// <inheritdoc/>
        protected internal virtual bool IsNull(T value)
        {
            return value == null;
        }

        /// <inheritdoc/>
        public sealed override bool IsNull(DataRow dataRow)
        {
            return IsNull(this[dataRow]);
        }

        /// <inheritdoc/>
        public sealed override IModels ScalarSourceModels
        {
            get { return IsAbsoluteExpression ? Expression.ScalarSourceModels : ParentModel; }
        }

        /// <inheritdoc/>
        public sealed override IModels AggregateSourceModels
        {
            get { return IsAbsoluteExpression ? Expression.AggregateSourceModels : Models.Empty; }
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="value">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        protected internal abstract Column<T> CreateConst(T value);

        internal sealed override bool ShouldSerializeOverride
        {
            get { return _valueManager == null ? false : _valueManager.ShouldSerialize; }
        }
        
        /// <inheritdoc/>
        protected internal sealed override Column CreateParam(DataRow dataRow)
        {
            return CreateParam(this[dataRow]);
        }

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="value">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        protected abstract Column<T> CreateParam(T value);

        internal sealed override Default CreateDefault()
        {
            return new Default<T>(this);
        }

        /// <summary>Defines the default constant value for this column.</summary>
        /// <param name="defaultValue">The default constant value.</param>
        /// <remarks>To define default expression value, call <see cref="ColumnExtensions.Default{T}(T, T)"/> method.</remarks>
        public void DefaultValue(T defaultValue)
        {
            AddOrUpdateInterceptor(new Default<T>(CreateConst(defaultValue)));
        }

        /// <summary>Gets the default declaration for this column.</summary>
        /// <returns>The default declaration for this column. Returns <see langword="null"/> if no default defined.</returns>
        public new Default<T> GetDefault()
        {
            return GetInterceptor<Default<T>>();
        }

        /// <inheritdoc/>
        internal sealed override JsonValue Serialize(int rowOrdinal)
        {
            return SerializeValue(_valueManager[rowOrdinal]);
        }

        /// <summary>Serializes value into JSON.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The serialized JSON value.</returns>
        protected internal abstract JsonValue SerializeValue(T value);

        /// <inheritdoc/>
        internal sealed override void Deserialize(int ordinal, JsonValue value)
        {
            _valueManager[ordinal] = DeserializeValue(value);
        }

        /// <summary>Deserializes from JSON value.</summary>
        /// <param name="value">The JSON value.</param>
        /// <returns>The deserialized value.</returns>
        protected internal abstract T DeserializeValue(JsonValue value);

        internal virtual T GetDefaultValue()
        {
            var defaultDef = this.GetDefault();
            return defaultDef != null ? defaultDef.Value : default(T);
        }

        private sealed class ComputationExpression : ColumnExpression<T>
        {
            internal ComputationExpression(Column<T> computation)
            {
                Debug.Assert(computation != null);
                _computation = computation;
            }

            public override T this[DataRow dataRow]
            {
                get { return _computation[dataRow]; }
            }

            private Column<T> _computation;
            public override DbExpression GetDbExpression()
            {
                return _computation.DbExpression;
            }

            protected override IModels GetAggregateBaseModels()
            {
                return _computation.AggregateSourceModels;
            }

            protected override IModels GetScalarSourceModels()
            {
                return _computation.ScalarSourceModels;
            }

            protected override IColumns GetBaseColumns()
            {
                return _computation.BaseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var computation = _computation.TranslateTo(model);
                if (computation != _computation)
                    return new ComputationExpression(computation);
                else
                    return this;
            }
        }

        /// <summary>Defines the computation expression for this column.</summary>
        /// <param name="computation">The computation expression.</param>
        public void ComputedAs(Column<T> computation, bool isConcrete = true, bool isDbComputed = true)
        {
            Check.NotNull(computation, nameof(computation));
            if (ParentModel == null)
                throw new InvalidOperationException(Strings.Column_ComputedColumnMustBeMemberOfModel);
            if (Expression != null)
                throw new InvalidOperationException(Strings.Column_AlreadyComputed);

            VerifyDesignMode();
            SetComputation(computation, isConcrete, isDbComputed);
        }

        private void SetComputation(Column<T> computation, bool isConcrete, bool isDbComputed)
        {
            new ComputationExpression(computation).SetOwner(this);
            _isConcrete = isConcrete;
            _isDbComputed = isDbComputed;
        }

        private bool? _isConcrete;
        internal void SetIsConcrete(bool? value)
        {
            _isConcrete = value;
        }

        public sealed override bool IsConcrete
        {
            get { return Expression == null ? true : _isConcrete == true; }
        }

        internal sealed override void TryMakeConcrete()
        {
            if (!IsConcrete && !_isConcrete.HasValue)
            {
                _isConcrete = true;
                Debug.Assert(_valueManager == null);
                _valueManager = ValueManager.EnsureConcrete(this);
            }
        }

        internal sealed override void RefreshComputation(DataRow dataRow)
        {
            Debug.Assert(Expression != null);
            Debug.Assert(dataRow != ParentModel.EditingRow);
            if (_valueManager == null)
            {
                dataRow.OnValueChanged(this);
                return;
            }

            var computationValue = Expression[dataRow];
            UpdateValue(dataRow, computationValue);
        }

        private bool _isDbComputed;
        public sealed override bool IsDbComputed
        {
            get { return _isDbComputed; }
        }

        public sealed override ColumnMapping MapFrom(Column column)
        {
            return ColumnMapping.Map((Column<T>)column, this);
        }

        public sealed override object GetValue(DataRow dataRow)
        {
            return this[dataRow];
        }

        public sealed override void SetValue(DataRow dataRow, object value)
        {
            this[dataRow] = (T)value;
        }

        public virtual bool AreEqual(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }

        internal sealed override void CopyValue(DataRow sourceDataRow, Column targetColumn, DataRow targetDataRow)
        {
            var target = (Column<T>)targetColumn;
            if (!target.IsReadOnly(targetDataRow))
                target[targetDataRow] = this[sourceDataRow];
        }

        private T _editingValue;
        internal sealed override void BeginEdit(DataRow dataRow)
        {
            _editingValue = dataRow == DataRow.Placeholder || _valueManager == null ? GetDefaultValue() : _valueManager[dataRow.Ordinal];
        }

        internal sealed override void EndEdit(DataRow dataRow)
        {
            if (_valueManager != null)
                UpdateValue(dataRow, _editingValue);
        }

        public sealed override bool HasDefaultComparer
        {
            get { return Comparer<T>.Default != null; }
        }

        public int Compare(DataRow x, DataRow y, SortDirection direction = SortDirection.Ascending, IComparer<T> comparer = null)
        {
            comparer = Verify(comparer, nameof(comparer));
            var result = comparer.Compare(this[x], this[y]);
            if (direction == SortDirection.Descending)
                result *= -1;
            return result;
        }

        private IComparer<T> Verify(IComparer<T> comparer, string paramName)
        {
            if (comparer == null)
            {
                comparer = Comparer<T>.Default;
                if (comparer == null)
                    throw new ArgumentNullException(paramName, Strings.Column_NoDefaultComparer);
            }
            return comparer;
        }

        public sealed override int Compare(DataRow x, DataRow y, SortDirection direction)
        {
            return Compare(x, y, direction, null);
        }

        internal sealed override Column CreateBackup(Model model)
        {
            return model.DataSetContainer.CreateLocalColumn<T>(model);
        }

        internal IColumnComparer ToColumnComparer(SortDirection direction, IComparer<T> valueComparer)
        {
            Debug.Assert(ScalarSourceModels.Count == 1);
            valueComparer = Verify(valueComparer, nameof(valueComparer));
            return DataRowComparer.Create(this, direction, valueComparer);
        }

        internal sealed override IColumnComparer ToColumnComparer(SortDirection direction)
        {
            return ToColumnComparer(direction, ValueComparer);
        }

        private IComparer<T> _valueComparer;
        public IComparer<T> ValueComparer
        {
            get { return _valueComparer ?? Comparer<T>.Default; }
            set
            {
                VerifyDesignMode();
                _valueComparer = value;
            }
        }

        public sealed override bool HasValueComparer
        {
            get { return ValueComparer != null; }
        }

        internal sealed override Column PerformTranslateTo(Model model)
        {
            Debug.Assert(model != null);

            if (ParentModel != null)
            {
                if (model == ParentModel)
                    return this;

                if (model.GetType() != ParentModel.GetType())
                    throw new ArgumentException(Strings.Column_TranslateToModelTypeMismatch, nameof(model));

                if (IsLocal)
                    return model.LocalColumns[Ordinal];
                else
                    return model.Columns[Ordinal];
            }

            var translatedExpression = Expression.PerformTranslateTo(model);
            if (Expression == translatedExpression)
                return this;
            var result = (Column<T>)Activator.CreateInstance(GetType());
            translatedExpression.SetOwner(result);
            return result;
        }
    }
}
