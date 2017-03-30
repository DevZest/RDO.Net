using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public interface IDataRowProxy
    {
        DataRow Translate(DataRow dataRow);
        IEnumerable<DataRow> GetDataRows(Model parentModel);
        DataRow GetDataRow(Model parentModel, DataRow parentDataRow, int index);
    }
}
