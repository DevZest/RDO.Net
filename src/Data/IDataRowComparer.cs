using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    /// <summary>
    /// Compares two DataRow objects by value.
    /// </summary>
    public interface IDataRowComparer : IComparer<DataRow>
    {
        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        Type ModelType { get; }
    }
}
