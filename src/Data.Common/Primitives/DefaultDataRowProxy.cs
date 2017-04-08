using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    internal sealed class DefaultDataRowProxy : IDataRowProxy
    {
        public static readonly DefaultDataRowProxy Singleton = new DefaultDataRowProxy();

        private DefaultDataRowProxy()
        {
        }

        public DataRow Translate(DataRow dataRow)
        {
            return dataRow;
        }

        public IEnumerable<DataRow> GetDataRows(Model model)
        {
            return model.DataSet;
        }

        public DataRow GetDataRow(Model model, DataRow parentDataRow, int index)
        {
            var dataSet = GetDataSet(model, parentDataRow);
            return dataSet[index];
        }

        private static DataSet GetDataSet(Model parentModel, DataRow parentDataRow)
        {
            return parentDataRow == null ? parentModel.DataSet : parentDataRow[parentModel];
        }
    }
}
