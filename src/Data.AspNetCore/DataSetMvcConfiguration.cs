using DevZest.Data.AspNetCore.ClientValidation;
using DevZest.Data.AspNetCore.Primitives;
using System.Collections.Generic;

namespace DevZest.Data.AspNetCore
{
    public class DataSetMvcConfiguration
    {
        public DataSetMvcConfiguration()
        {
            DataSetClientValidators = new List<IDataSetClientValidator>();

            DataSetClientValidators.Add(new MaxLengthClientValidator());
            DataSetClientValidators.Add(new RequiredClientValidator());
        }

        public IList<IDataSetClientValidator> DataSetClientValidators { get; }
    }
}
