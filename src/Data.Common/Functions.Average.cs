using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region Average

        private abstract class AverageBase<T, TResult> : AggregateFunctionExpression<TResult>
        {
            protected AverageBase(Column<T> x)
                : base(x)
            {
                _column = x;
            }

            protected sealed override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Average; }
            }

            private Column<T> _column;

            protected abstract void EvalAccumulate(T value);
            protected sealed override void EvalAccumulate(DataRow dataRow)
            {
                EvalAccumulate(_column[dataRow]);
            }
        }

        [ExpressionConverterNonGenerics(typeof(AverageInt32.Converter), Id = "Average(_Int32)")]
        private sealed class AverageInt32 : AverageBase<Int32?, Double?>
        {
            private sealed class Converter : ConverterBase<Column<Int32?>, AverageInt32>
            {
                protected override AverageInt32 MakeExpression(Column<int?> param)
                {
                    return new AverageInt32(param);
                }
            }

            public AverageInt32(Column<Int32?> x)
                : base(x)
            {
            }

            int count;
            int num;

            protected override void EvalInit()
            {
                count = 0;
                num = 0;
            }

            protected override void EvalAccumulate(int? value)
            {
                checked
                {
                    if (value.HasValue)
                    {
                        num += value.GetValueOrDefault();
                        count++;
                    }
                }
            }

            protected override double? EvalReturn()
            {
                return count == 0 ? null : new double?((double)num / (double)count);
            }
        }

        public static _Double Average(this Column<Int32?> x)
        {
            Check.NotNull(x, nameof(x));
            return new AverageInt32(x).MakeColumn<_Double>();
        }

        [ExpressionConverterNonGenerics(typeof(AverageInt64.Converter), Id = "Average(_Int64)")]
        private sealed class AverageInt64 : AverageBase<Int64?, Double?>
        {
            private sealed class Converter : ConverterBase<Column<Int64?>, AverageInt64>
            {
                protected override AverageInt64 MakeExpression(Column<long?> param)
                {
                    return new AverageInt64(param);
                }
            }

            public AverageInt64(Column<Int64?> x)
                : base(x)
            {
            }

            int count;
            Int64 num;

            protected override void EvalInit()
            {
                count = 0;
                num = 0;
            }

            protected override void EvalAccumulate(Int64? value)
            {
                checked
                {
                    if (value.HasValue)
                    {
                        num += value.GetValueOrDefault();
                        count++;
                    }
                }
            }

            protected override Double? EvalReturn()
            {
                return count == 0 ? null : new Double?((double)num / (double)count);
            }
        }

        public static _Double Average(this Column<Int64?> x)
        {
            Check.NotNull(x, nameof(x));
            return new AverageInt64(x).MakeColumn<_Double>();
        }

        [ExpressionConverterNonGenerics(typeof(AverageDecimal.Converter), Id = "Average(_Decimal)")]
        private sealed class AverageDecimal : AverageBase<Decimal?, Decimal?>
        {
            private sealed class Converter : ConverterBase<Column<Decimal?>, AverageDecimal>
            {
                protected override AverageDecimal MakeExpression(Column<decimal?> param)
                {
                    return new AverageDecimal(param);
                }
            }

            public AverageDecimal(Column<Decimal?> x)
                : base(x)
            {
            }

            int count;
            Decimal num;

            protected override void EvalInit()
            {
                count = 0;
                num = 0;
            }

            protected override void EvalAccumulate(Decimal? value)
            {
                checked
                {
                    if (value.HasValue)
                    {
                        num += value.GetValueOrDefault();
                        count++;
                    }
                }
            }

            protected override Decimal? EvalReturn()
            {
                return count == 0 ? null : new Decimal?(num / (Decimal)count);
            }
        }

        public static _Decimal Average(this Column<Decimal?> x)
        {
            Check.NotNull(x, nameof(x));
            return new AverageDecimal(x).MakeColumn<_Decimal>();
        }

        [ExpressionConverterNonGenerics(typeof(AverageDouble.Converter), Id = "Average(_Double)")]
        private sealed class AverageDouble : AverageBase<Double?, Double?>
        {
            private sealed class Converter : ConverterBase<Column<Double?>, AverageDouble>
            {
                protected override AverageDouble MakeExpression(Column<double?> param)
                {
                    return new AverageDouble(param);
                }
            }

            public AverageDouble(Column<Double?> x)
                : base(x)
            {
            }

            int count;
            Double num;

            protected override void EvalInit()
            {
                count = 0;
                num = 0;
            }

            protected override void EvalAccumulate(Double? value)
            {
                checked
                {
                    if (value.HasValue)
                    {
                        num += value.GetValueOrDefault();
                        count++;
                    }
                }
            }

            protected override Double? EvalReturn()
            {
                return count == 0 ? null : new Double?(num / (Double)count);
            }
        }

        public static _Double Average(this Column<Double?> x)
        {
            Check.NotNull(x, nameof(x));
            return new AverageDouble(x).MakeColumn<_Double>();
        }

        [ExpressionConverterNonGenerics(typeof(AverageSingle.Converter), Id = "Average(_Single)")]
        private sealed class AverageSingle : AverageBase<Single?, Single?>
        {
            private sealed class Converter : ConverterBase<Column<Single?>, AverageSingle>
            {
                protected override AverageSingle MakeExpression(Column<float?> param)
                {
                    return new AverageSingle(param);
                }
            }

            public AverageSingle(Column<Single?> x)
                : base(x)
            {
            }

            int count;
            Single num;

            protected override void EvalInit()
            {
                count = 0;
                num = 0;
            }

            protected override void EvalAccumulate(Single? value)
            {
                checked
                {
                    if (value.HasValue)
                    {
                        num += value.GetValueOrDefault();
                        count++;
                    }
                }
            }

            protected override Single? EvalReturn()
            {
                return count == 0 ? null : new Single?(num / (Single)count);
            }
        }

        public static _Single Average(this Column<Single?> x)
        {
            Check.NotNull(x, nameof(x));
            return new AverageSingle(x).MakeColumn<_Single>();
        }

        #endregion
    }
}
