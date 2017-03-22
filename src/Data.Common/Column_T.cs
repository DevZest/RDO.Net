using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            int RowCount { get; }
            bool ShouldSerialize { get; }
            bool IsReadOnly(int ordinal);
            T this[int ordinal] { get; set; }
            void AddRow(DataRow dataRow);
            void RemoveRow(DataRow dataRow);
            void ClearRows();
            void EnsureConcrete();
            T GetComputationValue(DataRow dataRow);
            void RefreshComputationValue(DataRow dataRow, T value);
        }

        private sealed class ListValueManager : IValueManager
        {
            public ListValueManager(Column<T> column)
            {
                Debug.Assert(column != null);
                _column = column;
                _isPrimaryKey = GetIsPrimaryKey();
            }

            private List<T> _values = new List<T>();
            private Column<T> _column;

            bool _isPrimaryKey;

            private bool GetIsPrimaryKey()
            {
                var primaryKey = Model.PrimaryKey;
                if (primaryKey == null)
                    return false;

                for (int i = 0; i < primaryKey.Count; i++)
                {
                    if (_column == primaryKey[i].Column)
                        return true;
                }
                return false;
            }

            private Model Model
            {
                get { return _column.ParentModel; }
            }

            public int RowCount
            {
                get { return _values.Count; }
            }

            public bool ShouldSerialize
            {
                get { return true; }
            }

            public bool IsReadOnly(int ordinal)
            {
                if (Model.IsKeyUpdateAllowed)
                    return false;
                if (_isPrimaryKey)
                    return !_column.IsNull(_values[ordinal]);
                return false;
            }

            public T this[int ordinal]
            {
                get { return _values[ordinal]; }
                set { _values[ordinal] = value; }
            }

            public void AddRow(DataRow dataRow)
            {
                Debug.Assert(Model == dataRow.Model);
                _values.Insert(dataRow.Ordinal, _column.GetDefaultValue());
            }

            public void RemoveRow(DataRow dataRow)
            {
                Debug.Assert(dataRow != null);
                Debug.Assert(dataRow.Model == Model);
                _values.RemoveAt(dataRow.Ordinal);
            }

            public void ClearRows()
            {
                _values.Clear();
            }

            public void EnsureConcrete()
            {
            }

            public T GetComputationValue(DataRow dataRow)
            {
                return this[dataRow.Ordinal];
            }

            public void RefreshComputationValue(DataRow dataRow, T value)
            {
            }
        }

        private sealed class ExpressionValueManager : IValueManager
        {
            public ExpressionValueManager(DataSet dataSet, ColumnExpression<T> expression, bool cacheValues)
            {
                Debug.Assert(dataSet != null);
                Debug.Assert(expression != null);
                _dataSet = dataSet;
                _expression = expression;
                if (cacheValues)
                    EnsureConcrete();
            }

            private DataSet _dataSet;
            private ColumnExpression<T> _expression;
            private List<T> _cachedValues;

            public T this[int ordinal]
            {
                get { return _cachedValues != null ? _cachedValues[ordinal] : GetComputationValue(_dataSet[ordinal]); }
                set { Debug.Fail("Computed column is read only."); }
            }

            public int RowCount
            {
                get { return _dataSet.Count; }
            }

            public bool ShouldSerialize
            {
                get { return false; }
            }

            public void AddRow(DataRow dataRow)
            {
                if (_cachedValues != null)
                    _cachedValues.Insert(dataRow.Ordinal, default(T));
            }

            public void ClearRows()
            {
                if (_cachedValues != null)
                    _cachedValues.Clear();
            }

            public bool IsReadOnly(int ordinal)
            {
                return true;
            }

            public void RemoveRow(DataRow dataRow)
            {
                if (_cachedValues != null)
                    _cachedValues.RemoveAt(dataRow.Ordinal);
            }

            public void EnsureConcrete()
            {
                if (_cachedValues == null)
                {
                    var cachedValues = new List<T>();
                    for (int i = 0; i < _dataSet.Count; i++)
                        cachedValues.Add(this[i]);
                    _cachedValues = cachedValues;
                }
            }

            public T GetComputationValue(DataRow dataRow)
            {
                return _expression[dataRow];
            }

            public void RefreshComputationValue(DataRow dataRow, T value)
            {
                if (_cachedValues != null)
                    _cachedValues[dataRow.Ordinal] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Column{T}"/> object.
        /// </summary>
        protected Column()
        {
        }

        internal sealed override void InitValueManager()
        {
            _valueManager = CreateValueManager();
        }

        private IValueManager CreateValueManager()
        {
            if (IsExpression)
                return new ExpressionValueManager(ParentModel.DataSet, Expression, _isConcrete);

            return new ListValueManager(this);
        }

        internal sealed override void InitAsChild(Column parentColumn)
        {
            if (IsExpression)
                throw new InvalidOperationException(Strings.Column_ComputationNotAllowedForChildColumn(this.Name));
            SetComputation((Column<T>)parentColumn, 1, false, false);
        }

        private IValueManager _valueManager;

        private IValueManager ValueManager
        {
            get
            {
                if (_valueManager == null)
                    throw new InvalidOperationException(Strings.Column_NullValueManager);
                return _valueManager;
            }
        }

        /// <summary>Gets the expression of this column.</summary>
        /// <value>The expression of this column.</value>
        public ColumnExpression<T> Expression { get; internal set; }

        public sealed override ColumnExpression GetExpression()
        {
            return Expression;
        }

        public sealed override IColumnSet BaseColumns
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
                if (IsExpression)
                    return Expression[dataRow];

                VerifyDataRow(dataRow, nameof(dataRow));
                return ValueOf(dataRow);
            }
            set
            {
                VerifyDataRow(dataRow, nameof(dataRow));
                SetValue(dataRow, value);
            }
        }

        private T ValueOf(DataRow dataRow)
        {
            return dataRow == ParentModel.EditingRow ? _editingValue : ValueManager[dataRow.Ordinal];
        }

        private void SetValue(DataRow dataRow, T value)
        {
            Debug.Assert(dataRow != null);
            if (IsReadOnly(dataRow))
                throw new InvalidOperationException(Strings.Column_SetReadOnlyValue(this));

            if (dataRow == ParentModel.EditingRow)
                _editingValue = value;
            else
                UpdateValue(dataRow, value);
        }

        private void UpdateValue(DataRow dataRow, T value)
        {
            var ordinal = dataRow.Ordinal;
            bool areEqual = AreEqual(ValueManager[ordinal], value);
            ValueManager[ordinal] = value;
            if (!areEqual)
                dataRow.OnUpdated(this);
        }

        private void VerifyDataRow(DataRow dataRow, string paramName)
        {
            Check.NotNull(dataRow, paramName);
            if (dataRow != ParentModel.EditingRow && dataRow.Model != ParentModel)
                throw new ArgumentException(Strings.Column_VerifyDataRow, paramName);
        }

        /// <summary>Gets a value indicates whether this column is readonly for provided <see cref="DataRow"/> object.</summary>
        /// <param name="dataRow">The <see cref="DataRow"/> object.</param>
        /// <returns><see langword="true"/> if this column is readonly for provided <paramref name="dataRow"/>, otherwise <see langword="false"/>.</returns>
        public bool IsReadOnly(DataRow dataRow)
        {
            VerifyDataRow(dataRow, nameof(dataRow));
            return dataRow == DataRow.Placeholder ? false : IsReadOnly(dataRow.Ordinal);
        }

        /// <summary>Gets a value indicates whether this column is readonly for provided <see cref="DataRow"/> ordinal.</summary>
        /// <param name="ordinal">The <see cref="DataRow"/> ordinal.</param>
        /// <returns><see langword="true"/> if this column is readonly for provided <see cref="DataRow"/> oridinal, otherwise <see langword="false"/>.</returns>
        public bool IsReadOnly(int ordinal)
        {
            if (IsExpression)
                return true;
            return ValueManager.IsReadOnly(ordinal);
        }

        public bool IsReadOnly(DataRow parentDataRow, int childOrdinal)
        {
            return IsReadOnly(GetDataRow(parentDataRow, childOrdinal));
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

        public T this[DataRow parentDataRow, int childOrdinal]
        {
            get
            {
                var dataRow = GetDataRow(parentDataRow, childOrdinal);
                return ValueOf(dataRow);
            }
            set
            {
                var dataRow = GetDataRow(parentDataRow, childOrdinal);
                SetValue(dataRow, value);
            }
        }

        private DataRow GetDataRow(DataRow parentDataRow, int childOrdinal)
        {
            var dataSet = GetDataSet(parentDataRow);
            return dataSet[childOrdinal];
        }

        private DataSet GetDataSet(DataRow parentDataRow)
        {
            var model = ParentModel;
            if (parentDataRow == null)
                return model.DataSet;
            if (parentDataRow.Model != model.ParentModel)
                throw new ArgumentException(Strings.Column_InvalidParentDataRow, nameof(parentDataRow));
            return parentDataRow[model];
        }

        internal sealed override void InsertRow(DataRow dataRow)
        {
            _valueManager.AddRow(dataRow);
        }

        internal sealed override void RemoveRow(DataRow dataRow)
        {
            _valueManager.RemoveRow(dataRow);
        }

        internal override void ClearRows()
        {
            _valueManager.ClearRows();
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
        protected internal abstract bool IsNull(T value);

        /// <inheritdoc/>
        public sealed override bool IsNull(DataRow dataRow)
        {
            return IsNull(this[dataRow]);
        }

        /// <inheritdoc/>
        public sealed override IModelSet ScalarSourceModels
        {
            get { return IsAbsoluteExpression ? Expression.ScalarSourceModels : ParentModel; }
        }

        /// <inheritdoc/>
        public sealed override IModelSet AggregateSourceModels
        {
            get { return IsAbsoluteExpression ? Expression.AggregateSourceModels : ModelSet.Empty; }
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="value">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        protected internal abstract Column<T> CreateConst(T value);

        internal sealed override bool ShouldSerializeOverride
        {
            get
            {
                Debug.Assert(_valueManager != null);
                return _valueManager.ShouldSerialize;
            }
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
            return SerializeValue(this[rowOrdinal]);
        }

        /// <summary>Serializes value into JSON.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The serialized JSON value.</returns>
        protected internal abstract JsonValue SerializeValue(T value);

        /// <inheritdoc/>
        internal sealed override void Deserialize(int ordinal, JsonValue value)
        {
            this[ordinal] = DeserializeValue(value);
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

        [ExpressionConverterGenerics(typeof(Column<>.ComputationExpression.Converter), Id = "Column<>.Computation")]
        private sealed class ComputationExpression : ColumnExpression<T>
        {
            /// <remarks>
            /// Every ColumnExpression requires an ExpressionConverter to fail fast. However the ComputationExpression will never be serialized/deserialized,
            /// because computed columns must be the member of Model, they will always be serialized to column name, so the converter will never be executed.
            /// </remarks>
            private sealed class Converter : ExpressionConverter
            {
                internal override ColumnExpression ParseJson(JsonParser parser, Model model)
                {
                    throw new NotSupportedException();
                }

                internal override void WriteJson(JsonWriter jsonWriter, ColumnExpression expression)
                {
                    throw new NotSupportedException();
                }
            }

            internal ComputationExpression(Column<T> computation, int ancestorLevel)
            {
                Debug.Assert(computation != null);
                Debug.Assert(ancestorLevel >= 0);
                _computation = computation;
                _ancestorLevel = ancestorLevel;
            }

            private DataRow IfAncestor(DataRow dataRow)
            {
                return dataRow.AncestorOf(_ancestorLevel);
            }

            protected internal override T this[DataRow dataRow]
            {
                get { return _computation[IfAncestor(dataRow)]; }
            }

            private Column<T> _computation;
            int _ancestorLevel;

            public override DbExpression GetDbExpression()
            {
                return _computation.DbExpression;
            }

            protected override IModelSet GetAggregateBaseModels()
            {
                return _computation.AggregateSourceModels;
            }

            protected override IModelSet GetScalarSourceModels()
            {
                return _computation.ScalarSourceModels;
            }

            protected override IColumnSet GetBaseColumns()
            {
                return _computation.BaseColumns;
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
            var ancestorLevel = VerifyComputation(computation, nameof(computation));
            SetComputation(computation, ancestorLevel, isConcrete, isDbComputed);
        }

        private int VerifyComputation(Column<T> computation, string paramName)
        {
            var computationModel = computation.ParentModel;
            if (computationModel == null)
            {
                computation.VerifyScalarSourceModels(ParentModel, paramName);
                computation.VerifyAggregateSourceModels(ParentModel, paramName);
                return 0;
            }

            var ancestorLevel = computationModel.AncestorLevelOf(ParentModel);
            if (ancestorLevel.HasValue)
                return ancestorLevel.GetValueOrDefault();
            else
                throw new ArgumentException(Strings.Column_InvalidScalarSourceModel(computationModel), nameof(paramName));
        }

        private void VerifyAggregateSourceModels(Model ancestor, string exceptionParamName)
        {
            foreach (var model in AggregateSourceModels)
            {
                if (!ancestor.IsAncestorOf(model))
                    throw new ArgumentException(Strings.Column_InvalidAggregateSourceModel(model), exceptionParamName);
            }
        }

        private void SetComputation(Column<T> computation, int ancestorLevel, bool isConcrete, bool isDbComputed)
        {
            new ComputationExpression(computation, ancestorLevel).SetOwner(this);
            _isConcrete = isConcrete;
            _isDbComputed = isDbComputed;
        }

        private bool _isConcrete;
        public sealed override bool IsConcrete
        {
            get { return _isConcrete; }
        }

        internal sealed override void EnsureConcrete()
        {
            if (!_isConcrete)
            {
                _isConcrete = true;
                if (_valueManager != null)
                    _valueManager.EnsureConcrete();
            }
        }

        internal sealed override bool RefreshComputation(DataRow dataRow)
        {
            Debug.Assert(IsExpression);
            if (!IsConcrete)
                return true;

            var computationValue = _valueManager.GetComputationValue(dataRow);
            var areEqual = AreEqual(this[dataRow], computationValue);
            _valueManager.RefreshComputationValue(dataRow, computationValue);
            return !areEqual;
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
            _editingValue = dataRow == DataRow.Placeholder ? GetDefaultValue() : ValueManager[dataRow.Ordinal];
        }

        internal sealed override void EndEdit(DataRow dataRow)
        {
            if (!IsReadOnly(dataRow))
                UpdateValue(dataRow, _editingValue);
        }

        public sealed override int Compare(DataRow x, DataRow y)
        {
            return Comparer.Compare(this[x], this[y]);
        }

        protected virtual IComparer<T> Comparer
        {
            get { return Comparer<T>.Default; }
        }
    }
}
