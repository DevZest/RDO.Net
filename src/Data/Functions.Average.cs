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

        private sealed class AverageInt32 : AverageBase<Int32?, Double?>
        {
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
            x.VerifyNotNull(nameof(x));
            return new AverageInt32(x).MakeColumn<_Double>();
        }

        private sealed class AverageInt64 : AverageBase<Int64?, Double?>
        {
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
            x.VerifyNotNull(nameof(x));
            return new AverageInt64(x).MakeColumn<_Double>();
        }

        private sealed class AverageDecimal : AverageBase<Decimal?, Decimal?>
        {
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
            x.VerifyNotNull(nameof(x));
            return new AverageDecimal(x).MakeColumn<_Decimal>();
        }

        private sealed class AverageDouble : AverageBase<Double?, Double?>
        {
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
            x.VerifyNotNull(nameof(x));
            return new AverageDouble(x).MakeColumn<_Double>();
        }

        private sealed class AverageSingle : AverageBase<Single?, Single?>
        {
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
            x.VerifyNotNull(nameof(x));
            return new AverageSingle(x).MakeColumn<_Single>();
        }

        #endregion
    }
}
