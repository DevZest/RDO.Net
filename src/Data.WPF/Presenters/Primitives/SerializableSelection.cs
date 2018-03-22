using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public struct SerializableSelection
    {
        private const char CommaDelimiter = ',';
        private const char TabDelimiter = '\t';

        public SerializableSelection(IReadOnlyList<RowPresenter> rows, IReadOnlyList<ColumnSerializer> columnSerializers)
        {
            VerifyNotNull(rows, nameof(rows));
            VerifyNotNull(columnSerializers, nameof(columnSerializers));
            Rows = rows;
            ColumnSerializers = columnSerializers;
        }

        private static void VerifyNotNull<T>(IReadOnlyList<T> list, string listParamName)
            where T : class
        {
            Check.NotNull(list, listParamName);
            for (int i = 0; i < list.Count; i++)
                VerifyNotNull(list, i, listParamName);
        }

        private static T VerifyNotNull<T>(IReadOnlyList<T> list, int index, string listParamName)
            where T : class
        {
            Debug.Assert(list != null);
            var result = list[index];
            if (result == null)
                throw new ArgumentNullException(string.Format("{0}[1]", listParamName, index));
            return result;
        }

        public readonly IReadOnlyList<RowPresenter> Rows;
        public readonly IReadOnlyList<ColumnSerializer> ColumnSerializers;

        public bool IsEmpty
        {
            get { return IsEmptyList(Rows) || IsEmptyList(ColumnSerializers); }
        }

        private static bool IsEmptyList<T>(IReadOnlyList<T> list)
        {
            return list == null || list.Count == 0;
        }

        public bool CanCopyToClipboard
        {
            get { return !IsEmpty; }
        }

        public void CopyToClipboard(bool copy)
        {
            if (!CanCopyToClipboard)
                return;

            var tabDelimitedText = Serialize(TabDelimiter);
            var commaDelimitedText = Serialize(CommaDelimiter);
            var dataObject = new DataObject();
            dataObject.SetText(tabDelimitedText, TextDataFormat.UnicodeText);
            dataObject.SetText(commaDelimitedText, TextDataFormat.CommaSeparatedValue);
            Clipboard.SetDataObject(dataObject, copy);
        }

        private string Serialize(char delimiter)
        {
            if (IsEmpty)
                return null;

            var result = new StringBuilder();
            for (int i = 0; i < Rows.Count; i++)
            {
                var row = Rows[i];
                for (int j = 0; j < ColumnSerializers.Count; j++)
                {
                    var columnSerializer = ColumnSerializers[j];
                    var s = columnSerializer.Serialize(row);
                    Format(s, result, delimiter);
                    var isLast = j == ColumnSerializers.Count - 1;
                    if (!isLast)
                        result.Append(delimiter);
                }
                result.AppendLine();
            }
            return result.ToString();
        }

        private static void Format(string s, StringBuilder output, char delimiter)
        {
            if (s == null)
                return;

            var length = output.Length;
            var escapeApplied = FormatEscaped(s, output, delimiter);
            if (escapeApplied)
            {
                output.Insert(length, '"');
                output.Append('"');
            }
        }

        private static bool FormatEscaped(string s, StringBuilder output, char delimiter)
        {
            Debug.Assert(s != null);

            int length = s.Length;
            for (int i = 0; i < length; i++)
            {
                char c = s[i];
                output.Append(c);

                if (c == '"')
                {
                    output.Append('"');
                    return true;
                }

                if (c == delimiter)
                    return true;
            }

            return false;
        }
    }
}
