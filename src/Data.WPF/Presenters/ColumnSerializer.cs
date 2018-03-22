using DevZest.Data.Presenters.Primitives;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public abstract class ColumnSerializer
    {
        public static ColumnSerializer Create(Column column)
        {
            Check.NotNull(column, nameof(column));
            return new DefaultColumnSerializer(column);
        }

        public static ColumnSerializer Create<T>(Column<T> column, Func<T, string> serializer, Func<string, T> deserializer)
        {
            Check.NotNull(column, nameof(column));
            Check.NotNull(serializer, nameof(serializer));
            Check.NotNull(deserializer, nameof(deserializer));
            return new TypedColumnSerializer<T>(column, serializer, deserializer);
        }

        private sealed class DefaultColumnSerializer : ColumnSerializer
        {
            public DefaultColumnSerializer(Column column)
            {
                Debug.Assert(column != null);
                _column = column;
            }

            private readonly Column _column;
            public override Column Column
            {
                get { return _column; }
            }

            public override string Serialize(RowPresenter row)
            {
                return row.GetObject(_column).ToString();
            }

            public override void Deserialize(string s, ColumnValueBag columnValueBag)
            {
                var converter = TypeDescriptor.GetConverter(_column.DataType);
                var value = converter.ConvertFromString(s);
                columnValueBag[_column] = value;
            }
        }

        private sealed class TypedColumnSerializer<T> : ColumnSerializer
        {
            public TypedColumnSerializer(Column<T> column, Func<T, string> serializer, Func<string, T> deserializer)
            {
                Debug.Assert(column != null);
                Debug.Assert(serializer != null);
                Debug.Assert(deserializer != null);
                _column = column;
                _serializer = serializer;
                _deserializer = deserializer;
            }

            private readonly Column<T> _column;
            public override Column Column
            {
                get { return _column; }
            }

            private readonly Func<T, string> _serializer;
            public override string Serialize(RowPresenter row)
            {
                return _serializer(row.GetValue(_column));
            }

            private readonly Func<string, T> _deserializer;
            public override void Deserialize(string s, ColumnValueBag columnValueBag)
            {
                var value = _deserializer(s);
                columnValueBag.SetValue(_column, value);
            }
        }

        public abstract Column Column { get; }

        public abstract void Deserialize(string value, ColumnValueBag columnValueBag);

        public abstract string Serialize(RowPresenter row);
    }
}
