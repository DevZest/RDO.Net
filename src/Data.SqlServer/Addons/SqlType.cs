using DevZest.Data.Addons;
using DevZest.Data.Primitives;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data.SqlServer.Addons
{
    public abstract class SqlType : IAddon
    {
        private const string NULL = "NULL";

        private enum ValueKind
        {
            TSqlLiteral,
            XmlValue
        }

        internal abstract SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion);

        internal void GenerateColumnDefinitionSql(IndentedStringBuilder sqlBuilder, string tableName, bool isTempTable, SqlVersion sqlVersion)
        {
            GenerateDataTypeSql(sqlBuilder, isTempTable, sqlVersion);

            var column = GetColumn();
            GenerateDefaultConstraint(column, sqlBuilder, tableName, isTempTable, sqlVersion);

            // IDENTITY()
            var identity = column.GetIdentity(isTempTable);
            if (identity != null)
                sqlBuilder.Append(" IDENTITY(").Append(identity.Seed).Append(", ").Append(identity.Increment).Append(")");
        }

        private static void GenerateDefaultConstraint(Column column, IndentedStringBuilder sqlBuilder, string tableName, bool isTempTable, SqlVersion sqlVersion)
        {
            // DEFAULT
            var defaultConstraint = column.GetDefault();
            if (defaultConstraint == null || (!isTempTable && column.IsDbComputed))
                return;

            if (!isTempTable && !string.IsNullOrEmpty(defaultConstraint.Name))
            {
                sqlBuilder.Append(" CONSTRAINT ");
                sqlBuilder.Append(defaultConstraint.Name.FormatName(tableName));
            }
            sqlBuilder.Append(" DEFAULT(");
            defaultConstraint.DbExpression.GenerateSql(sqlBuilder, sqlVersion);
            sqlBuilder.Append(")");
        }

        private void GenerateDataTypeSql(IndentedStringBuilder sqlBuilder, bool isTempTable, SqlVersion sqlVersion)
        {
            var column = GetColumn();
            if (!isTempTable)
            {
                if (column.IsDbComputed)
                {
                    sqlBuilder.Append("AS (");
                    var generator = new ExpressionGenerator()
                    {
                        SqlBuilder = sqlBuilder,
                        SqlVersion = sqlVersion
                    };
                    column.DbComputedExpression.Accept(generator);
                    sqlBuilder.Append(")");
                    return;
                }
            }

            sqlBuilder.Append(GetDataTypeSql(sqlVersion));

            // NULL | NOT NULL
            bool isNullable = IsNullable;
            sqlBuilder.Append(isNullable ? " NULL" : " NOT NULL");
        }

        private bool IsNullable
        {
            get { return GetColumn().IsNullable; }
        }

        internal abstract string GetDataTypeSql(SqlVersion sqlVersion);

        internal abstract string GetTSqlLiteral(object value, SqlVersion sqlVersion);

        internal abstract string GetXmlValueByOrdinal(int ordinal, SqlVersion sqlVersion);

        internal abstract Column GetColumn();

        internal abstract object ConvertParameterValue(object value);

        internal SqlParameter CreateSqlParameter(string name, ParameterDirection direction, object value, SqlVersion version)
        {
            var result = new SqlParameter
            {
                ParameterName = name
            };

            // .Direction
            if (result.Direction != direction)
                result.Direction = direction;

            // .Size, .Precision, .Scale and .SqlDbType
            var sqlParamInfo = GetSqlParameterInfo(version);

            var sqlDbType = sqlParamInfo.SqlDbType;
            if (result.SqlDbType != sqlDbType)
                result.SqlDbType = sqlDbType;

            if (sqlDbType == SqlDbType.Udt)
                result.TypeName = sqlParamInfo.UdtTypeName;

            var size = sqlParamInfo.Size;
            if (size.HasValue && result.Size != size.GetValueOrDefault())
                result.Size = size.Value;

            var precision = sqlParamInfo.Precision;
            if (precision.HasValue && result.Precision != precision.GetValueOrDefault())
                result.Precision = precision.Value;

            var scale = sqlParamInfo.Scale;
            if (scale.HasValue && result.Scale != scale.GetValueOrDefault())
                result.Scale = scale.Value;

            // .IsNullable
            var isNullable = this.IsNullable;
            if (isNullable != result.IsNullable)
                result.IsNullable = isNullable;

            // .Value
            if (direction == ParameterDirection.Input || direction == ParameterDirection.InputOutput)
                result.Value = value == null ? DBNull.Value : ConvertParameterValue(value);

            result.SetDebugInfo(this, value, version);

            return result;
        }

        object IAddon.Key
        {
            get { return typeof(SqlType); }
        }

        private abstract class GenericSqlType<T> : SqlType
        {
            protected GenericSqlType(Column<T> column)
            {
                Column = column;
            }

            public Column<T> Column { get; private set; }

            internal sealed override string GetTSqlLiteral(object value, SqlVersion sqlVersion)
            {
                return value == null ? NULL : GetTSqlLiteral((T)value, sqlVersion);
            }

            internal sealed override string GetXmlValueByOrdinal(int ordinal, SqlVersion sqlVersion)
            {
                return GetXmlValue(Column[ordinal], sqlVersion);
            }

            protected abstract string GetTSqlLiteral(T value, SqlVersion sqlVersion);

            protected abstract string GetXmlValue(T value, SqlVersion sqlVersion);

            internal sealed override Column GetColumn()
            {
                return Column;
            }
        }

        private abstract class StructSqlType<T> : GenericSqlType<Nullable<T>>
            where T : struct
        {
            protected StructSqlType(Column<Nullable<T>> column)
                : base(column)
            {
            }

            internal sealed override object ConvertParameterValue(object value)
            {
                Nullable<T> x = (Nullable<T>)value;
                if (x.HasValue)
                    return x.GetValueOrDefault();
                else
                    return DBNull.Value;
            }

            protected sealed override string GetTSqlLiteral(T? value, SqlVersion sqlVersion)
            {
                return value.HasValue ? GetValue(value.GetValueOrDefault(), ValueKind.TSqlLiteral, sqlVersion) : NULL;
            }

            protected sealed override string GetXmlValue(T? value, SqlVersion sqlVersion)
            {
                return value.HasValue ? GetValue(value.GetValueOrDefault(), ValueKind.XmlValue, sqlVersion) : string.Empty;
            }

            protected abstract string GetValue(T value, ValueKind kind, SqlVersion sqlVersion);
        }

        private sealed class BigIntType : StructSqlType<Int64>
        {
            public BigIntType(Column<Int64?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.BigInt();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "BIGINT";
            }

            protected override string GetValue(long value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType BigInt(Column<Int64?> int64Column)
        {
            return new BigIntType(int64Column);
        }

        private sealed class DecimalType : StructSqlType<Decimal>
        {
            public DecimalType(Column<Decimal?> column, Byte precision, Byte scale)
                : base(column)
            {
                Precision = precision;
                Scale = scale;
            }

            public Byte Precision { get; private set; }

            public Byte Scale { get; private set; }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Decimal(Precision, Scale);
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return string.Format("DECIMAL({0}, {1})", Precision, Scale);
            }

            protected override string GetValue(decimal value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType Decimal(Column<Decimal?> decimalColumn, Byte precision, Byte scale)
        {
            return new DecimalType(decimalColumn, precision, scale);
        }

        private sealed class BitType : StructSqlType<Boolean>
        {
            public BitType(Column<Boolean?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Bit();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "BIT";
            }

            protected override string GetValue(bool value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value ? "1" : "0";
            }
        }

        internal static SqlType Bit(Column<Boolean?> booleanColumn)
        {
            return new BitType(booleanColumn);
        }

        private sealed class TinyIntType : StructSqlType<Byte>
        {
            public TinyIntType(Column<Byte?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.TinyInt();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "TINYINT";
            }

            protected override string GetValue(byte value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType TinyInt(Column<Byte?> byteColumn)
        {
            return new TinyIntType(byteColumn);
        }

        private sealed class SmallIntType : StructSqlType<Int16>
        {
            public SmallIntType(Column<Int16?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.SmallInt();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "SMALLINT";
            }

            protected override string GetValue(short value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType SmallInt(Column<Int16?> int16Column)
        {
            return new SmallIntType(int16Column);
        }

        private sealed class IntType : StructSqlType<Int32>
        {
            public IntType(Column<Int32?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Int();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "INT";
            }

            protected override string GetValue(int value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType Int(Column<Int32?> int32Column)
        {
            return new IntType(int32Column);
        }

        private sealed class SmallMoneyType : StructSqlType<Decimal>
        {
            public SmallMoneyType(Column<Decimal?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.SmallMoney();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "SMALLMONEY";
            }

            protected override string GetValue(decimal value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType SmallMoney(Column<Decimal?> decimalColumn)
        {
            return new SmallMoneyType(decimalColumn);
        }

        private sealed class MoneyType : StructSqlType<Decimal>
        {
            public MoneyType(Column<Decimal?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Money();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "MONEY";
            }

            protected override string GetValue(decimal value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType Money(Column<Decimal?> decimalColumn)
        {
            return new MoneyType(decimalColumn);
        }

        private abstract class MaxSizeSqlType<T> : GenericSqlType<T>
            where T : class
        {
            protected MaxSizeSqlType(Column<T> column, int size)
                : base(column)
            {
                Size = size;
            }

            public int Size { get; private set; }

            protected abstract SqlParameterInfo GetSqlParameterInfo(int size);

            protected abstract string GetSqlDataType();

            internal sealed override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return GetSqlParameterInfo(Size);
            }

            internal sealed override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                SqlParameterInfo paramInfo = GetSqlParameterInfo(sqlVersion);
                Debug.Assert(paramInfo.Size.HasValue);
                var size = paramInfo.Size.GetValueOrDefault();
                return size == -1 ? GetSqlDataType() + "(MAX)" : string.Format(CultureInfo.InvariantCulture, GetSqlDataType() + "({0})", size);
            }
        }

        private abstract class BinaryTypeBase : MaxSizeSqlType<Binary>
        {
            protected BinaryTypeBase(Column<Binary> column, int size)
                : base(column, size)
            {
            }

            internal sealed override object ConvertParameterValue(object value)
            {
                var result = (Binary)value;
                if (result == null)
                    return DBNull.Value;
                else
                    return result.ToArray();
            }

            protected sealed override string GetTSqlLiteral(Binary value, SqlVersion sqlVersion)
            {
                return value.ToXmlValue(sqlVersion).ToSingleQuoted();
            }

            protected sealed override string GetXmlValue(Binary value, SqlVersion sqlVersion)
            {
                return value.ToXmlValue(sqlVersion);
            }
        }

        private sealed class BinaryType : BinaryTypeBase
        {
            public BinaryType(Column<Binary> column, int size)
                : base(column, size)
            {
            }

            protected override SqlParameterInfo GetSqlParameterInfo(int size)
            {
                return SqlParameterInfo.Binary(size);
            }

            protected override string GetSqlDataType()
            {
                return "BINARY";
            }
        }

        private static void GenerateBinaryConst(IndentedStringBuilder sqlBuilder, Binary value)
        {
            if (value != null)
            {
            }
            else
                sqlBuilder.Append(NULL);
        }


        internal static SqlType Binary(Column<Binary> binaryColumn, int size)
        {
            return new BinaryType(binaryColumn, size);
        }

        private sealed class VarBinaryType : BinaryTypeBase
        {
            public VarBinaryType(Column<Binary> column, int size)
                : base(column, size)
            {
            }

            protected override string GetSqlDataType()
            {
                return "VARBINARY";
            }

            protected override SqlParameterInfo GetSqlParameterInfo(int size)
            {
                return SqlParameterInfo.VarBinary(size);
            }
        }

        internal static SqlType VarBinary(Column<Binary> binaryColumn, int size)
        {
            return new VarBinaryType(binaryColumn, size);
        }

        private sealed class TimestampType : GenericSqlType<Binary>
        {
            public TimestampType(Column<Binary> column)
                : base(column)
            {
            }

            internal override object ConvertParameterValue(object value)
            {
                var result = (Binary)value;
                if (result == null)
                    return DBNull.Value;
                else
                    return result.ToArray();
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Timestamp();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "TIMESTAMP";
            }

            protected override string GetTSqlLiteral(Binary value, SqlVersion sqlVersion)
            {
                return value.ToXmlValue(sqlVersion).ToSingleQuoted();
            }

            protected override string GetXmlValue(Binary value, SqlVersion sqlVersion)
            {
                return value.ToXmlValue(sqlVersion);
            }
        }

        internal static SqlType Timestamp(Column<Binary> binaryColumn)
        {
            return new TimestampType(binaryColumn);
        }

        private sealed class UniqueIdentifierType : StructSqlType<Guid>
        {
            public UniqueIdentifierType(Column<Guid?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.UniqueIdentifier();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "UNIQUEIDENTIFIER";
            }

            protected override string GetValue(Guid value, ValueKind kind, SqlVersion sqlVersion)
            {
                var text = value.ToString();
                return kind == ValueKind.TSqlLiteral ? text.ToSingleQuoted() : text;
            }
        }

        internal static SqlType UniqueIdentifier(Column<Guid?> column)
        {
            return new UniqueIdentifierType(column);
        }

        private abstract class DateTimeTypeBase : StructSqlType<DateTime>
        {
            protected DateTimeTypeBase(Column<DateTime?> column)
                : base(column)
            {
            }

            protected sealed override string GetValue(DateTime value, ValueKind kind, SqlVersion sqlVersion)
            {
                var xmlValue = GetXmlValue(value, sqlVersion);
                return kind == ValueKind.TSqlLiteral ? xmlValue.ToSingleQuoted() : xmlValue;
            }

            protected abstract string GetXmlValue(DateTime value, SqlVersion sqlVersion);
        }

        private sealed class DateType : DateTimeTypeBase
        {
            public DateType(Column<DateTime?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Date();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "DATE";
            }

            protected override string GetXmlValue(DateTime value, SqlVersion sqlVersion)
            {
                return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType Date(Column<DateTime?> dateTimeColumn)
        {
            return new DateType(dateTimeColumn);
        }

        private sealed class TimeType : DateTimeTypeBase
        {
            public TimeType(Column<DateTime?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Time();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "TIME";
            }

            protected override string GetXmlValue(DateTime value, SqlVersion sqlVersion)
            {
                return value.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType Time(Column<DateTime?> column)
        {
            return new TimeType(column);
        }

        private sealed class DateTimeType : DateTimeTypeBase
        {
            public DateTimeType(Column<DateTime?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.DateTime();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "DATETIME";
            }

            protected override string GetXmlValue(DateTime value, SqlVersion sqlVersion)
            {
                return value.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType DateTime(Column<DateTime?> column)
        {
            return new DateTimeType(column);
        }

        private sealed class SmallDateTimeType : DateTimeTypeBase
        {
            public SmallDateTimeType(Column<DateTime?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.SmallDateTime();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "SMALLDATETIME";
            }

            protected override string GetXmlValue(DateTime value, SqlVersion sqlVersion)
            {
                return value.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType SmallDateTime(Column<DateTime?> column)
        {
            return new SmallDateTimeType(column);
        }

        private sealed class DateTime2Type : DateTimeTypeBase
        {
            public DateTime2Type(Column<DateTime?> column, byte precision)
                : base(column)
            {
                Precision = precision;
            }

            public Byte Precision { get; private set; }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.DateTime2(Precision);
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return string.Format(CultureInfo.InvariantCulture, "DATETIME2({0})", Precision);
            }

            protected override string GetXmlValue(DateTime value, SqlVersion sqlVersion)
            {
                return value.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType DateTime2(Column<DateTime?> column, byte precision)
        {
            return new DateTime2Type(column, precision);
        }

        private sealed class DateTimeOffsetType : StructSqlType<DateTimeOffset>
        {
            public DateTimeOffsetType(Column<DateTimeOffset?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.DateTimeOffset();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "DATETIMEOFFSET";
            }

            protected override string GetValue(DateTimeOffset value, ValueKind kind, SqlVersion sqlVersion)
            {
                var xmlValue = value.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz", CultureInfo.InvariantCulture);
                return kind == ValueKind.TSqlLiteral ? xmlValue.ToSingleQuoted() : xmlValue;
            }
        }

        internal static SqlType DateTimeOffset(Column<DateTimeOffset?> dateTimeOffsetColumn)
        {
            return new DateTimeOffsetType(dateTimeOffsetColumn);
        }

        private sealed class SingleType : StructSqlType<Single>
        {
            public SingleType(Column<Single?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Single();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "FLOAT(24)";
            }

            protected override string GetValue(float value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString("R", CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType Single(Column<Single?> column)
        {
            return new SingleType(column);
        }

        private sealed class DoubleType : StructSqlType<Double>
        {
            public DoubleType(Column<Double?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Double();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "FLOAT(53)";
            }

            protected override string GetValue(double value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString("R", CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType Double(Column<Double?> column)
        {
            return new DoubleType(column);
        }

        private abstract class StringTypeBase : MaxSizeSqlType<String>
        {
            protected StringTypeBase(Column<String> column, int size)
                : base(column, size)
            {
            }

            internal sealed override object ConvertParameterValue(object value)
            {
                if (value == null)
                    return DBNull.Value;
                else
                    return value;
            }

            protected abstract bool IsUnicode { get; }

            protected sealed override string GetTSqlLiteral(string value, SqlVersion sqlVersion)
            {
                return value.ToTSqlLiteral(IsUnicode);
            }

            protected sealed override string GetXmlValue(string value, SqlVersion sqlVersion)
            {
                return value;
            }
        }

        private sealed class NVarCharType : StringTypeBase
        {
            public NVarCharType(Column<String> column, int size)
                : base(column, size)
            {
            }

            protected override string GetSqlDataType()
            {
                return "NVARCHAR";
            }

            protected override SqlParameterInfo GetSqlParameterInfo(int size)
            {
                return SqlParameterInfo.NVarChar(size);
            }

            protected override bool IsUnicode
            {
                get { return true; }
            }
        }

        internal static SqlType NVarChar(Column<String> column, int size)
        {
            return new NVarCharType(column, size);
        }

        private sealed class NCharType : StringTypeBase
        {
            public NCharType(Column<String> column, int size)
                : base(column, size)
            {
            }

            protected override string GetSqlDataType()
            {
                return "NCHAR";
            }

            protected override SqlParameterInfo GetSqlParameterInfo(int size)
            {
                return SqlParameterInfo.NChar(size);
            }

            protected override bool IsUnicode
            {
                get { return true; }
            }
        }

        internal static SqlType NChar(Column<String> column, int size)
        {
            return new NCharType(column, size);
        }

        private sealed class VarCharType : StringTypeBase
        {
            public VarCharType(Column<String> column, int size)
                : base(column, size)
            {
            }

            protected override string GetSqlDataType()
            {
                return "VARCHAR";
            }

            protected override SqlParameterInfo GetSqlParameterInfo(int size)
            {
                return SqlParameterInfo.VarChar(size);
            }

            protected override bool IsUnicode
            {
                get { return false; }
            }
        }

        internal static SqlType VarChar(Column<String> column, int size)
        {
            return new VarCharType(column, size);
        }

        private sealed class CharType : StringTypeBase
        {
            public CharType(Column<String> column, int size)
                : base(column, size)
            {
            }

            protected override string GetSqlDataType()
            {
                return "CHAR";
            }

            protected override SqlParameterInfo GetSqlParameterInfo(int size)
            {
                return SqlParameterInfo.Char(size);
            }

            protected override bool IsUnicode
            {
                get { return false; }
            }
        }

        internal static SqlType Char(Column<String> column, int size)
        {
            return new CharType(column, size);
        }

        private sealed class SingleCharType : StructSqlType<Char>
        {
            public SingleCharType(Column<Char?> column, bool isUnicode)
                : base(column)
            {
                IsUnicode = isUnicode;
            }

            public bool IsUnicode { get; private set; }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return IsUnicode ? SqlParameterInfo.NChar(1) : SqlParameterInfo.Char(1);
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return IsUnicode ? "NCHAR(1)" : "CHAR(1)";
            }

            protected override string GetValue(char value, ValueKind kind, SqlVersion sqlVersion)
            {
                var text = value.ToString();
                return kind == ValueKind.TSqlLiteral ? text.ToTSqlLiteral(IsUnicode) : text;
            }
        }

        internal static SqlType SingleChar(Column<Char?> column, bool isUnicode)
        {
            return new SingleCharType(column, isUnicode);
        }

        private sealed class TimeSpanType : StructSqlType<TimeSpan>
        {
            public TimeSpanType(Column<TimeSpan?> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Time();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "TIME";
            }

            protected override string GetValue(TimeSpan value, ValueKind kind, SqlVersion sqlVersion)
            {
                var xmlValue = value.ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture);
                return kind == ValueKind.TSqlLiteral ? xmlValue.ToSingleQuoted() : xmlValue;
            }
        }

        internal static SqlType TimeSpan(Column<TimeSpan?> column)
        {
            return new TimeSpanType(column);
        }

        private sealed class XmlType : GenericSqlType<SqlXml>
        {
            public XmlType(Column<SqlXml> column)
                : base(column)
            {
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Xml();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "XML";
            }

            internal override object ConvertParameterValue(object value)
            {
                if (value == null)
                    return DBNull.Value;
                else
                    return value;
            }

            protected override string GetTSqlLiteral(System.Data.SqlTypes.SqlXml value, SqlVersion sqlVersion)
            {
                return value == null ? NULL : value.Value.ToTSqlLiteral(true);
            }

            protected override string GetXmlValue(System.Data.SqlTypes.SqlXml value, SqlVersion sqlVersion)
            {
                return value == null ? string.Empty : value.Value;
            }
        }

        internal static SqlType Xml(Column<SqlXml> column)
        {
            return new XmlType(column);
        }

        private abstract class EnumType<TEnum, TValue> : SqlType
            where TValue : struct
        {
            internal sealed override object ConvertParameterValue(object value)
            {
                if (value == null)
                    return DBNull.Value;

                var dbValue = GetDbValue((TEnum)value);
                if (dbValue.HasValue)
                    return dbValue.GetValueOrDefault();
                else
                    return DBNull.Value;
            }

            protected abstract Column<TEnum> GetEnumColumn();

            internal sealed override Column GetColumn()
            {
                return GetEnumColumn();
            }

            protected abstract TValue? GetDbValue(TEnum enumValue);

            internal sealed override string GetTSqlLiteral(object value, SqlVersion sqlVersion)
            {
                if (value == null)
                    return NULL;

                var dbValue = GetDbValue((TEnum)value);
                return dbValue.HasValue ? GetTSqlLiteral(dbValue.GetValueOrDefault(), sqlVersion) : NULL;
            }

            internal sealed override string GetXmlValueByOrdinal(int ordinal, SqlVersion sqlVersion)
            {
                var enumValue = GetEnumColumn()[ordinal];
                var dbValue = GetDbValue(enumValue);
                return dbValue.HasValue ? GetXmlValue(dbValue.GetValueOrDefault(), sqlVersion) : string.Empty;
            }

            protected abstract string GetTSqlLiteral(TValue value, SqlVersion sqlVersion);

            protected abstract string GetXmlValue(TValue value, SqlVersion sqlVersion);
        }

        private sealed class CharEnumType<T> : EnumType<T?, char>
            where T : struct, IConvertible
        {
            public CharEnumType(_CharEnum<T> column)
            {
                Column = column;
            }

            public _CharEnum<T> Column { get; private set; }

            protected override Column<T?> GetEnumColumn()
            {
                return Column;
            }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Char(1);
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "CHAR(1)";
            }

            protected override char? GetDbValue(T? enumValue)
            {
                return Column.ConvertToDbValue(enumValue);
            }

            protected override string GetXmlValue(char value, SqlVersion sqlVersion)
            {
                return new string(new char[] { value });
            }

            protected override string GetTSqlLiteral(char value, SqlVersion sqlVersion)
            {
                return new string(new char[] { '\'', value, '\'' });
            }
        }

        internal static SqlType CharEnum<T>(_CharEnum<T> column)
            where T : struct, IConvertible
        {
            return new CharEnumType<T>(column);
        }

        private sealed class ByteEnumType<T> : EnumType<T?, byte>
            where T : struct, IConvertible
        {
            public ByteEnumType(_ByteEnum<T> column)
            {
                Column = column;
            }

            public _ByteEnum<T> Column { get; private set; }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.TinyInt();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "TINYINT";
            }

            protected override byte? GetDbValue(T? value)
            {
                return Column.ConvertToDbValue(value);
            }

            protected override Column<T?> GetEnumColumn()
            {
                return Column;
            }

            protected override string GetTSqlLiteral(byte value, SqlVersion sqlVersion)
            {
                return GetXmlValue(value, sqlVersion);
            }

            protected override string GetXmlValue(byte value, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType ByteEnum<T>(_ByteEnum<T> column)
            where T : struct, IConvertible
        {
            return new ByteEnumType<T>(column);
        }

        private sealed class Int16EnumType<T> : EnumType<T?, Int16>
            where T : struct, IConvertible
        {
            public Int16EnumType(_Int16Enum<T> column)
            {
                Column = column;
            }

            public _Int16Enum<T> Column { get; private set; }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.SmallInt();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "SMALLINT";
            }

            protected override Int16? GetDbValue(T? value)
            {
                return Column.ConvertToDbValue(value);
            }

            protected override Column<T?> GetEnumColumn()
            {
                return Column;
            }

            protected override string GetTSqlLiteral(Int16 value, SqlVersion sqlVersion)
            {
                return GetXmlValue(value, sqlVersion);
            }

            protected override string GetXmlValue(Int16 value, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType Int16Enum<T>(_Int16Enum<T> column)
            where T : struct, IConvertible
        {
            return new Int16EnumType<T>(column);
        }

        private sealed class Int32EnumType<T> : EnumType<T?, Int32>
            where T : struct, IConvertible
        {
            public Int32EnumType(_Int32Enum<T> column)
            {
                Column = column;
            }

            public _Int32Enum<T> Column { get; private set; }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Int();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "INT";
            }

            protected override Int32? GetDbValue(T? value)
            {
                return Column.ConvertToDbValue(value);
            }

            protected override Column<T?> GetEnumColumn()
            {
                return Column;
            }

            protected override string GetTSqlLiteral(Int32 value, SqlVersion sqlVersion)
            {
                return GetXmlValue(value, sqlVersion);
            }

            protected override string GetXmlValue(Int32 value, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType Int32Enum<T>(_Int32Enum<T> column)
            where T : struct, IConvertible
        {
            return new Int32EnumType<T>(column);
        }

        private sealed class Int64EnumType<T> : EnumType<T?, Int64>
            where T : struct, IConvertible
        {
            public Int64EnumType(_Int64Enum<T> column)
            {
                Column = column;
            }

            public _Int64Enum<T> Column { get; private set; }

            internal override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.BigInt();
            }

            internal override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "BIGINT";
            }

            protected override Int64? GetDbValue(T? value)
            {
                return Column.ConvertToDbValue(value);
            }

            protected override Column<T?> GetEnumColumn()
            {
                return Column;
            }

            protected override string GetTSqlLiteral(Int64 value, SqlVersion sqlVersion)
            {
                return GetXmlValue(value, sqlVersion);
            }

            protected override string GetXmlValue(Int64 value, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static SqlType Int64Enum<T>(_Int64Enum<T> column)
            where T : struct, IConvertible
        {
            return new Int64EnumType<T>(column);
        }
    }
}
