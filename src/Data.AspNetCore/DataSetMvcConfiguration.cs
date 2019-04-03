using DevZest.Data.AspNetCore.ClientValidation;
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
