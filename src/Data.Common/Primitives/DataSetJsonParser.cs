using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    internal sealed class DataSetJsonParser : JsonParser
    {
        internal DataSetJsonParser(string json)
            : base(json)
        {
        }

        internal DataSet Parse(Func<DataSet> dataSetCreator)
        {
            return Parse(dataSetCreator, true);
        }

        private DataSet Parse(Func<DataSet> dataSetCreator, bool isTopLevel)
        {
            if (PeekToken().Kind == TokenKind.Null)
            {
                ConsumeToken();
                if (isTopLevel)
                    ExpectToken(TokenKind.Eof);
                return null;
            }

            var result = dataSetCreator();
            Parse(result, isTopLevel);
            return result;
        }

        internal void Parse(DataSet dataSet, bool isTopLevel)
        {
            ExpectToken(TokenKind.SquaredOpen);

            if (PeekToken().Kind == TokenKind.CurlyOpen)
            {
                var model = dataSet.Model;
                model.EnterDataSetInitialization();

                Parse(dataSet.AddRow());

                while (PeekToken().Kind == TokenKind.Comma)
                {
                    ConsumeToken();
                    Parse(dataSet.AddRow());
                }

                model.ExitDataSetInitialization();
            }

            ExpectToken(TokenKind.SquaredClose);
            if (isTopLevel)
                ExpectToken(TokenKind.Eof);
        }

        private void Parse(DataRow dataRow)
        {
            ExpectToken(TokenKind.CurlyOpen);

            var token = PeekToken();
            if (token.Kind == TokenKind.String)
            {
                ConsumeToken();
                Parse(dataRow, token.Text);

                while (PeekToken().Kind == TokenKind.Comma)
                {
                    ConsumeToken();
                    token = ExpectToken(TokenKind.String);
                    Parse(dataRow, token.Text);
                }
            }

            ExpectToken(TokenKind.CurlyClose);
        }

        private void Parse(DataRow dataRow, string memberName)
        {
            ExpectToken(TokenKind.Colon);

            var model = dataRow.Model;
            var member = model[memberName];
            if (member == null)
                throw new FormatException(Strings.JsonParser_InvalidModelMember(memberName, model.GetType().FullName));
            var column = member as Column;
            if (column != null)
                Parse(column, dataRow.Ordinal);
            else
                Parse(dataRow[(Model)member], false);
        }

        private void Parse(Column column, int ordinal)
        {
            Debug.Assert(column != null);

            var dataSetColumn = column as IDataSetColumn;
            if (dataSetColumn != null)
            {
                var dataSet = Parse(() => dataSetColumn.NewValue(ordinal), false);
                if (column.ShouldSerialize)
                    dataSetColumn.Deserialize(ordinal, dataSet);
                return;
            }

            var token = ExpectToken(TokenKind.ColumnValues);
            if (column.ShouldSerialize)
                column.Deserialize(ordinal, token.JsonValue);
        }
    }
}
