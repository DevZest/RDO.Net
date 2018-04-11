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
            Check.NotNull(s, nameof(s));
            using (var textReader = new StringReader(s))
            {
                return Parse(textReader, delimiter);
            }
        }

        public static DataSet<TabularText> Parse(TextReader reader, char delimiter)
        {
            Check.NotNull(reader, nameof(reader));
            if (delimiter == QuotationMark)
                throw new ArgumentException(DiagnosticMessages.TabularText_DelimiterCannotBeQuote, nameof(delimiter));

            var result = DataSet<TabularText>.New();

            var sb = new StringBuilder();
            var hasNext = reader.Peek() != -1;
            while (hasNext)
                result.AddRow((_, dataRow) => hasNext = Parse(reader, delimiter, _, dataRow, sb));

            return result;
        }

        private static bool Parse(TextReader reader, char delimiter, TabularText _, DataRow dataRow, StringBuilder sb)
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
                            _.AddValue(dataRow, fieldIndex++, sb, ref inQuote);
                        return reader.Peek() != -1;
                    }
                }
                else if (sb.Length == 0 && inQuote != true)
                {
                    if (readChar == QuotationMark)
                        inQuote = true;
                    else if (readChar == delimiter)
                        _.AddValue(dataRow, fieldIndex++, sb, ref inQuote);
                    else
                        sb.Append(readChar);
                }
                else if (readChar == delimiter)
                {
                    if (inQuote == true)
                        sb.Append(delimiter);
                    else
                        _.AddValue(dataRow, fieldIndex++, sb, ref inQuote);
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

            _.AddValue(dataRow, fieldIndex++, sb, ref inQuote);
            return false;
        }

        private readonly List<Column<string>> _textColumns = new List<Column<string>>();
        public IReadOnlyList<Column<string>> TextColumns
        {
            get { return _textColumns; }
        }

        private void AddValue(DataRow dataRow, int fieldIndex, StringBuilder sb, ref bool? inQuote)
        {
            Debug.Assert(dataRow.Index == DataSet.Count - 1);
            Debug.Assert(fieldIndex >= 0 && fieldIndex <= TextColumns.Count);

            if (fieldIndex == TextColumns.Count)
                _textColumns.Add(CreateLocalColumn<string>());

            if (sb.Length > 0)
            {
                var value = sb.ToString();
                if (inQuote == true)
                    value = QuotationMark + value;
                TextColumns[fieldIndex][dataRow] = value;
                sb.Clear();
            }
            else if (inQuote == true)
                TextColumns[fieldIndex][dataRow] = "\"";
            else if (inQuote == false)
                TextColumns[fieldIndex][dataRow] = string.Empty;

            inQuote = null;
        }
    }
}
