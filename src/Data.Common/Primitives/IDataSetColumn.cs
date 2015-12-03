using System.Text;

namespace DevZest.Data.Primitives
{
    internal interface IDataSetColumn
    {
        void Serialize(int rowOrdinal, StringBuilder stringBuilder);

        DataSet NewValue(int rowOrdinal);

        void Deserialize(int rowOrdinal, DataSet value);
    }
}
