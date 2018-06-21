using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public sealed class TabularText : Model
    {
        public const char QuotationMark = '"';
        public const char CommaDelimiter = ',';
        public const char TabDelimiter = '\t';

        internal static void Format(string s, StringBuilder output, char delimiter)
        {
            if (s == null)
                return;

            if (s.Length == 0)
            {
                output.Append(QuotationMark).Append(QuotationMark);
                return;
            }

            var length = output.Length;
            var inQuote = FormatEscaped(s, output, delimiter);
            if (inQuote)
            {
                output.Insert(length, QuotationMark);
                output.Append(QuotationMark);
            }
        }

        private static bool FormatEscaped(string s, StringBuilder output, char delimiter)
        {
            Debug.Assert(!string.IsNullOrEmpty(s));

            int length = s.Length;
            bool escaped = false;
            for (int i = 0; i < length; i++)
            {
                char c = s[i];
                output.Append(c);

                if (c == QuotationMark)
                {
                    output.Append(QuotationMark);
                    escaped = true;
                }
                else if (c == delimiter || c == '\r' || c == '\n')
                    escaped = true;
            }
            return escaped;
        }

        public static bool CanPasteFromClipboard
        {
            get { return Clipboard.ContainsData(DataFormats.CommaSeparatedValue) || Clipboard.ContainsText(); }
        }

        public static DataSet<TabularText> PasteFromClipboard()
        {
            var csv = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
            if (csv != null)
                return Parse(csv, ',');

            var text = Clipboard.GetText();
            if (!string.IsNullOrEmpty(text))
                return Parse(text, '\t');

            return null;
        }

        public static DataSet<TabularText> Parse(string s, char delimiter)
        {
            s.VerifyNotNull(nameof(s));
            using (var textReader = new StringReader(s))
            {
                return Parse(textReader, delimiter);
            }
        }

        public static DataSet<TabularText> Parse(TextReader reader, char delimiter)
        {
            reader.VerifyNotNull(nameof(reader));
            if (delimiter == QuotationMark)
                throw new ArgumentException(DiagnosticMessages.TabularText_DelimiterCannotBeQuote, nameof(delimiter));

            var rows = new List<List<string>>();
            int maxColumnsCount = 0;
            var sb = new StringBuilder();
            var hasNext = reader.Peek() != -1;
            while (hasNext)
            {
                var columns = new List<string>();
                rows.Add(columns);
                hasNext = ParseRow(reader, delimiter, sb, value => columns.Add(value));
                if (columns.Count > maxColumnsCount)
                    maxColumnsCount = columns.Count;
            }
            return ToDataSet(rows, maxColumnsCount);
        }

        private static DataSet<TabularText> ToDataSet(List<List<string>> rows, int maxColumnsCount)
        {
            var result = DataSet<TabularText>.New();
            result._.InitializeTextColumns(maxColumnsCount);

            for (int i = 0; i < rows.Count; i++)
            {
                var columns = rows[i];
                result.AddRow((_, dataRow) =>
                {
                    for (int j = 0; j < columns.Count; j++)
                        _.TextColumns[j][dataRow] = columns[j];
                });
            }
            return result;
        }

        private readonly List<LocalColumn<string>> _textColumns = new List<LocalColumn<string>>();
        public IReadOnlyList<Column<string>> TextColumns
        {
            get { return _textColumns; }
        }

        private void InitializeTextColumns(int count)
        {
            for (int i = 0; i < count; i++)
                _textColumns.Add(CreateLocalColumn<string>());
        }

        private static bool ParseRow(TextReader reader, char delimiter, StringBuilder sb, Action<string> onColumnParsed)
        {
            Debug.Assert(reader.Peek() != -1);

            bool? inQuote = null;   // three states to distinguish between null and string.Empty
            var fieldIndex = 0;

            do
            {
                var readChar = (char)reader.Read();

                if (readChar == '\n' || (readChar == '\r' && (char)reader.Peek() == '\n'))
                {
                    // If it's a \r\n combo consume the \n part and throw it away.
                    if (readChar == '\r')
                        reader.Read();

                    if (inQuote == true)
                    {
                        if (readChar == '\r')
                            sb.Append('\r');
                        sb.Append('\n');
                    }
                    else
                    {
                        if (sb.Length > 0)
                            ReportColumnParsed(sb, ref fieldIndex, ref inQuote, onColumnParsed);
                        return reader.Peek() != -1;
                    }
                }
                else if (sb.Length == 0 && inQuote != true)
                {
                    if (readChar == QuotationMark)
                        inQuote = true;
                    else if (readChar == delimiter)
                        ReportColumnParsed(sb, ref fieldIndex, ref inQuote, onColumnParsed);
                    else
                        sb.Append(readChar);
                }
                else if (readChar == delimiter)
                {
                    if (inQuote == true)
                        sb.Append(delimiter);
                    else
                        ReportColumnParsed(sb, ref fieldIndex, ref inQuote, onColumnParsed);
                }
                else if (readChar == QuotationMark)
                {
                    if (inQuote == true)
                    {
                        if ((char)reader.Peek() == QuotationMark) // escaped quote
                        {
                            reader.Read();
                            sb.Append(QuotationMark);
                        }
                        else
                            inQuote = false;
                    }
                    else
                        sb.Append(readChar);
                }
                else
                    sb.Append(readChar);
            }
            while (reader.Peek() != -1);

            ReportColumnParsed(sb, ref fieldIndex, ref inQuote, onColumnParsed);
            return false;
        }

        private static void ReportColumnParsed(StringBuilder sb, ref int fieldIndex, ref bool? inQuote, Action<string> onColumnParsed)
        {
            if (sb.Length > 0)
            {
                var value = sb.ToString();
                if (inQuote == true)
                    value = QuotationMark + value;
                onColumnParsed(value);
                sb.Clear();
            }
            else if (inQuote == true)
                onColumnParsed("\"");
            else if (inQuote == false)
                onColumnParsed(string.Empty);
            else
                onColumnParsed(null);

            fieldIndex++;
            inQuote = null;
        }
    }
}
