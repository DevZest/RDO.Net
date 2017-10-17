using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Reflection;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region Min

        private sealed class ComparableMinFunction<T> : AggregateFunctionExpression<T>
            where T : IComparable<T>
        {
            public ComparableMinFunction(Column<T> x)
                : base(x)
            {
                _column = x;
            }

            private Column<T> _column;

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Min; }
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

                if (result == null || value.CompareTo(result) < 0)
                    result = value;
            }

            protected override T EvalReturn()
            {
                return result;
            }
        }

        private sealed class NullableMinFunction<[UnderlyingValueType]T> : AggregateFunctionExpression<Nullable<T>>
            where T : struct, IComparable<T>
        {
            public NullableMinFunction(Column<Nullable<T>> x)
                : base(x)
            {
                _column = x;
            }

            private Column<Nullable<T>> _column;

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Min; }
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

                if (!result.HasValue || value.GetValueOrDefault().CompareTo(result.GetValueOrDefault()) < 0)
                    result = value;
            }

            protected override T? EvalReturn()
            {
                return result;
            }
        }

        private sealed class ComparableMinFunctionInvoker<T> : GenericInvoker<T>
        {
            public static readonly ComparableMinFunctionInvoker<T> Singleton = new ComparableMinFunctionInvoker<T>();

            private ComparableMinFunctionInvoker()
                : base(typeof(Functions).GetStaticMethodInfo(nameof(ComparableMin)), () => typeof(T).ResolveColumnDataType())
            {
            }
        }

        private sealed class NullableMinFunctionInvoker<T> : GenericInvoker<T>
        {
            public static readonly NullableMinFunctionInvoker<T> Singleton = new NullableMinFunctionInvoker<T>();

            private NullableMinFunctionInvoker()
                : base(typeof(Functions).GetStaticMethodInfo(nameof(NullableMin)), () => typeof(T).ResolveColumnDataType(true))
            {
            }
        }

        public static T Min<T>(this T x)
            where T : Column
        {
            bool? bypassNullable = BypassNullableToComparable<T>();
            if (!bypassNullable.HasValue)
                throw new NotSupportedException();

            if (bypassNullable.GetValueOrDefault())
                return NullableMinFunctionInvoker<T>.Singleton.Invoke(x);
            else
                return ComparableMinFunctionInvoker<T>.Singleton.Invoke(x);
        }

        private static bool? BypassNullableToComparable<T>()
            where T : Column
        {
            for (var type = typeof(T); type != null; type = type.GetTypeInfo().BaseType)
            {
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Column<>))
                {
                    var typeParam = type.GetGenericArguments()[0];
                    if (typeParam.IsComparable())
                        return false;

                    if (typeParam.GetTypeInfo().IsGenericType && typeParam.GetGenericTypeDefinition() == typeof(Nullable<>))
                        typeParam = typeParam.GetGenericArguments()[0];

                    return typeParam.IsComparable() ? new bool?(true) : null;
                }
            }

            return null;
        }

        private static T ComparableMin<T, TValue>(this T x)
            where T : Column<TValue>, new()
            where TValue : IComparable<TValue>
        {
            return new ComparableMinFunction<TValue>(x).MakeColumn<T>();
        }

        private static T NullableMin<T, TValue>(this T x)
            where T : Column<Nullable<TValue>>, new()
            where TValue : struct, IComparable<TValue>
        {
            return new NullableMinFunction<TValue>(x).MakeColumn<T>();
        }

        #endregion
    }
}
