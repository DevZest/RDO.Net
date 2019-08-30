#if !DbInit
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public static class DataSetExtensions
    {
        public static DataSet<T> AddRows<T>(this DataSet<T> dataSet, int count)
            where T : class, IEntity, new()
        {
            for (int i = 0; i < count; i++)
                dataSet.AddRow();

            return dataSet;
        }
    }
}
#endif