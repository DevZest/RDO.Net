using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents selection of rows that can be copied to system clipboard.
    /// </summary>
    public struct SerializableSelection
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SerializableSelection"/>.
        /// </summary>
        /// <param name="rows">The selection of rows.</param>
        /// <param name="columnSerializers">The column serializers.</param>
        public SerializableSelection(IReadOnlyList<RowPresenter> rows, IReadOnlyList<ColumnSerializer> columnSerializers)
        {
            Rows = rows.VerifyNotNull(nameof(rows)).VerifyNoNullItem(nameof(rows));
            ColumnSerializers = columnSerializers.VerifyNotNull(nameof(columnSerializers)).VerifyNoNullItem(nameof(columnSerializers));
        }

        /// <summary>
        /// Gets the selection of rows.
        /// </summary>
        public readonly IReadOnlyList<RowPresenter> Rows;

        /// <summary>
        /// Gets the column serializers.
        /// </summary>
        public readonly IReadOnlyList<ColumnSerializer> ColumnSerializers;

        /// <summary>
        /// Gets a value indicates whether this serializable selection is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return IsEmptyList(Rows) || IsEmptyList(ColumnSerializers); }
        }

        private static bool IsEmptyList<T>(IReadOnlyList<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// Gets a value indicates whether the selection of rows can be copied to system clipboard.
        /// </summary>
        public bool CanCopyToClipboard
        {
            get { return !IsEmpty; }
        }

        /// <summary>
        /// Copyies data to clipboard.
        /// </summary>
        /// <param name="includeColumnNames">Indicates whether column names should be included.</param>
        /// <param name="copy">Indicates whether the data object should be left on the clipboard when the application exits.</param>
        public void CopyToClipboard(bool includeColumnNames, bool copy)
        {
            if (!CanCopyToClipboard)
                return;

            var tabDelimitedText = Serialize(includeColumnNames, TabularText.TabDelimiter);
            var commaDelimitedText = Serialize(includeColumnNames, TabularText.CommaDelimiter);
            var dataObject = new DataObject();
            dataObject.SetText(tabDelimitedText, TextDataFormat.UnicodeText);
            dataObject.SetText(commaDelimitedText, TextDataFormat.CommaSeparatedValue);
            Clipboard.SetDataObject(dataObject, copy);
        }

        private string Serialize(bool includeColumnNames, char delimiter)
        {
            if (IsEmpty)
                return null;

            var result = new StringBuilder();
            if (includeColumnNames)
                SerializeColumnNames(result, delimiter);

            for (int i = 0; i < Rows.Count; i++)
            {
                var row = Rows[i];
                for (int j = 0; j < ColumnSerializers.Count; j++)
                {
                    var columnSerializer = ColumnSerializers[j];
                    var s = columnSerializer.Serialize(row);
                    TabularText.Format(s, result, delimiter);
                    var isLast = j == ColumnSerializers.Count - 1;
                    if (!isLast)
                        result.Append(delimiter);
                }
                result.AppendLine();
            }
            return result.ToString();
        }

        private void SerializeColumnNames(StringBuilder output, char delimiter)
        {
            for (int i = 0; i < ColumnSerializers.Count; i++)
            {
                var column = ColumnSerializers[i].Column;
                TabularText.Format(column.DisplayName, output, delimiter);
                var isLast = i == ColumnSerializers.Count - 1;
                if (!isLast)
                    output.Append(delimiter);
            }
            output.AppendLine();
        }
    }
}
