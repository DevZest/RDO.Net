using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Guid"/> column.
    /// </summary>
    public sealed class _Guid : Column<Guid?>, IColumn<DbReader, Guid?>
    {
        /// <inheritdoc/>
        protected sealed override Column<Guid?> CreateParam(Guid? value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal sealed override Column<Guid?> CreateConst(Guid? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(Guid? value)
        {
            return JsonValue.Guid(value);
        }

        /// <inheritdoc/>
        protected internal override Guid? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Guid?(new Guid(value.Text));
        }

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
        public Guid? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private Guid? GetValue(DbReader reader)
        {
            return reader.GetGuid(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(Guid? value)
        {
            return !value.HasValue;
        }

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
        public static _Guid Param(Guid? x, _Guid sourceColumn = null)
        {
            return new ParamExpression<Guid?>(x, sourceColumn).MakeColumn<_Guid>();
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _Guid Const(Guid? x)
        {
            return new ConstantExpression<Guid?>(x).MakeColumn<_Guid>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _Guid(Guid? x)
        {
            return Param(x);
        }

        private sealed class DbStringCast : CastExpression<String, Guid?>
        {
            public DbStringCast(_String x)
                : base(x)
            {
            }

            protected override Guid? Cast(String value)
            {
                if (value == null)
                    return null;
                return Guid.Parse(value);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Guid" />.</summary>
        /// <returns>A <see cref="_Guid" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Guid(_String x)
        {
            Check.NotNull(x, nameof(x));
            return new DbStringCast(x).MakeColumn<_Guid>();
        }

        private sealed class LessThanExpression : BinaryExpression<Guid?, bool?>
        {
            public LessThanExpression(_Guid x, _Guid y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(Guid? x, Guid? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) < 0;
            }
        }

        /// <summary>Compares the two <see cref="_Guid" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_Guid" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        /// <param name="y">A <see cref="_Guid" /> object. </param>
        public static _Boolean operator <(_Guid x, _Guid y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<Guid?, bool?>
        {
            public LessThanOrEqualExpression(_Guid x, _Guid y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(Guid? x, Guid? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) <= 0;
            }
        }

        /// <summary>Compares the two <see cref="_Guid" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Guid" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        /// <param name="y">A <see cref="_Guid" /> object. </param>
        public static _Boolean operator <=(_Guid x, _Guid y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<Guid?, bool?>
        {
            public GreaterThanExpression(_Guid x, _Guid y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(Guid? x, Guid? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) > 0;
            }
        }

        /// <summary>Compares the two <see cref="_Guid" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Guid" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        /// <param name="y">A <see cref="_Guid" /> object. </param>
        public static _Boolean operator >(_Guid x, _Guid y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Guid?, bool?>
        {
            public GreaterThanOrEqualExpression(_Guid x, _Guid y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(Guid? x, Guid? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) >= 0;
            }
        }

        /// <summary>Compares the two <see cref="_Guid" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Guid" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        /// <param name="y">A <see cref="_Guid" /> object. </param>
        public static _Boolean operator >=(_Guid x, _Guid y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<Guid?, bool?>
        {
            public EqualExpression(_Guid x, _Guid y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(Guid? x, Guid? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault() == y.GetValueOrDefault();
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Guid" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        /// <param name="y">A <see cref="_Guid" /> object. </param>
        public static _Boolean operator ==(_Guid x, _Guid y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<Guid?, bool?>
        {
            public NotEqualExpression(_Guid x, _Guid y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(Guid? x, Guid? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault() != y.GetValueOrDefault();
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Guid" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        /// <param name="y">A <see cref="_Guid" /> object. </param>
        public static _Boolean operator !=(_Guid x, _Guid y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        /// <exclude />
        public override bool Equals(object obj)
        {
            // override to eliminate compile warning
            return base.Equals(obj);
        }

        /// <exclude />
        public override int GetHashCode()
        {
            // override to eliminate compile warning
            return base.GetHashCode();
        }
    }
}
