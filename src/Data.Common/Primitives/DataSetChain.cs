using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public struct DataSetChain
    {
        internal DataSetChain(DataSet dataSet, List<Model> modelChain, int modelChainIndex)
        {
            Current = dataSet;
            ModelChain = modelChain;
            ModelChainIndex = modelChainIndex;
        }

        private readonly DataSet Current;

        private readonly List<Model> ModelChain;

        private readonly int ModelChainIndex;

        public int RowCount
        {
            get { return Current == null ? 0 : Current.Count; }
        }

        public DataRow this[int index]
        {
            get
            {
                if (index < 0 || index >= RowCount)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return Current[index];
            }
        }

        public bool HasNext
        {
            get { return ModelChainIndex > 0; }
        }

        public DataSetChain Next(DataRow dataRow)
        {
            if (dataRow == null || !HasNext || dataRow.Model != Current.Model)
                return new DataSetChain();

            return new DataSetChain(dataRow[ModelChain[ModelChainIndex - 1]], ModelChain, ModelChainIndex - 1);
        }
    }
}
