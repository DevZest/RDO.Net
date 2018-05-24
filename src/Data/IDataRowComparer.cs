using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IDataRowComparer : IComparer<DataRow>
    {
        Type ModelType { get; }
    }
}
