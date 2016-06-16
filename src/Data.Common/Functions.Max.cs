using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Reflection;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region Max

        [ExpressionConverterGenerics(typeof(ComparableMaxFunction<>.Converter), TypeId = "Max.Comparable(Column)")]
        private sealed class ComparableMaxFunction<T> : AggregateFunctionExpression<T>
            where T : IComparable<T>
        {
            private sealed class Converter : ConverterBase<Column<T>, ComparableMaxFunction<T>>
            {
                protected override ComparableMaxFunction<T> MakeExpression(Column<T> param)
                {
                    return new ComparableMaxFunction<T>(param);
                }
            }

            public ComparableMaxFunction(Column<T> x)
                : base(x)
            {
                _column = x;
            }

            protected internal override Type[] ArgColumnTypes
            {
                get { return new Type[] { Owner.GetType() }; }
            }

            private Column<T> _column;

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Max; }
            }

            T result;
            protected override void EvalInit()
            {
                result = default(T);
            }

            protected override void EvalAccumulate(DataRow dataRow)
            {
                var value = _column[dataRow];
                if (value == null)
                    return;

                if (result == null || value.CompareTo(result) > 0)
                    result = value;
            }

            protected override T EvalReturn()
            {
                return result;
            }
        }

        [ExpressionConverterGenerics(typeof(NullableMaxFunction<>.Converter), TypeId = "Max.Nullable(Column)")]
        private sealed class NullableMaxFunction<[UnderlyingValueType]T> : AggregateFunctionExpression<Nullable<T>>
            where T : struct, IComparable<T>
        {
            private sealed class Converter : ConverterBase<Column<Nullable<T>>, NullableMaxFunction<T>>
            {
                protected override NullableMaxFunction<T> MakeExpression(Column<T?> param)
                {
                    return new NullableMaxFunction<T>(param);
                }
            }

            public NullableMaxFunction(Column<Nullable<T>> x)
                : base(x)
            {
                _column = x;
            }

            protected internal override Type[] ArgColumnTypes
            {
                get { return new Type[] { Owner.GetType() }; }
            }

            private Column<Nullable<T>> _column;

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Max; }
            }

            T? result;
            protected override void EvalInit()
            {
                result = null;
            }

            protected override void EvalAccumulate(DataRow dataRow)
            {
                var value = _column[dataRow];
                if (!value.HasValue)
                    return;

                if (!result.HasValue || value.GetValueOrDefault().CompareTo(result.GetValueOrDefault()) > 0)
                    result = value;
            }

            protected override T? EvalReturn()
            {
                return result;
            }
        }

        private sealed class ComparableMaxFunctionInvoker<T> : GenericInvoker<T>
        {
            public static readonly ComparableMaxFunctionInvoker<T> Singleton = new ComparableMaxFunctionInvoker<T>();

            private ComparableMaxFunctionInvoker()
                : base(typeof(Functions).GetStaticMethodInfo(nameof(ComparableMax)), () => typeof(T).ResolveColumnDataType())
            {
            }
        }

        private sealed class NullableMaxFunctionInvoker<T> : GenericInvoker<T>
        {
            public static readonly NullableMaxFunctionInvoker<T> Singleton = new NullableMaxFunctionInvoker<T>();

            private NullableMaxFunctionInvoker()
                : base(typeof(Functions).GetStaticMethodInfo(nameof(NullableMax)), () => typeof(T).ResolveColumnDataType(true))
            {
            }
        }

        public static T Max<T>(this T x)
            where T : Column
        {
            bool? bypassNullable = BypassNullableToComparable<T>();
            if (!bypassNullable.HasValue)
                throw new NotSupportedException();

            if (bypassNullable.GetValueOrDefault())
                return NullableMaxFunctionInvoker<T>.Singleton.Invoke(x);
            else
                return ComparableMaxFunctionInvoker<T>.Singleton.Invoke(x);
        }

        private static T ComparableMax<T, TValue>(this T x)
            where T : Column<TValue>, new()
            where TValue : IComparable<TValue>
        {
            return new ComparableMaxFunction<TValue>(x).MakeColumn<T>();
        }

        private static T NullableMax<T, TValue>(this T x)
            where T : Column<Nullable<TValue>>, new()
            where TValue : struct, IComparable<TValue>
        {
            return new NullableMaxFunction<TValue>(x).MakeColumn<T>();
        }

        #endregion
    }
}
