using DevZest.Data.AspNetCore.Primitives;
using System.Collections.Generic;

namespace DevZest.Data.AspNetCore
{
    public class DataSetMvcConfiguration
    {
        public DataSetMvcConfiguration()
        {
            DataSetClientValidators = new List<IDataSetClientValidator>();
        }

        public IList<IDataSetClientValidator> DataSetClientValidators { get; }
    }
}
