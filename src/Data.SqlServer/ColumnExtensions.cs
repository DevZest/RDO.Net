using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using DevZest.Data.Primitives;
using System.Data.SqlTypes;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq.Expressions;
using DevZest.Data.Addons;

namespace DevZest.Data.SqlServer
{
    public static class ColumnExtensions
    {
        private const int MIN_VARBINARY_SIZE = 1;
        internal const int MAX_VARBINARY_SIZE = 8000;

        private const int MIN_NVARCHAR_SIZE = 1;
        internal const int MAX_NVARCHAR_SIZE = 4000;

        private const int MIN_VARCHAR_SIZE = 1;
        internal const int MAX_VARCHAR_SIZE = 8000;

        private const int MIN_CHAR_SIZE = 1;
        internal const int MAX_CHAR_SIZE = 8000;

        private const int MIN_NCHAR_SIZE = 1;
        internal const int MAX_NCHAR_SIZE = 4000;

        private const byte MIN_DECIMAL_PRECISION = 1;
        private const byte MAX_DECIMAL_PRECISION = 38;
        internal const byte DEFAULT_DECIMAL_PRECISION = 38;

        internal const byte DEFAULT_DECIMAL_SCALE = 0;

        private const byte MIN_DATETIME2_PRECISION = 1;
        private const byte MAX_DATETIME2_PRECISION = 7;

        private const int MIN_BINARY_SIZE = 1;
        internal const int MAX_BINARY_SIZE = 8000;        

        private static void SetSqlType(this Column column, SqlType sqlType)
        {
            column.AddOrUpdate(sqlType);
        }

        public static _Binary AsSqlBinary(this _Binary column, int size)
        {
            column.VerifyNotNull(nameof(column));

            if (size < MIN_BINARY_SIZE || size > MAX_BINARY_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetSqlType(SqlType.Binary(column, size));
            return column;
        }

        public static _Binary AsSqlBinaryMax(this _Binary column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.Binary(column, -1));
            return column;
        }

        public static _Binary AsSqlVarBinary(this _Binary column, int size = MAX_VARBINARY_SIZE)
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_VARBINARY_SIZE || size > MAX_VARBINARY_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetSqlType(SqlType.VarBinary(column, size));
            return column;
        }

