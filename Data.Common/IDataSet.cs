using System.Collections.Generic;
using System.Text;

namespace DevZest.Data
{
    internal interface IDataSet : IList<DataRow>
    {
        Model Model { get; }

        void BuildJsonString(StringBuilder stringBuilder);

        IDataSet CreateSubDataSet(DataRow parentRow);

        DataRow AddRow();
    }
}
