using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents DataSet bottom-up chain in aggregate expression.
    /// </summary>
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

        /// <summary>
        /// Gets the row count of current DataSet.
        /// </summary>
        public int RowCount
        {
            get { return Current == null ? 0 : Current.Count; }
        }

        /// <summary>
        /// Gets the DataRow at specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        /// <returns>The DataRow result.</returns>
        public DataRow this[int index]
        {
            get
            {
                if (index < 0 || index >= RowCount)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return Current[index];
            }
        }

        /// <summary>
        /// Gets a value indicates whether there is next item in the chain.
        /// </summary>
        public bool HasNext
        {
            get { return ModelChainIndex > 0; }
        }

        /// <summary>
        /// Gets next DataSet chain for specified DataRow.
        /// </summary>
        /// <param name="dataRow">The specified DataRow.</param>
        /// <returns>Next DataSet chain for specified DataRow.</returns>
        public DataSetChain Next(DataRow dataRow)
        {
            if (dataRow == null || !HasNext || dataRow.Model != Current.Model)
                return new DataSetChain();

            return new DataSetChain(dataRow[ModelChain[ModelChainIndex - 1]], ModelChain, ModelChainIndex - 1);
        }
    }
}
