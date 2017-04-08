using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public interface IDataRowProxy
    {
        DataRow Translate(DataRow dataRow);
        IEnumerable<DataRow> GetDataRows(Model model);
        DataRow GetDataRow(Model model, DataRow parentDataRow, int index);
    }
}
