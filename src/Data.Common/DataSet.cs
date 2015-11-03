using DevZest.Data.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DevZest.Data
{
    public abstract class DataSet : DataSource, IList<DataRow>
    {
        internal DataSet(Model model)
            : base(model)
        {
        }

        /// <inheritdoc/>
        public sealed override DataSourceKind Kind
        {
            get { return DataSourceKind.DataSet; }
        }

        internal abstract DataSet CreateSubDataSet(DataRow parentRow);

        /// Creates a new instance of <see cref="DataRow"/> and add to this data set.
        /// </summary>
        /// <returns>The new <see cref="DataRow"/> object.</returns>
        public DataRow AddRow()
        {
            var result = new DataRow();
            this.Add(result);
            return result;
        }

        internal readonly List<DataRow> _rows = new List<DataRow>();

        public abstract DataRow ParentRow { get; }

        /// <inheritdoc cref="ICollection{T}.Count"/>
        public int Count
        {
            get { return _rows.Count; }
        }

        /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
        public abstract bool IsReadOnly { get; }

        /// <summary>Gets or sets the <see cref="DataRow"/> at specified index.</summary>
        /// <param name="index">The zero-based index of the <see cref="DataRow"/> to get or set.</param>
        /// <returns>The <see cref="DataRow"/> at the specified index.</returns>
        public DataRow this[int index]
        {
            get { return _rows[index]; }
            set
            {
                if (this[index] == value)
                    return;

                RemoveAt(index);
                Insert(index, value);
            }
        }

        /// <inheritdoc cref="IList{T}.IndexOf(T)"/>
        public abstract int IndexOf(DataRow dataRow);

        /// <inheritdoc cref="ICollection{T}.Contains(T)"/>
        public bool Contains(DataRow dataRow)
        {
            return IndexOf(dataRow) != -1;
        }

        /// <inheritdoc cref="IList{T}.Insert(int, T)"/>
        public abstract void Insert(int index, DataRow dataRow);

        public virtual bool Remove(DataRow dataRow)
        {
            var index = IndexOf(dataRow);
            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        /// <inheritdoc cref="IList{T}.RemoveAt(int)"/>
        public abstract void RemoveAt(int index);

        public void Add(DataRow dataRow)
        {
            Insert(Count, dataRow);
        }

        public abstract void Clear();

        public void CopyTo(DataRow[] array, int arrayIndex)
        {
            _rows.CopyTo(array, arrayIndex);
        }

        public IEnumerator<DataRow> GetEnumerator()
        {
            foreach (var dataRow in _rows)
                yield return dataRow;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToJsonString(isPretty: true);
        }

        public string ToJsonString(bool isPretty)
        {
            var result = new StringBuilder();
            BuildJsonString(result);

            if (isPretty)
                return JsonFormatter.PrettyPrint(result.ToString());
            else
                return result.ToString();
        }

        public void BuildJsonString(StringBuilder stringBuilder)
        {
            stringBuilder.Append('[');
            int count = 0;
            foreach (var dataRow in this)
            {
                if (count > 0)
                    stringBuilder.Append(',');
                dataRow.BuildJsonString(stringBuilder);
                count++;
            }
            stringBuilder.Append(']');
        }

        public bool AllowsKeyUpdate(bool value)
        {
            return Model.AllowsKeyUpdate(value);
        }
    }
}
