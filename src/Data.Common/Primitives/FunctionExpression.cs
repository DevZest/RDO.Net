using DevZest.Data.Utilities;
using System;
using System.Collections.ObjectModel;

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

        protected sealed override IColumns GetBaseColumns()
        {
            var result = Columns.Empty;
            if (Parameters == null)
                return result;

            for (int i = 0; i < Parameters.Count; i++)
                result = result.Union(Parameters[i].BaseColumns);

            return result.Seal();
        }

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

        protected sealed internal override ColumnExpression PerformTranslateTo(Model model)
        {
            var parameters = Parameters.TranslateToColumns(model);
            if (parameters == Parameters)
                return this;
            else
                return (ColumnExpression)Activator.CreateInstance(GetType(), parameters);
        }
    }
}