        public static _Binary AsSqlVarBinaryMax(this _Binary column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.VarBinary(column, -1));
            return column;
        }

        public static _Binary AsSqlTimestamp(this _Binary column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.Timestamp(column));
            return column;
        }

        public static _Decimal AsSqlDecimal(this _Decimal column, byte precision = DEFAULT_DECIMAL_PRECISION, byte scale = DEFAULT_DECIMAL_SCALE)
        {
            column.VerifyNotNull(nameof(column));
            if (precision < MIN_DECIMAL_PRECISION || precision > MAX_DECIMAL_PRECISION)
                throw new ArgumentOutOfRangeException(nameof(precision));
            if (scale < 0 || scale > precision)
                throw new ArgumentOutOfRangeException(nameof(scale));
            column.SetSqlType(SqlType.Decimal(column, precision, scale));
            return column;
        }

        public static _Decimal AsSqlSmallMoney(this _Decimal column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.SmallMoney(column));
            return column;
        }

        public static _Decimal AsSqlMoney(this _Decimal column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.Money(column));
            return column;
        }

        public static _DateTime AsSqlDate(this _DateTime column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.Date(column));
            return column;
        }

        public static _DateTime AsSqlTime(this _DateTime column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.Time(column));
            return column;
        }

        public static _DateTime AsSqlSmallDateTime(this _DateTime column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.SmallDateTime(column));
            return column;
        }

        public static _DateTime AsSqlDateTime(this _DateTime column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.DateTime(column));
            return column;
        }

        public static _DateTime AsSqlDateTime2(this _DateTime column, byte precision)
        {
            column.VerifyNotNull(nameof(column));
            if (precision < MIN_DATETIME2_PRECISION || precision > MAX_DATETIME2_PRECISION)
                throw new ArgumentOutOfRangeException(nameof(precision));
            column.SetSqlType(SqlType.DateTime2(column, precision));
            return column;
        }

        public static _String AsSqlNChar(this _String column, int size)
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_CHAR_SIZE || size > MAX_CHAR_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetSqlType(SqlType.NChar(column, size));
            return column;
        }

        public static _String AsSqlNCharMax(this _String column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.NChar(column, -1));
            return column;
        }

        public static _String AsSqlNVarChar(this _String column, int size = MAX_NVARCHAR_SIZE)
        {
            column.VerifyNotNull(nameof(column));
            if ((size < MIN_NVARCHAR_SIZE || size > MAX_NVARCHAR_SIZE) && size != -1)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetSqlType(SqlType.NVarChar(column, size));
            return column;
        }

        public static _String AsSqlNVarCharMax(this _String column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.NVarChar(column, -1));
            return column;
        }

        public static T AsSqlChar<T>(this T column, bool isUnicode)
            where T: Column<Char?>
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.SingleChar(column, isUnicode));
            return column;
        }

        public static _String AsSqlChar(this _String column, int size)
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_CHAR_SIZE || size > MAX_CHAR_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetSqlType(SqlType.Char(column, size));
            return column;
        }

        public static _String AsSqlCharMax(this _String column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.Char(column, -1));
            return column;
        }

        public static T AsSqlVarChar<T>(this T column, int size = MAX_VARBINARY_SIZE)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_VARCHAR_SIZE || size > MAX_VARCHAR_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetSqlType(SqlType.VarChar(column, size));
            return column;
        }

        public static _String AsSqlVarCharMax(this _String column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetSqlType(SqlType.VarChar(column, -1));
            return column;
        }

        internal static SqlType GetSqlType(this Column column)
        {
            var result = column.GetAddon<SqlType>() ?? column.GetDefaultSqlType();
            return result;
        }

        private abstract class SqlTypeProvider
        {
            public abstract SqlType GetDefaultSqlType(Column column);

            public abstract Type DataType { get; }
        }

        private sealed class SqlTypeProvider<T> : SqlTypeProvider
        {
            public SqlTypeProvider(Func<Column<T>, SqlType> callback)
            {
                Debug.Assert(callback != null);
                _callback = callback;
            }

            readonly Func<Column<T>, SqlType> _callback;

            public override Type DataType
            {
                get { return typeof(T); }
            }

            public override SqlType GetDefaultSqlType(Column column)
            {
                var x = column as Column<T>;
                return x == null ? null : _callback(x);
            }
        }

        private sealed class SqlTypeProviderCollection : Dictionary<Type, SqlTypeProvider>
        {
            public void Add(SqlTypeProvider value)
            {
                base.Add(value.DataType, value);
            }
        }

        private static readonly ConcurrentDictionary<Type, SqlTypeProvider> s_defaultSqlTypeProviders = new ConcurrentDictionary<Type, SqlTypeProvider>(
            new SqlTypeProviderCollection()
            {
                new SqlTypeProvider<Binary>(x => SqlType.VarBinary(x, MAX_VARBINARY_SIZE)),
                new SqlTypeProvider<Boolean?>(x => SqlType.Bit(x)),
                new SqlTypeProvider<Byte?>(x => SqlType.TinyInt(x)),
                new SqlTypeProvider<Char?>(x => SqlType.SingleChar(x, false)),
                new SqlTypeProvider<DateTime?>(x => SqlType.DateTime2(x, MAX_DATETIME2_PRECISION)),
                new SqlTypeProvider<DateTimeOffset?>(x => SqlType.DateTimeOffset(x)),
                new SqlTypeProvider<Decimal?>(x => SqlType.Decimal(x, DEFAULT_DECIMAL_PRECISION, DEFAULT_DECIMAL_SCALE)),
                new SqlTypeProvider<Double?>(x => SqlType.Double(x)),
                new SqlTypeProvider<Guid?>(x => SqlType.UniqueIdentifier(x)),
                new SqlTypeProvider<Int16?>(x => SqlType.SmallInt(x)),
                new SqlTypeProvider<Int32?>(x => SqlType.Int(x)),
                new SqlTypeProvider<Int64?>(x => SqlType.BigInt(x)),
                new SqlTypeProvider<Single?>(x => SqlType.Single(x)),
                new SqlTypeProvider<String>(x => SqlType.NVarChar(x, MAX_NVARCHAR_SIZE)),
                new SqlTypeProvider<TimeSpan?>(x => SqlType.TimeSpan(x)),
                new SqlTypeProvider<SqlXml>(x => SqlType.Xml(x))
            });

        private static SqlType GetDefaultSqlType(this Column column)
        {
            Debug.Assert(column != null);
            return column.GetDefaultSqlTypeProvider().GetDefaultSqlType(column);
        }

        private static SqlTypeProvider GetDefaultSqlTypeProvider(this Column column)
        {
            SqlTypeProvider result;
            if (s_defaultSqlTypeProviders.TryGetValue(column.DataType, out result))
                return result;

            var columnType = column.GetType();
            var columnDataType = column.DataType;
            var enumType = columnDataType.GenericTypeArguments[0];
            if (columnType.IsDerivedFromGeneric(typeof(_ByteEnum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetByteEnumColumnDescriptorProvider));
                return s_defaultSqlTypeProviders.GetOrAdd(columnType, BuildSqlTypeProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_CharEnum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetCharEnumColumnDescriptorProvider));
                return s_defaultSqlTypeProviders.GetOrAdd(columnType, BuildSqlTypeProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_Int16Enum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetInt16EnumColumnDescriptorProvider));
                return s_defaultSqlTypeProviders.GetOrAdd(columnType, BuildSqlTypeProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_Int32Enum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetInt32EnumColumnDescriptorProvider));
                return s_defaultSqlTypeProviders.GetOrAdd(columnType, BuildSqlTypeProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_Int64Enum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetInt64EnumColumnDescriptorProvider));
                return s_defaultSqlTypeProviders.GetOrAdd(columnType, BuildSqlTypeProviderFactory(methodInfo, enumType));
            }

            throw new NotSupportedException(DiagnosticMessages.ColumnTypeNotSupported(column.GetType()));
        }

        private static bool IsDerivedFromGeneric(this Type type, Type genericTypeDefinition)
        {
            for (; type != null; type = type.GetTypeInfo().BaseType)
            {
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
                    return true;
            }
            return false;
        }

        private static SqlTypeProvider<T?> GetCharEnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new SqlTypeProvider<T?>(x => SqlType.CharEnum<T>((_CharEnum<T>)x));
        }

        private static SqlTypeProvider<T?> GetByteEnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new SqlTypeProvider<T?>(x => SqlType.ByteEnum<T>((_ByteEnum<T>)x));
        }

        private static SqlTypeProvider<T?> GetInt16EnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new SqlTypeProvider<T?>(x => SqlType.Int16Enum<T>((_Int16Enum<T>)x));
        }

        private static SqlTypeProvider<T?> GetInt32EnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new SqlTypeProvider<T?>(x => SqlType.Int32Enum<T>((_Int32Enum<T>)x));
        }

        private static SqlTypeProvider<T?> GetInt64EnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new SqlTypeProvider<T?>(x => SqlType.Int64Enum<T>((_Int64Enum<T>)x));
        }

        private static Func<Type, SqlTypeProvider> BuildSqlTypeProviderFactory(MethodInfo methodInfo, Type columnDataType)
        {
            methodInfo = methodInfo.MakeGenericMethod(columnDataType);
            var call = Expression.Call(methodInfo);
            return Expression.Lambda<Func<Type, SqlTypeProvider>>(call, Expression.Parameter(typeof(Type))).Compile();
        }

        internal static SqlDbType GetSqlDbType(this Column column, SqlVersion sqlVersion)
        {
            return column.GetSqlType().GetSqlParameterInfo(sqlVersion).SqlDbType;
        }
    }
}
