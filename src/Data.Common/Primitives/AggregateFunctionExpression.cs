using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents a column expression of aggregate function.</summary>
    /// <typeparam name="T">The data type of the expression.</typeparam>
    public abstract class AggregateFunctionExpression<T> : FunctionExpression<T>
    {
        /// <summary>Initializes a new instance of <see cref="AggregateFunctionExpression{T}"/> class.</summary>
        /// <param name="param">The parameter of the aggregate function.</param>
        protected AggregateFunctionExpression(Column param)
            : base(new Column[] { Check.NotNull(param, nameof(param)) })
        {
        }

        /// <summary>Gets the parameter of the aggregate function.</summary>
        public Column Param
        {
            get { return Parameters[0]; }
        }

        /// <inheritdoc/>
        protected sealed override IModelSet GetParentModelSet()
        {
            return ModelSet.Empty;
        }

        /// <inheritdoc/>
        protected sealed override IModelSet GetAggregateModelSet()
        {
            return Param.ParentModelSet;
        }

        /// <summary>Initializes the expression evaluation.</summary>
        protected abstract void EvalInit();

        /// <summary>Accumulates the aggregate against specified parent <see cref="DataRow"/>.</summary>
        /// <param name="dataRow">The parent <see cref="DataRow"/>.</param>
        protected abstract void EvalAccumulate(DataRow dataRow);

        /// <summary>Returns the calculated aggregate result.</summary>
        /// <returns>The calculated aggregate result.</returns>
        protected abstract T EvalReturn();

        List<Model> ResolveModelChain(DataRow dataRow)
        {
            Model root = dataRow == null ? null : dataRow.Model;
            List<Model> result = new List<Model>();
            for (var model = Param.ParentModel; model != null; model = model.ParentModel)
            {
                if (model == root)
                    break;
                result.Add(model);
            }

            if (result.Count == 0 || result[result.Count - 1].ParentModel != root)
                return null;
            else
                return result;
        }

        /// <inheritdoc/>
        protected internal sealed override T this[DataRow dataRow]
        {
            get { return Eval(dataRow); }
        }

        /// <inheritdoc/>
        protected internal sealed override T Eval()
        {
            return Eval(null);
        }

        private T Eval(DataRow dataRow)
        {
            if (Param.ParentModel.DataSource.Kind != DataSourceKind.DataSet)
                throw new InvalidOperationException(Strings.ColumnAggregateFunction_EvalOnNonDataSet(Param, Param.ParentModel.DataSource.Kind));

            var modelChain = ResolveModelChain(dataRow);
            if (modelChain == null)
                throw new ArgumentException(Strings.ColumnAggregateFunction_NoModelChain(Param.ToString()), nameof(dataRow));

            EvalInit();

            int lastIndex = modelChain.Count - 1;
            var lastModel = modelChain[lastIndex];
            DataSet dataSet = dataRow == null ? lastModel.DataSet : dataRow[lastModel];
            EvalTraverse(new DataSetChain(dataSet, modelChain, lastIndex));
            return EvalReturn();
        }

        /// <summary>Traverse the <see cref="DataSetChain"/> during expression evaluation.</summary>
        /// <param name="dataSetChain">The <see cref="DataSetChain"/>.</param>
        protected virtual void EvalTraverse(DataSetChain dataSetChain)
        {
            if (dataSetChain.HasNext)
            {
                for (int i = 0; i < dataSetChain.RowCount; i++)
                    EvalTraverse(dataSetChain.Next(dataSetChain[i]));
            }
            else
            {
                for (int i = 0; i < dataSetChain.RowCount; i++)
                    EvalAccumulate(dataSetChain[i]);
            }
        }
    }
}
