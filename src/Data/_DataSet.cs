﻿using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public sealed class _DataSet<T> : Column<DataSet<T>>, IDataSetColumn
        where T : Model, new()
    {
        public override _String CastToString()
        {
            return null;
        }

        public override bool AreEqual(DataSet<T> x, DataSet<T> y)
        {
            return x == y;
        }

        protected internal override JsonValue SerializeValue(DataSet<T> value)
        {
            throw new NotSupportedException();
        }

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
            return ParentModel.NewDataSetValue(this, ordinal);
        }

        void IDataSetColumn.Deserialize(int rowOrdinal, DataSet value)
        {
            this[rowOrdinal] = (DataSet<T>)value;
        }

        protected internal override Column<DataSet<T>> CreateConst(DataSet<T> value)
        {
            return Const(value);
        }

        protected override Column<DataSet<T>> CreateParam(DataSet<T> value)
        {
            throw new NotSupportedException();
        }

        protected internal override bool IsNull(DataSet<T> value)
        {
            return value == null;
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _DataSet<T> Const(DataSet<T> x)
        {
            return new ConstantExpression<DataSet<T>>(x).MakeColumn<_DataSet<T>>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _DataSet<T>(DataSet<T> x)
        {
            return Const(x);
        }
    }
}