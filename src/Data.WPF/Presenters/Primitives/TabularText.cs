using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DevZest.Data.Presenters.Primitives
{
    public sealed class TabularText : Model
    {
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

            var result = DataSet<TabularText>.New();
            var _ = result._;

            var inQuote = false;
            DataRow currentRow = null;
            var currentField = 0;
            var sb = new StringBuilder();

            while (reader.Peek() != -1)
            {
                if (currentRow == null)
                {
                    currentRow = new DataRow();
                    result.Add(currentRow);
                }

                var readChar = (char)reader.Read();

                if (readChar == '\n' || (readChar == '\r' && (char)reader.Peek() == '\n'))
                {
                    // If it's a \r\n combo consume the \n part and throw it away.
                    if (readChar == '\r')
                        reader.Read();

                    if (inQuote)
                    {
                        if (readChar == '\r')
                            sb.Append('\r');
                        sb.Append('\n');
                    }
                    else
                    {
                        _.AddValue(currentRow, currentField++, sb);
                        currentRow = null;
                        currentField = 0;
                    }
                }
                else if (sb.Length == 0 && !inQuote)
                {
                    if (readChar == '"')
                        inQuote = true;
                    else if (readChar == delimiter)
                        _.AddValue(currentRow, currentField++, sb);
                    else
                        sb.Append(readChar);
                }
                else if (readChar == delimiter)
                {
                    if (inQuote)
                        sb.Append(delimiter);
                    else
                        _.AddValue(currentRow, currentField++, sb);
                }
                else if (readChar == '"')
                {
                    if (inQuote)
                    {
                        if ((char)reader.Peek() == '"') // escaped quote
                        {
                            reader.Read();
                            sb.Append('"');
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

            return result;
        }

        private readonly List<Column<string>> _textColumns = new List<Column<string>>();
        public IReadOnlyList<Column<string>> TextColumns
        {
            get { return _textColumns; }
        }

        private void AddValue(DataRow dataRow, int fieldIndex, StringBuilder sb)
        {
            Debug.Assert(dataRow.Index == DataSet.Count - 1);
            Debug.Assert(fieldIndex >= 0 && fieldIndex <= TextColumns.Count);

            if (fieldIndex == TextColumns.Count)
                _textColumns.Add(CreateLocalColumn<string>());

            if (sb.Length > 0)
            {
                var value = sb.ToString();
                TextColumns[fieldIndex][dataRow] = value;
                sb.Clear();
            }
        }
    }
}
