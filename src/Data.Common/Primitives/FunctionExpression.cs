using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace DevZest.Data.Primitives
{
    public abstract class FunctionExpression<T> : ColumnExpression<T>
    {
        protected abstract class AbstractConverter<TExpression> : ExpressionConverter
            where TExpression : ColumnExpression<T>
        {
            private const string PARAMS = "Params";

            internal sealed override void WriteJson(StringBuilder stringBuilder, ColumnExpression expression)
            {
                var functionExpression = (FunctionExpression<T>)expression;
                stringBuilder.WriteNameColumnsPair<Column>(PARAMS, functionExpression.Parameters);
            }

            internal sealed override ColumnExpression ParseJson(Model model, ColumnJsonParser parser)
            {
                var parameters = parser.ParseNameColumnsPair<Column>(PARAMS, model);
                return MakeExpression(model, parameters);
            }

            protected abstract TExpression MakeExpression(Model model, IReadOnlyList<Column> parameters);
        }

        protected abstract class ConverterBase<TExpression> : AbstractConverter<TExpression>
            where TExpression : ColumnExpression<T>
        {
            protected sealed override TExpression MakeExpression(Model model, IReadOnlyList<Column> parameters)
            {
                return MakeExpression();
            }

            protected abstract TExpression MakeExpression();
        }

        protected abstract class ConverterBase<TParam1, TExpression> : AbstractConverter<TExpression>
            where TParam1 : Column
            where TExpression : ColumnExpression<T>
        {
            protected sealed override TExpression MakeExpression(Model model, IReadOnlyList<Column> parameters)
            {
                return MakeExpression((TParam1)parameters[0]);
            }

            protected abstract TExpression MakeExpression(TParam1 param);
        }

        protected abstract class ConverterBase<TParam1, TParam2, TExpression> : AbstractConverter<TExpression>
            where TParam1 : Column
            where TParam2 : Column
            where TExpression : ColumnExpression<T>
        {
            protected sealed override TExpression MakeExpression(Model model, IReadOnlyList<Column> parameters)
            {
                return MakeExpression((TParam1)parameters[0], (TParam2)parameters[1]);
            }

            protected abstract TExpression MakeExpression(TParam1 param1, TParam2 param2);
        }

        protected abstract class ConverterBase<TParam1, TParam2, TParam3, TExpression> : AbstractConverter<TExpression>
            where TParam1 : Column
            where TParam2 : Column
            where TParam3 : Column
            where TExpression : ColumnExpression<T>
        {
            protected sealed override TExpression MakeExpression(Model model, IReadOnlyList<Column> parameters)
            {
                return MakeExpression((TParam1)parameters[0], (TParam2)parameters[1], (TParam3)parameters[2]);
            }

            protected abstract TExpression MakeExpression(TParam1 param1, TParam2 param2, TParam3 param3);
        }

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
    }
}
