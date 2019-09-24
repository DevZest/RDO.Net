using DevZest.Data.AspNetCore.ClientValidation;
using System.Collections.Generic;

namespace DevZest.Data.AspNetCore
{
    /// <summary>
    /// Represents the configuration for DataSet MVC.
    /// </summary>
    public class DataSetMvcConfiguration
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DataSetMvcConfiguration"/> class.
        /// </summary>
        public DataSetMvcConfiguration()
        {
            DataSetClientValidators = new List<IDataSetClientValidator>();
        }

        /// <summary>
        /// Gets the DataSet client validators.
        /// </summary>
        public IList<IDataSetClientValidator> DataSetClientValidators { get; }
    }
}
