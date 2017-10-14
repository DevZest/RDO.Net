using DevZest.Data.Primitives;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data.SqlServer
{
    internal abstract class ColumnMapper : IInterceptor
    {
        private const string NULL = "NULL";

        private enum ValueKind
        {
            TSqlLiteral,
            XmlValue
        }

        public abstract SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion);

        public void GenerateColumnDefinitionSql(IndentedStringBuilder sqlBuilder, bool isTempTable, SqlVersion sqlVersion)
        {
            GenerateDataTypeSql(sqlBuilder, isTempTable, sqlVersion);

            var column = GetColumn();
            GenerateDefault(column, sqlBuilder, isTempTable, sqlVersion);

            // IDENTITY()
            var identity = column.GetIdentity(isTempTable);
            if (identity != null)
                sqlBuilder.Append(" IDENTITY(").Append(identity.Seed).Append(", ").Append(identity.Increment).Append(")");
        }

        private static void GenerateDefault(Column column, IndentedStringBuilder sqlBuilder, bool isTempTable, SqlVersion sqlVersion)
        {
            // DEFAULT
            var defaultDef = column.GetDefault();
            if (defaultDef == null || (!isTempTable && column.IsDbComputed))
                return;

            sqlBuilder.Append(" DEFAULT(");
            defaultDef.DbExpression.GenerateSql(sqlBuilder, sqlVersion);
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

        public abstract string GetDataTypeSql(SqlVersion sqlVersion);

        public abstract string GetTSqlLiteral(object value, SqlVersion sqlVersion);

        public abstract string GetXmlValue(int ordinal, SqlVersion sqlVersion);

        public abstract Column GetColumn();

        protected abstract object ConvertParameterValue(object value);

        public SqlParameter CreateSqlParameter(string name, ParameterDirection direction, object value, SqlVersion version)
        {
            var result = new SqlParameter();
            
            result.ParameterName = name;

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

        public string FullName
        {
            get { return typeof(ColumnMapper).FullName; }
        }

        private abstract class MapperBase<T> : ColumnMapper
        {
            protected MapperBase(Column<T> column)
            {
                Column = column;
            }

            public Column<T> Column { get; private set; }

            public sealed override string GetTSqlLiteral(object value, SqlVersion sqlVersion)
            {
                return value == null ? NULL : GetTSqlLiteral((T)value, sqlVersion);
            }

            public sealed override string GetXmlValue(int ordinal, SqlVersion sqlVersion)
            {
                return GetXmlValue(Column[ordinal], sqlVersion);
            }

            protected abstract string GetTSqlLiteral(T value, SqlVersion sqlVersion);

            protected abstract string GetXmlValue(T value, SqlVersion sqlVersion);

            public sealed override Column GetColumn()
            {
                return Column;
            }
        }

        private abstract class StructMapper<T> : MapperBase<Nullable<T>>
            where T : struct
        {
            protected StructMapper(Column<Nullable<T>> column)
                : base(column)
            {
            }

            protected sealed override object ConvertParameterValue(object value)
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

        private sealed class BigIntMapper : StructMapper<Int64>
        {
            public BigIntMapper(Column<Int64?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.BigInt();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "BIGINT";
            }

            protected override string GetValue(long value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper BigInt(Column<Int64?> int64Column)
        {
            return new BigIntMapper(int64Column);
        }

        private sealed class DecimalMapper : StructMapper<Decimal>
        {
            public DecimalMapper(Column<Decimal?> column, Byte precision, Byte scale)
                : base(column)
            {
                Precision = precision;
                Scale = scale;
            }

            public Byte Precision { get; private set; }

            public Byte Scale { get; private set; }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Decimal(Precision, Scale);
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return string.Format("DECIMAL({0}, {1})", Precision, Scale);
            }

            protected override string GetValue(decimal value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper Decimal(Column<Decimal?> decimalColumn, Byte precision, Byte scale)
        {
            return new DecimalMapper(decimalColumn, precision, scale);
        }

        private sealed class BitMapper : StructMapper<Boolean>
        {
            public BitMapper(Column<Boolean?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Bit();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "BIT";
            }

            protected override string GetValue(bool value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value ? "1" : "0";
            }
        }

        public static ColumnMapper Bit(Column<Boolean?> booleanColumn)
        {
            return new BitMapper(booleanColumn);
        }

        private sealed class TinyIntMapper : StructMapper<Byte>
        {
            public TinyIntMapper(Column<Byte?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.TinyInt();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "TINYINT";
            }

            protected override string GetValue(byte value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper TinyInt(Column<Byte?> byteColumn)
        {
            return new TinyIntMapper(byteColumn);
        }

        private sealed class SmallIntMapper : StructMapper<Int16>
        {
            public SmallIntMapper(Column<Int16?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.SmallInt();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "SMALLINT";
            }

            protected override string GetValue(short value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper SmallInt(Column<Int16?> int16Column)
        {
            return new SmallIntMapper(int16Column);
        }

        private sealed class IntMapper : StructMapper<Int32>
        {
            public IntMapper(Column<Int32?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Int();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "INT";
            }

            protected override string GetValue(int value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper Int(Column<Int32?> int32Column)
        {
            return new IntMapper(int32Column);
        }

        private sealed class SmallMoneyMapper : StructMapper<Decimal>
        {
            public SmallMoneyMapper(Column<Decimal?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.SmallMoney();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "SMALLMONEY";
            }

            protected override string GetValue(decimal value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper SmallMoney(Column<Decimal?> decimalColumn)
        {
            return new SmallMoneyMapper(decimalColumn);
        }

        private sealed class MoneyMapper : StructMapper<Decimal>
        {
            public MoneyMapper(Column<Decimal?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Money();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "MONEY";
            }

            protected override string GetValue(decimal value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper Money(Column<Decimal?> decimalColumn)
        {
            return new MoneyMapper(decimalColumn);
        }

        private abstract class MaxSizeColumnMapper<T> : MapperBase<T>
            where T : class
        {
            protected MaxSizeColumnMapper(Column<T> column, int size)
                : base(column)
            {
                Size = size;
            }

            public int Size { get; private set; }

            protected abstract SqlParameterInfo GetSqlParameterInfo(int size);

            protected abstract string GetSqlDataType();

            public sealed override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return GetSqlParameterInfo(Size);
            }

            public sealed override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                SqlParameterInfo paramInfo = GetSqlParameterInfo(sqlVersion);
                Debug.Assert(paramInfo.Size.HasValue);
                var size = paramInfo.Size.GetValueOrDefault();
                return size == -1 ? GetSqlDataType() + "(MAX)" : string.Format(CultureInfo.InvariantCulture, GetSqlDataType() + "({0})", size);
            }
        }

        private abstract class BinaryMapperBase : MaxSizeColumnMapper<Binary>
        {
            protected BinaryMapperBase(Column<Binary> column, int size)
                : base(column, size)
            {
            }

            protected sealed override object ConvertParameterValue(object value)
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

        private sealed class BinaryMapper : BinaryMapperBase
        {
            public BinaryMapper(Column<Binary> column, int size)
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


        public static ColumnMapper Binary(Column<Binary> binaryColumn, int size)
        {
            return new BinaryMapper(binaryColumn, size);
        }

        private sealed class VarBinaryMapper : BinaryMapperBase
        {
            public VarBinaryMapper(Column<Binary> column, int size)
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

        public static ColumnMapper VarBinary(Column<Binary> binaryColumn, int size)
        {
            return new VarBinaryMapper(binaryColumn, size);
        }

        private sealed class TimestampMapper : MapperBase<Binary>
        {
            public TimestampMapper(Column<Binary> column)
                : base(column)
            {
            }

            protected override object ConvertParameterValue(object value)
            {
                var result = (Binary)value;
                if (result == null)
                    return DBNull.Value;
                else
                    return result.ToArray();
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Timestamp();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
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

        public static ColumnMapper Timestamp(Column<Binary> binaryColumn)
        {
            return new TimestampMapper(binaryColumn);
        }

        private sealed class UniqueIdentifierMapper : StructMapper<Guid>
        {
            public UniqueIdentifierMapper(Column<Guid?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.UniqueIdentifier();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "UNIQUEIDENTIFIER";
            }

            protected override string GetValue(Guid value, ValueKind kind, SqlVersion sqlVersion)
            {
                var text = value.ToString();
                return kind == ValueKind.TSqlLiteral ? text.ToSingleQuoted() : text;
            }
        }

        public static ColumnMapper UniqueIdentifier(Column<Guid?> column)
        {
            return new UniqueIdentifierMapper(column);
        }

        private abstract class DateTimeMapperBase : StructMapper<DateTime>
        {
            protected DateTimeMapperBase(Column<DateTime?> column)
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

        private sealed class DateMapper : DateTimeMapperBase
        {
            public DateMapper(Column<DateTime?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Date();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "DATE";
            }

            protected override string GetXmlValue(DateTime value, SqlVersion sqlVersion)
            {
                return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper Date(Column<DateTime?> dateTimeColumn)
        {
            return new DateMapper(dateTimeColumn);
        }

        private sealed class TimeMapper : DateTimeMapperBase
        {
            public TimeMapper(Column<DateTime?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Time();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "TIME";
            }

            protected override string GetXmlValue(DateTime value, SqlVersion sqlVersion)
            {
                return value.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper Time(Column<DateTime?> column)
        {
            return new TimeMapper(column);
        }

        private sealed class DateTimeMapper : DateTimeMapperBase
        {
            public DateTimeMapper(Column<DateTime?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.DateTime();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "DATETIME";
            }

            protected override string GetXmlValue(DateTime value, SqlVersion sqlVersion)
            {
                return value.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper DateTime(Column<DateTime?> column)
        {
            return new DateTimeMapper(column);
        }

        private sealed class SmallDateTimeMapper : DateTimeMapperBase
        {
            public SmallDateTimeMapper(Column<DateTime?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.SmallDateTime();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "SMALLDATETIME";
            }

            protected override string GetXmlValue(DateTime value, SqlVersion sqlVersion)
            {
                return value.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper SmallDateTime(Column<DateTime?> column)
        {
            return new SmallDateTimeMapper(column);
        }

        private sealed class DateTime2Mapper : DateTimeMapperBase
        {
            public DateTime2Mapper(Column<DateTime?> column, byte precision)
                : base(column)
            {
                Precision = precision;
            }

            public Byte Precision { get; private set; }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.DateTime2(Precision);
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return string.Format(CultureInfo.InvariantCulture, "DATETIME2({0})", Precision);
            }

            protected override string GetXmlValue(DateTime value, SqlVersion sqlVersion)
            {
                return value.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper DateTime2(Column<DateTime?> column, byte precision)
        {
            return new DateTime2Mapper(column, precision);
        }

        private sealed class DateTimeOffsetMapper : StructMapper<DateTimeOffset>
        {
            public DateTimeOffsetMapper(Column<DateTimeOffset?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.DateTimeOffset();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "DATETIMEOFFSET";
            }

            protected override string GetValue(DateTimeOffset value, ValueKind kind, SqlVersion sqlVersion)
            {
                var xmlValue = value.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz", CultureInfo.InvariantCulture);
                return kind == ValueKind.TSqlLiteral ? xmlValue.ToSingleQuoted() : xmlValue;
            }
        }

        public static ColumnMapper DateTimeOffset(Column<DateTimeOffset?> dateTimeOffsetColumn)
        {
            return new DateTimeOffsetMapper(dateTimeOffsetColumn);
        }

        private sealed class SingleMapper : StructMapper<Single>
        {
            public SingleMapper(Column<Single?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Single();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "FLOAT(24)";
            }

            protected override string GetValue(float value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString("R", CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper Single(Column<Single?> column)
        {
            return new SingleMapper(column);
        }


        private sealed class DoubleMapper : StructMapper<Double>
        {
            public DoubleMapper(Column<Double?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Double();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "FLOAT(53)";
            }

            protected override string GetValue(double value, ValueKind kind, SqlVersion sqlVersion)
            {
                return value.ToString("R", CultureInfo.InvariantCulture);
            }
        }

        public static ColumnMapper Double(Column<Double?> column)
        {
            return new DoubleMapper(column);
        }

        private abstract class StringMapperBase : MaxSizeColumnMapper<String>
        {
            protected StringMapperBase(Column<String> column, int size)
                : base(column, size)
            {
            }

            protected sealed override object ConvertParameterValue(object value)
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

        private sealed class NVarCharMapper : StringMapperBase
        {
            public NVarCharMapper(Column<String> column, int size)
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

        public static ColumnMapper NVarChar(Column<String> column, int size)
        {
            return new NVarCharMapper(column, size);
        }

        private sealed class NCharMapper : StringMapperBase
        {
            public NCharMapper(Column<String> column, int size)
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

        public static ColumnMapper NChar(Column<String> column, int size)
        {
            return new NCharMapper(column, size);
        }

        private sealed class VarCharMapper : StringMapperBase
        {
            public VarCharMapper(Column<String> column, int size)
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

        public static ColumnMapper VarChar(Column<String> column, int size)
        {
            return new VarCharMapper(column, size);
        }

        private sealed class CharMapper : StringMapperBase
        {
            public CharMapper(Column<String> column, int size)
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

        public static ColumnMapper Char(Column<String> column, int size)
        {
            return new CharMapper(column, size);
        }

        private sealed class SingleCharMapper : StructMapper<Char>
        {
            public SingleCharMapper(Column<Char?> column, bool isUnicode)
                : base(column)
            {
                IsUnicode = isUnicode;
            }

            public bool IsUnicode { get; private set; }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return IsUnicode ? SqlParameterInfo.NChar(1) : SqlParameterInfo.Char(1);
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return IsUnicode ? "NCHAR(1)" : "CHAR(1)";
            }

            protected override string GetValue(char value, ValueKind kind, SqlVersion sqlVersion)
            {
                var text = value.ToString();
                return kind == ValueKind.TSqlLiteral ? text.ToTSqlLiteral(IsUnicode) : text;
            }
        }

        public static ColumnMapper SingleChar(Column<Char?> column, bool isUnicode)
        {
            return new SingleCharMapper(column, isUnicode);
        }

        private sealed class TimeSpanMapper : StructMapper<TimeSpan>
        {
            public TimeSpanMapper(Column<TimeSpan?> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Time();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "TIME";
            }

            protected override string GetValue(TimeSpan value, ValueKind kind, SqlVersion sqlVersion)
            {
                var xmlValue = value.ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture);
                return kind == ValueKind.TSqlLiteral ? xmlValue.ToSingleQuoted() : xmlValue;
            }
        }

        public static ColumnMapper TimeSpan(Column<TimeSpan?> column)
        {
            return new TimeSpanMapper(column);
        }

        private sealed class XmlMapper : MapperBase<SqlXml>
        {
            public XmlMapper(Column<SqlXml> column)
                : base(column)
            {
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Xml();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
            {
                return "XML";
            }

            protected override object ConvertParameterValue(object value)
            {
                if (value == null)
                    return DBNull.Value;
                else
                    return value;
            }

            protected override string GetTSqlLiteral(SqlXml value, SqlVersion sqlVersion)
            {
                return value == null ? NULL : value.Value.ToTSqlLiteral(true);
            }

            protected override string GetXmlValue(SqlXml value, SqlVersion sqlVersion)
            {
                return value == null ? string.Empty : value.Value;
            }
        }

        public static ColumnMapper Xml(Column<SqlXml> column)
        {
            return new XmlMapper(column);
        }

        private abstract class EnumMapperBase<TEnum, TValue> : ColumnMapper
            where TValue : struct
        {
            protected sealed override object ConvertParameterValue(object value)
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

            public sealed override Column GetColumn()
            {
                return GetEnumColumn();
            }

            protected abstract TValue? GetDbValue(TEnum enumValue);

            public sealed override string GetTSqlLiteral(object value, SqlVersion sqlVersion)
            {
                if (value == null)
                    return NULL;

                var dbValue = GetDbValue((TEnum)value);
                return dbValue.HasValue ? GetTSqlLiteral(dbValue.GetValueOrDefault(), sqlVersion) : NULL;
            }

            public sealed override string GetXmlValue(int ordinal, SqlVersion sqlVersion)
            {
                var enumValue = GetEnumColumn()[ordinal];
                var dbValue = GetDbValue(enumValue);
                return dbValue.HasValue ? GetXmlValue(dbValue.GetValueOrDefault(), sqlVersion) : string.Empty;
            }

            protected abstract string GetTSqlLiteral(TValue value, SqlVersion sqlVersion);

            protected abstract string GetXmlValue(TValue value, SqlVersion sqlVersion);
        }

        private sealed class CharEnumMapper<T> : EnumMapperBase<T?, char>
            where T : struct, IConvertible
        {
            public CharEnumMapper(_CharEnum<T> column)
            {
                Column = column;
            }

            public _CharEnum<T> Column { get; private set; }

            protected override Column<T?> GetEnumColumn()
            {
                return Column;
            }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.Char(1);
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
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

        public static ColumnMapper CharEnum<T>(_CharEnum<T> column)
            where T : struct, IConvertible
        {
            return new CharEnumMapper<T>(column);
        }

        private sealed class ByteEnumMapper<T> : EnumMapperBase<T?, byte>
            where T : struct, IConvertible
        {
            public ByteEnumMapper(_ByteEnum<T> column)
            {
                Column = column;
            }

            public _ByteEnum<T> Column { get; private set; }

            public override SqlParameterInfo GetSqlParameterInfo(SqlVersion sqlVersion)
            {
                return SqlParameterInfo.TinyInt();
            }

            public override string GetDataTypeSql(SqlVersion sqlVersion)
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

        public static ColumnMapper ByteEnum<T>(_ByteEnum<T> column)
            where T : struct, IConvertible
        {
            return new ByteEnumMapper<T>(column);
        }
    }
}
