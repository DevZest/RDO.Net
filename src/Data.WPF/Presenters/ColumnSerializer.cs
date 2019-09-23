using System;
using System.ComponentModel;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Base class to serialize/deserialize value of data column.
    /// </summary>
    public abstract class ColumnSerializer
    {
        /// <summary>
        /// Creates default column serializer for specified column.
        /// </summary>
        /// <param name="column">The specified column.</param>
        /// <returns>The default column serializer for specified column.</returns>
        public static ColumnSerializer Create(Column column)
        {
            column.VerifyNotNull(nameof(column));
            return new DefaultColumnSerializer(column);
        }

        /// <summary>
        /// Creates customm column serializer for specified column.
        /// </summary>
        /// <typeparam name="T">The data type of the column.</typeparam>
        /// <param name="column">The specified column.</param>
        /// <param name="serializer">The delegate to serialize the column.</param>
        /// <param name="deserializer">The delegate to deserialize the column.</param>
        /// <returns>The customm column serializer for specified column.</returns>
        public static ColumnSerializer Create<T>(Column<T> column, Func<T, string> serializer, Func<string, T> deserializer)
        {
            column.VerifyNotNull(nameof(column));
            serializer.VerifyNotNull(nameof(serializer));
            deserializer.VerifyNotNull(nameof(deserializer));
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
                return row[_column]?.ToString();
            }

            public override void Deserialize(string s, ColumnValueBag columnValueBag)
            {
                if (s == null)
                    columnValueBag[_column] = null;
                else
                {
                    var converter = TypeDescriptor.GetConverter(_column.DataType);
                    var value = converter.ConvertFromString(s);
                    columnValueBag[_column] = value;
                }
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

        /// <summary>
        /// Gets the column.
        /// </summary>
        public abstract Column Column { get; }

        /// <summary>
        /// Deserializes string into column.
        /// </summary>
        /// <param name="value">The value of the string.</param>
        /// <param name="columnValueBag">The <see cref="ColumnValueBag"/> that will contain the column and deserialized value.</param>
        public abstract void Deserialize(string value, ColumnValueBag columnValueBag);

        /// <summary>
        /// Serializes colummn into string.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public abstract string Serialize(RowPresenter row);
    }
}
