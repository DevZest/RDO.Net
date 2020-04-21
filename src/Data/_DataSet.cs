using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents column which contains DataSet as value.
    /// </summary>
    /// <typeparam name="T">Entity type of DataSet.</typeparam>
    public sealed class _DataSet<T> : Column<DataSet<T>>, IDataSetColumn
        where T : Model, new()
    {
        /// <inheritdoc/>
        public override _String CastToString()
        {
            return null;
        }

        /// <inheritdoc/>
        public override bool AreEqual(DataSet<T> x, DataSet<T> y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(DataSet<T> value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected internal override DataSet<T> DeserializeValue(JsonValue value)
        {
            throw new NotSupportedException();
        }

        void IDataSetColumn.Serialize(int rowOrdinal, JsonWriter jsonWriter)
        {
            var dataSet = this[rowOrdinal];
            if (dataSet == null)
                jsonWriter.WriteValue(JsonValue.Null);
            else
                jsonWriter.Write(dataSet);
        }

        DataSet IDataSetColumn.NewValue(int ordinal)
        {
            return ParentModel.CreateDataSet(this, ordinal);
        }

        void IDataSetColumn.Deserialize(int rowOrdinal, DataSet value)
        {
            this[rowOrdinal] = (DataSet<T>)value;
        }

        /// <inheritdoc />
        protected internal override Column<DataSet<T>> CreateConst(DataSet<T> value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected override Column<DataSet<T>> CreateParam(DataSet<T> value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(DataSet<T> value)
        {
            return value == null;
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _DataSet<T> Const(DataSet<T> x)
        {
            return new ConstantExpression<DataSet<T>>(x).MakeColumn<_DataSet<T>>();
        }

        /// <summary>Implicitly converts the supplied value to a column of const expression.</summary>
        /// <param name="x">The value of the const expression.</param>
        /// <returns>The column of const expression.</returns>
        public static implicit operator _DataSet<T>(DataSet<T> x)
        {
            return Const(x);
        }
    }
}
