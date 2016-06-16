using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public abstract class FunctionExpression<T> : ColumnExpression<T>
    {
        protected FunctionExpression(params Column[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                Parameters = null;
            Parameters = new ReadOnlyCollection<Column>(parameters);
        }

        public ReadOnlyCollection<Column> Parameters;

        protected abstract FunctionKey FunctionKey { get; }

        private DbExpression _dbExpression;
        public sealed override DbExpression GetDbExpression()
        {
            return _dbExpression ?? (_dbExpression = CreateDbExpression());
        }

        private DbExpression CreateDbExpression()
        {
            if (Parameters == null)
                return new DbFunctionExpression(FunctionKey);

            var paramList = new DbExpression[Parameters.Count];
            for (int i = 0; i < Parameters.Count; i++)
            {
                var parameter = Parameters[i];
                paramList[i] = parameter == null ? null : parameter.DbExpression;
            }

            return new DbFunctionExpression(FunctionKey, paramList);
        }

        internal sealed override Column<T> GetCounterpart(Model model)
        {
            if (Parameters == null)
                return this.Owner;

            var functionExpr = (FunctionExpression<T>)this.MemberwiseClone();
            var parameters = new Column[Parameters.Count];
            for (int i = 0; i < Parameters.Count; i++)
                parameters[i] = Parameters[i].GetCounterpart(model);
            Parameters = new ReadOnlyCollection<Column>(parameters);

            return GetCounterpart(functionExpr);
        }
    }
}
