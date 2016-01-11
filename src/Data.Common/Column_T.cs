﻿using DevZest.Data.Primitives;
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
    public abstract class Column<T> : Column, IColumn
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
        }

        private sealed class ChildValueManager : IValueManager
        {
            public ChildValueManager(DataSet dataSet, Column<T> parentColumn)
            {
                Debug.Assert(dataSet != null);
                Debug.Assert(parentColumn != null);
                Debug.Assert(dataSet.Model.ParentModel == parentColumn.ParentModel);
                _dataSet = dataSet;
                _parentColumn = parentColumn;
            }

            private DataSet _dataSet;
            private Column<T> _parentColumn;

            public int RowCount
            {
                get { return _dataSet.Count; }
            }

            public bool ShouldSerialize
            {
                get { return false; }
            }

            public bool IsReadOnly(int ordinal)
            {
                return true;
            }

            public T this[int ordinal]
            {
                get { return _parentColumn[_dataSet[ordinal].ParentDataRow.Ordinal]; }
                set { Debug.Fail("Child column is readonly."); }
            }

            public void AddRow(DataRow dataRow)
            {
            }

            public void RemoveRow(DataRow dataRow)
            {
            }

            public void ClearRows()
            {
            }
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
                _values.Insert(dataRow.Ordinal, _column.GetDefaultValue(dataRow));
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
        }

        private sealed class ComputedValueManager : IValueManager
        {
            public ComputedValueManager(DataSet dataSet, Column<T> computation)
            {
                Debug.Assert(dataSet != null);
                Debug.Assert(computation != null);
                _dataSet = dataSet;
                _computation = computation;
            }

            private DataSet _dataSet;
            private Column<T> _computation;

            public T this[int ordinal]
            {
                get { return _computation[_dataSet[ordinal]]; }
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
            }

            public void ClearRows()
            {
            }

            public bool IsReadOnly(int ordinal)
            {
                return true;
            }

            public void RemoveRow(DataRow dataRow)
            {
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
            var dataSet = ParentModel.DataSet;
            Debug.Assert(dataSet != null);
            _valueManager = CreateValueManager(dataSet, this);
        }

        private static IValueManager CreateValueManager(DataSet dataSet, Column<T> column)
        {
            var parentColumn = GetParentColumn(column);
            if (parentColumn != null)
                return new ChildValueManager(dataSet, parentColumn);

            var computation = column.Computation;
            if (computation != null)
                return new ComputedValueManager(dataSet, computation);

            return new ListValueManager(column);
        }

        private static Column<T> GetParentColumn(Column<T> column)
        {
            var model = column.ParentModel;
            var parentMappings = model.ParentMappings;
            if (parentMappings == null)
                return null;
            foreach (var parentMapping in parentMappings)
            {
                if (parentMapping.Source == column.DbExpression)
                    return (Column<T>)parentMapping.TargetColumn;
            }
            return null;
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

        private ColumnExpression<T> _expression;
        /// <summary>Gets the expression of this column.</summary>
        /// <value>The expression of this column.</value>
        public ColumnExpression<T> Expression
        {
            get { return _expression; }
            internal set
            {
                Check.NotNull(value, nameof(value));
                if (value.Owner != null)
                    throw new ArgumentException(Strings.Column_ExpressionAlreadyAttached, nameof(value));
                if (_expression != null)
                    throw new InvalidOperationException(Strings.Column_ExpressionOverwrite);
                if (ParentModel != null)
                    throw new InvalidOperationException(Strings.Column_ExpressionModelProperty);

                value.Owner = this;
                _expression = value;
            }
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
                {
                    Check.NotNull(dataRow, nameof(dataRow));
                    return _expression[dataRow];
                }

                VerifyDataRow(dataRow, nameof(dataRow));
                return ValueManager[dataRow.Ordinal];
            }
            set
            {
                VerifyDataRow(dataRow, nameof(dataRow));
                SetValue(dataRow, value);
            }
        }

        public T Eval()
        {
            if (!IsExpression)
                throw new InvalidOperationException(Strings.Column_Eval_NullExpression);
            return _expression.Eval();
        }

        private void SetValue(DataRow dataRow, T value)
        {
            Debug.Assert(dataRow != null);
            if (IsReadOnly(dataRow.Ordinal))
                throw new InvalidOperationException(Strings.Column_SetReadOnlyValue(this));

            UpdateValue(dataRow, value);
        }

        private void UpdateValue(DataRow dataRow, T value)
        {
            var ordinal = dataRow.Ordinal;
            bool areEqual = AreEqual(ValueManager[ordinal], value);
            ValueManager[ordinal] = value;
            if (!areEqual)
                OnValueChanged(dataRow);
        }

        private void VerifyDataRow(DataRow dataRow, string paramName)
        {
            Check.NotNull(dataRow, paramName);
            if (dataRow.Model != ParentModel)
                throw new ArgumentException(Strings.Column_VerifyDataRow, paramName);
        }

        /// <summary>Gets a value indicates whether this column is readonly for provided <see cref="DataRow"/> object.</summary>
        /// <param name="dataRow">The <see cref="DataRow"/> object.</param>
        /// <returns><see langword="true"/> if this column is readonly for provided <paramref name="dataRow"/>, otherwise <see langword="false"/>.</returns>
        public bool IsReadOnly(DataRow dataRow)
        {
            VerifyDataRow(dataRow, nameof(dataRow));
            return IsReadOnly(dataRow.Ordinal);
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
            return IsReadOnly(GetOrdinal(parentDataRow, childOrdinal));
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
                var ordinal = GetOrdinal(parentDataRow, childOrdinal);
                return ValueManager[ordinal];
            }
            set
            {
                var dataRow = GetDataRow(parentDataRow, childOrdinal);
                SetValue(dataRow, value);
            }
        }

        private int GetOrdinal(DataRow parentDataRow, int childOrdinal)
        {
            return parentDataRow == null ? childOrdinal : GetDataRow(parentDataRow, childOrdinal).Ordinal;
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

        private T _scalarValue;

        /// <inheritdoc/>
        public override bool IsExpression
        {
            get { return _expression != null; }
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
                return _dbExpression ?? (_dbExpression = _expression == null ? new DbColumnExpression(this) : _expression.GetDbExpression());
            }
        }

        /// <inheritdoc/>
        protected internal abstract bool IsNull(T value);

        /// <inheritdoc/>
        public sealed override bool IsNull(DataRow dataRow)
        {
            return IsNull(this[dataRow]);
        }

        public sealed override bool IsEvalNull
        {
            get { return IsNull(Eval()); }
        }

        /// <inheritdoc/>
        public sealed override IModelSet ParentModelSet
        {
            get { return _expression == null ? ParentModel : _expression.ParentModelSet; }
        }

        /// <inheritdoc/>
        public sealed override IModelSet AggregateModelSet
        {
            get { return _expression == null ? ModelSet.Empty : _expression.AggregateModelSet; }
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

        internal sealed override Column GetParrallel(Model model)
        {
            Debug.Assert(model.GetType() == this.ParentModel.GetType());
            return IsExpression ? _expression.GetParallelColumn(model) : model.Columns[Ordinal];
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
        protected internal sealed override JsonValue Serialize(int rowOrdinal)
        {
            return SerializeValue(this[rowOrdinal]);
        }

        /// <summary>Serializes value into JSON.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The serialized JSON value.</returns>
        protected internal abstract JsonValue SerializeValue(T value);

        /// <inheritdoc/>
        protected internal sealed override void Deserialize(int ordinal, JsonValue value)
        {
            this[ordinal] = DeserializeValue(value);
        }

        /// <summary>Deserializes from JSON value.</summary>
        /// <param name="value">The JSON value.</param>
        /// <returns>The deserialized value.</returns>
        protected internal abstract T DeserializeValue(JsonValue value);

        internal virtual T GetDefaultValue(DataRow dataRow)
        {
            var defaultDef = this.GetDefault();
            return defaultDef != null ? defaultDef.DefaultValue[dataRow] : default(T);
        }

        /// <summary>Defines the computation expression for this column.</summary>
        /// <param name="computation">The computation expression.</param>
        public void ComputedAs(Column<T> computation, bool isDbComputed = true)
        {
            Check.NotNull(computation, nameof(computation));

            VerifyDesignMode();
            Computation = computation;
            _isDbComputed = isDbComputed;
        }

        private bool _isDbComputed;
        public sealed override bool IsDbComputed
        {
            get { return _isDbComputed; }
        }

        /// <summary>Gets the computation expression for this column.</summary>
        public Column<T> Computation { get; private set; }

        /// <inheritdoc/>
        public sealed override Column GetComputation()
        {
            return Computation;
        }

        /// <inheritdoc/>
        public sealed override void Compute(DataRow dataRow)
        {
            VerifyDataRow(dataRow, nameof(dataRow));
            if (Computation == null)
                return;
            this[dataRow] = Computation[dataRow];
        }

        /// <summary>Mapping this column with another column.</summary>
        /// <param name="sourceColumn">The source column.</param>
        /// <returns>The <see cref="ColumnMapping"/> contains <paramref name="sourceColumn"/> and this column.</returns>
        public ColumnMapping From(Column<T> sourceColumn)
        {
            Check.NotNull(sourceColumn, nameof(sourceColumn));
            return new ColumnMapping(sourceColumn, this);
        }

        /// <summary>Mapping this column with constant value.</summary>
        /// <param name="value">The constant value.</param>
        /// <returns>The <see cref="ColumnMapping"/> contains the constant value and this column.</returns>
        public ColumnMapping From(T value)
        {
            return new ColumnMapping(CreateParam(value), this);
        }

        /// <inheritdoc/>
        public sealed override ColumnMapping From(Column column)
        {
            return From((Column<T>)column);
        }

        public sealed override object GetValue(DataRow dataRow)
        {
            return this[dataRow];
        }

        public sealed override void SetValue(DataRow dataRow, object value)
        {
            this[dataRow] = (T)value;
        }

        protected abstract bool AreEqual(T x, T y);

        internal sealed override void Save(DataRow dataRow)
        {
            var listValues = _valueManager as ListValueManager;
            if (listValues != null)
                _scalarValue = listValues[dataRow.Ordinal];
        }

        internal sealed override void Load(DataRow dataRow)
        {
            var listValues = _valueManager as ListValueManager;
            if (listValues != null)
                UpdateValue(dataRow, _scalarValue);
        }
    }
}
