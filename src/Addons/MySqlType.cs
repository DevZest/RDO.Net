using DevZest.Data.Addons;
using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data.MySql.Addons
{
    [Addon(typeof(MySqlType))]
    public abstract class MySqlType : IAddon
    {
        private const string NULL = "NULL";

        internal abstract MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion);

        internal void GenerateColumnDefinitionSql(IndentedStringBuilder sqlBuilder, string tableName, bool isTempTable, MySqlVersion mySqlVersion)
        {
            GenerateDataTypeSql(sqlBuilder, isTempTable, mySqlVersion);

            var column = GetColumn();
            GenerateDefaultConstraint(column, sqlBuilder, tableName, isTempTable, mySqlVersion);

            // AUTO_INCREMENT
            var identity = column.GetIdentity(isTempTable);
            if (identity != null)
                sqlBuilder.Append(" AUTO_INCREMENT");

            sqlBuilder.GenerateComment(column.DbColumnDescription);
        }

        private static void GenerateDefaultConstraint(Column column, IndentedStringBuilder sqlBuilder, string tableName, bool isTempTable, MySqlVersion mySqlVersion)
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
            defaultConstraint.DbExpression.GenerateSql(sqlBuilder, mySqlVersion);
            sqlBuilder.Append(")");
        }

        private void GenerateDataTypeSql(IndentedStringBuilder sqlBuilder, bool isTempTable, MySqlVersion mySqlVersion)
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
                        MySqlVersion = mySqlVersion
                    };
                    column.DbComputedExpression.Accept(generator);
                    sqlBuilder.Append(")");
                    return;
                }
            }

            sqlBuilder.Append(GetDataTypeSql(mySqlVersion));

            // NULL | NOT NULL
            bool isNullable = IsNullable;
            sqlBuilder.Append(isNullable ? " NULL" : " NOT NULL");
        }

        private bool IsNullable
        {
            get { return GetColumn().IsNullable; }
        }

        internal abstract string GetDataTypeSql(MySqlVersion mySqlVersion);

        internal abstract string GetLiteral(object value, MySqlVersion mySqlVersion);

        internal abstract Column GetColumn();

        internal abstract object ConvertParameterValue(object value);

        internal MySqlParameter CreateSqlParameter(string name, ParameterDirection direction, object value, MySqlVersion version)
        {
            var result = new MySqlParameter
            {
                ParameterName = name
            };

            // .Direction
            if (result.Direction != direction)
                result.Direction = direction;

            // .Size, .Precision, .Scale and .MySqlDbType
            var mySqlParamInfo = GetSqlParameterInfo(version);

            var mySqlDbType = mySqlParamInfo.MySqlDbType;
            if (result.MySqlDbType != mySqlDbType)
                result.MySqlDbType = mySqlDbType;

            var size = mySqlParamInfo.Size;
            if (size.HasValue && result.Size != size.GetValueOrDefault())
                result.Size = size.Value;

            var precision = mySqlParamInfo.Precision;
            if (precision.HasValue && result.Precision != precision.GetValueOrDefault())
                result.Precision = precision.Value;

            var scale = mySqlParamInfo.Scale;
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
            get { return typeof(MySqlType); }
        }

        private abstract class GenericSqlType<T> : MySqlType
        {
            protected GenericSqlType(Column<T> column)
            {
                Column = column;
            }

            public Column<T> Column { get; private set; }

            internal sealed override string GetLiteral(object value, MySqlVersion mySqlVersion)
            {
                return value == null ? NULL : GetValueLiteral((T)value, mySqlVersion);
            }

            protected abstract string GetValueLiteral(T value, MySqlVersion mySqlVersion);

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

            protected sealed override string GetValueLiteral(T? value, MySqlVersion mySqlVersion)
            {
                return value.HasValue ? GetValue(value.GetValueOrDefault(), mySqlVersion) : NULL;
            }

            protected abstract string GetValue(T value, MySqlVersion mySqlVersion);
        }

        private sealed class BigIntType : StructSqlType<Int64>
        {
            public BigIntType(Column<Int64?> column)
                : base(column)
            {
            }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Int64();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return "BIGINT";
            }

            protected override string GetValue(long value, MySqlVersion mySqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static MySqlType BigInt(Column<Int64?> int64Column)
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

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Decimal(Precision, Scale);
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return string.Format("DECIMAL({0}, {1})", Precision, Scale);
            }

            protected override string GetValue(decimal value, MySqlVersion mySqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static MySqlType Decimal(Column<Decimal?> decimalColumn, Byte precision, Byte scale)
        {
            return new DecimalType(decimalColumn, precision, scale);
        }

        private sealed class BitType : StructSqlType<Boolean>
        {
            public BitType(Column<Boolean?> column)
                : base(column)
            {
            }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Bit();
            }

            internal override string GetDataTypeSql(MySqlVersion sqlVersion)
            {
                return "BIT";
            }

            protected override string GetValue(bool value, MySqlVersion mySqlVersion)
            {
                return value ? "1" : "0";
            }
        }

        internal static MySqlType Bit(Column<Boolean?> booleanColumn)
        {
            return new BitType(booleanColumn);
        }

        private sealed class TinyIntType : StructSqlType<Byte>
        {
            public TinyIntType(Column<Byte?> column)
                : base(column)
            {
            }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Byte();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return "TINYINT";
            }

            protected override string GetValue(byte value, MySqlVersion mySqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static MySqlType TinyInt(Column<Byte?> byteColumn)
        {
            return new TinyIntType(byteColumn);
        }

        private sealed class SmallIntType : StructSqlType<Int16>
        {
            public SmallIntType(Column<Int16?> column)
                : base(column)
            {
            }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Int16();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return "SMALLINT";
            }

            protected override string GetValue(short value, MySqlVersion mySqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static MySqlType SmallInt(Column<Int16?> int16Column)
        {
            return new SmallIntType(int16Column);
        }

        private sealed class IntType : StructSqlType<Int32>
        {
            public IntType(Column<Int32?> column)
                : base(column)
            {
            }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Int32();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return "INT";
            }

            protected override string GetValue(int value, MySqlVersion mySqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static MySqlType Int(Column<Int32?> int32Column)
        {
            return new IntType(int32Column);
        }

        private abstract class SizedSqlType<T> : GenericSqlType<T>
            where T : class
        {
            protected SizedSqlType(Column<T> column, int size)
                : base(column)
            {
                Debug.Assert(size > 0);
                Size = size;
            }

            public int Size { get; private set; }

            protected abstract MySqlParameterInfo GetSqlParameterInfo(int size);

            protected abstract string GetSqlDataType();

            internal sealed override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return GetSqlParameterInfo(Size);
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                MySqlParameterInfo paramInfo = GetSqlParameterInfo(mySqlVersion);
                Debug.Assert(paramInfo.Size.HasValue);
                var size = paramInfo.Size.GetValueOrDefault();
                return string.Format(CultureInfo.InvariantCulture, GetSqlDataType() + "({0})", size);
            }
        }

        private abstract class BinaryTypeBase : SizedSqlType<Binary>
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

            protected sealed override string GetValueLiteral(Binary value, MySqlVersion mySqlVersion)
            {
                return value.ToHexLitera();
            }
        }

        private sealed class BinaryType : BinaryTypeBase
        {
            public BinaryType(Column<Binary> column, int size)
                : base(column, size)
            {
            }

            protected override MySqlParameterInfo GetSqlParameterInfo(int size)
            {
                return MySqlParameterInfo.Binary(size);
            }

            protected override string GetSqlDataType()
            {
                return "BINARY";
            }
        }

        internal static MySqlType Binary(Column<Binary> binaryColumn, int size)
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

            protected override MySqlParameterInfo GetSqlParameterInfo(int size)
            {
                return MySqlParameterInfo.VarBinary(size);
            }
        }

        internal static MySqlType VarBinary(Column<Binary> binaryColumn, int size)
        {
            return new VarBinaryType(binaryColumn, size);
        }

        private sealed class UniqueIdentifierType : StructSqlType<Guid>
        {
            public UniqueIdentifierType(Column<Guid?> column)
                : base(column)
            {
            }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion sqlVersion)
            {
                return MySqlParameterInfo.Guid();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return "CHAR(36)";
            }

            protected override string GetValue(Guid value, MySqlVersion mySqlVersion)
            {
                return value.ToString().ToSingleQuoted();
            }
        }

        internal static MySqlType UniqueIdentifier(Column<Guid?> column)
        {
            return new UniqueIdentifierType(column);
        }

        private sealed class DateType : StructSqlType<DateTime>
        {
            public DateType(Column<DateTime?> column)
                : base(column)
            {
            }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Date();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return "DATE";
            }

            protected override string GetValue(DateTime value, MySqlVersion mySqlVersion)
            {
                return string.Format(CultureInfo.InvariantCulture, "(DATE '{0}')", value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
        }

        internal static MySqlType Date(Column<DateTime?> dateTimeColumn)
        {
            return new DateType(dateTimeColumn);
        }

        private sealed class TimeType : StructSqlType<DateTime>
        {
            public TimeType(Column<DateTime?> column, int precision)
                : base(column)
            {
                Debug.Assert(precision >= 0 && precision <= ColumnExtensions.MAX_TIME_PRECISION);
                Precision = precision;
            }

            public int Precision { get; }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Time();
            }

            internal static readonly string[] s_dataTypes = new string[]
            {
                "TIME",
                "TIME(1)",
                "TIME(2)",
                "TIME(3)",
                "TIME(4)",
                "TIME(5)",
                "TIME(6)"
            };

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return s_dataTypes[Precision];
            }

            internal static readonly string[] s_formats = new string[]
            {
                "HH:mm:ss",
                "HH:mm:ss.f",
                "HH:mm:ss.ff",
                "HH:mm:ss.fff",
                "HH:mm:ss.ffff",
                "HH:mm:ss.fffff",
                "HH:mm:ss.ffffff"
            };

            protected override string GetValue(DateTime value, MySqlVersion mySqlVersion)
            {
                return string.Format(CultureInfo.InvariantCulture, "(TIME '{0}')", value.ToString(s_formats[Precision], CultureInfo.InvariantCulture));
            }
        }

        internal static MySqlType Time(Column<DateTime?> column, int precision)
        {
            return new TimeType(column, precision);
        }

        private class DateTimeType : StructSqlType<DateTime>
        {
            public DateTimeType(Column<DateTime?> column, int precision)
                : base(column)
            {
                Debug.Assert(precision >= 0 && precision <= ColumnExtensions.MAX_TIME_PRECISION);
                Precision = precision;
            }

            public int Precision { get; }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.DateTime();
            }

            private static readonly string[] s_dataTypes = new string[]
            {
                "DATETIME",
                "DATETIME(1)",
                "DATETIME(2)",
                "DATETIME(3)",
                "DATETIME(4)",
                "DATETIME(5)",
                "DATETIME(6)"
            };

            internal override string GetDataTypeSql(MySqlVersion sqlVersion)
            {
                return s_dataTypes[Precision];
            }

            private static readonly string[] s_formats = new string[]
            {
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd HH:mm:ss.f",
                "yyyy-MM-dd HH:mm:ss.ff",
                "yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-MM-dd HH:mm:ss.ffff",
                "yyyy-MM-dd HH:mm:ss.fffff",
                "yyyy-MM-dd HH:mm:ss.ffffff",
            };

            protected override string GetValue(DateTime value, MySqlVersion mySqlVersion)
            {
                return string.Format(CultureInfo.InvariantCulture, "(TIMESTAMP '{0}')", value.ToString(s_formats[Precision], CultureInfo.InvariantCulture));
            }
        }

        internal static MySqlType DateTime(Column<DateTime?> column, int precision)
        {
            return new DateTimeType(column, precision);
        }

        private sealed class TimestampType : DateTimeType
        {
            public TimestampType(Column<DateTime?> column, int precision)
                : base(column, precision)
            {
            }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Timestamp();
            }

            private static readonly string[] s_dataTypes = new string[]
            {
                "TIMESTAMP",
                "TIMESTAMP(1)",
                "TIMESTAMP(2)",
                "TIMESTAMP(3)",
                "TIMESTAMP(4)",
                "TIMESTAMP(5)",
                "TIMESTAMP(6)"
            };

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return s_dataTypes[Precision];
            }
        }

        internal static MySqlType Timestamp(Column<DateTime?> dateTimeColumn, int precision)
        {
            return new TimestampType(dateTimeColumn, precision);
        }

        private sealed class SingleType : StructSqlType<Single>
        {
            public SingleType(Column<Single?> column)
                : base(column)
            {
            }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Float();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return "FLOAT";
            }

            protected override string GetValue(float value, MySqlVersion mySqlVersion)
            {
                return value.ToString("R", CultureInfo.InvariantCulture);
            }
        }

        internal static MySqlType Single(Column<Single?> column)
        {
            return new SingleType(column);
        }

        private sealed class DoubleType : StructSqlType<Double>
        {
            public DoubleType(Column<Double?> column)
                : base(column)
            {
            }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Double();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return "DOUBLE";
            }

            protected override string GetValue(double value, MySqlVersion sqlVersion)
            {
                return value.ToString("R", CultureInfo.InvariantCulture);
            }
        }

        internal static MySqlType Double(Column<Double?> column)
        {
            return new DoubleType(column);
        }

        private sealed class JsonType : GenericSqlType<string>
        {
            public JsonType(Column<string> column)
                : base(column)
            {
            }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Json();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return "JSON";
            }

            internal override object ConvertParameterValue(object value)
            {
                if (value == null)
                    return DBNull.Value;
                else
                    return value;
            }

            protected override string GetValueLiteral(string value, MySqlVersion mySqlVersion)
            {
                return value.ToLiteral();
            }
        }

        private abstract class StringTypeBase : SizedSqlType<String>
        {
            protected StringTypeBase(Column<String> column, int size, string charSetName, string collationName)
                : base(column, size)
            {
                CharSetName = charSetName;
                CollationName = collationName;
            }

            private string CharSetName { get; }

            private string CollationName { get; }

            internal sealed override object ConvertParameterValue(object value)
            {
                if (value == null)
                    return DBNull.Value;
                else
                    return value;
            }

            protected sealed override string GetValueLiteral(string value, MySqlVersion mySqlVersion)
            {
                return value.ToLiteral();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return AppendCharSetAndCollation(base.GetDataTypeSql(mySqlVersion), CharSetName, CollationName);
            }

            internal static string AppendCharSetAndCollation(string dataType, string charSetName, string collationName)
            {
                if (!string.IsNullOrEmpty(charSetName))
                    dataType = dataType + " CHARACTER SET " + charSetName;
                if (!string.IsNullOrEmpty(collationName))
                    dataType = dataType + " COLLATE " + collationName;
                return dataType;
            }
        }

        internal static MySqlType Json(Column<String> column)
        {
            return new JsonType(column);
        }

        private sealed class TextType : StringTypeBase
        {
            public TextType(Column<String> column, int size, string charSetName, string collationName)
                : base(column, size, charSetName, collationName)
            {
            }

            protected override string GetSqlDataType()
            {
                return "TEXT";
            }

            protected override MySqlParameterInfo GetSqlParameterInfo(int size)
            {
                return MySqlParameterInfo.VarString(size);
            }
        }

        internal static MySqlType Text(Column<String> column, int size, string charSetName, string collationName)
        {
            return new TextType(column, size, charSetName, collationName);
        }

        private sealed class VarCharType : StringTypeBase
        {
            public VarCharType(Column<String> column, int size, string charSetName, string collationName)
                : base(column, size, charSetName, collationName)
            {
            }

            protected override string GetSqlDataType()
            {
                return "VARCHAR";
            }

            protected override MySqlParameterInfo GetSqlParameterInfo(int size)
            {
                return MySqlParameterInfo.VarString(size);
            }
        }

        internal static MySqlType VarChar(Column<String> column, int size, string charSetName, string collationName)
        {
            return new VarCharType(column, size, charSetName, collationName);
        }

        private sealed class CharType : StringTypeBase
        {
            public CharType(Column<String> column, int size, string charsetName, string collationName)
                : base(column, size, charsetName, collationName)
            {
            }

            protected override string GetSqlDataType()
            {
                return "CHAR";
            }

            protected override MySqlParameterInfo GetSqlParameterInfo(int size)
            {
                return MySqlParameterInfo.String(size);
            }
        }

        internal static MySqlType Char(Column<String> column, int size, string charSetName, string collationName)
        {
            return new CharType(column, size, charSetName, collationName);
        }

        private sealed class SingleCharType : StructSqlType<Char>
        {
            public SingleCharType(Column<Char?> column, string charSetName, string collationName)
                : base(column)
            {
                CharSetName = charSetName;
                CollationName = collationName;
            }

            private string CharSetName { get; }

            private string CollationName { get; }

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.String(1);
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return StringTypeBase.AppendCharSetAndCollation("CHAR(1)", CharSetName, CollationName);
            }

            protected override string GetValue(char value, MySqlVersion mySqlVersion)
            {
                return value.ToString().ToLiteral();
            }
        }

        internal static MySqlType SingleChar(Column<Char?> column, string charSetName, string collationName)
        {
            return new SingleCharType(column, charSetName, collationName);
        }

        private abstract class EnumType<TEnum, TValue> : MySqlType
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

            internal sealed override string GetLiteral(object value, MySqlVersion mySqlVersion)
            {
                if (value == null)
                    return NULL;

                var dbValue = GetDbValue((TEnum)value);
                return dbValue.HasValue ? GetValueLiteral(dbValue.GetValueOrDefault(), mySqlVersion) : NULL;
            }

            protected abstract string GetValueLiteral(TValue value, MySqlVersion mySqlVersion);
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

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.String(1);
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
            {
                return "CHAR(1)";
            }

            protected override char? GetDbValue(T? enumValue)
            {
                return Column.ConvertToDbValue(enumValue);
            }

            protected override string GetValueLiteral(char value, MySqlVersion mySqlVersion)
            {
                return new string(new char[] { '\'', value, '\'' });
            }
        }

        internal static MySqlType CharEnum<T>(_CharEnum<T> column)
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

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Byte();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
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

            protected override string GetValueLiteral(byte value, MySqlVersion mysqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static MySqlType ByteEnum<T>(_ByteEnum<T> column)
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

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Int16();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
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

            protected override string GetValueLiteral(Int16 value, MySqlVersion mySqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static MySqlType Int16Enum<T>(_Int16Enum<T> column)
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

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Int32();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
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

            protected override string GetValueLiteral(Int32 value, MySqlVersion mySqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static MySqlType Int32Enum<T>(_Int32Enum<T> column)
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

            internal override MySqlParameterInfo GetSqlParameterInfo(MySqlVersion mySqlVersion)
            {
                return MySqlParameterInfo.Int64();
            }

            internal override string GetDataTypeSql(MySqlVersion mySqlVersion)
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

            protected override string GetValueLiteral(Int64 value, MySqlVersion mySqlVersion)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal static MySqlType Int64Enum<T>(_Int64Enum<T> column)
            where T : struct, IConvertible
        {
            return new Int64EnumType<T>(column);
        }
    }
}
