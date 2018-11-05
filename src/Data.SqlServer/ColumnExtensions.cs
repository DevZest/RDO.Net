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

        private static void SetMapper(this Column column, SqlColumnDescriptor columnMapper)
        {
            column.AddOrUpdate(columnMapper);
        }

        public static _Binary AsSqlBinary(this _Binary column, int size)
        {
            column.VerifyNotNull(nameof(column));

            if (size < MIN_BINARY_SIZE || size > MAX_BINARY_SIZE)
                throw new ArgumentOutOfRangeException("size");
            column.SetMapper(SqlColumnDescriptor.Binary(column, size));
            return column;
        }

        public static _Binary AsSqlBinaryMax(this _Binary column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.Binary(column, -1));
            return column;
        }

        public static _Binary AsSqlVarBinary(this _Binary column, int size = MAX_VARBINARY_SIZE)
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_VARBINARY_SIZE || size > MAX_VARBINARY_SIZE)
                throw new ArgumentOutOfRangeException("size");
            column.SetMapper(SqlColumnDescriptor.VarBinary(column, size));
            return column;
        }

        public static _Binary AsSqlVarBinaryMax(this _Binary column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.VarBinary(column, -1));
            return column;
        }

        public static _Binary AsSqlTimestamp(this _Binary column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.Timestamp(column));
            return column;
        }

        public static _Decimal AsSqlDecimal(this _Decimal column, byte precision = DEFAULT_DECIMAL_PRECISION, byte scale = DEFAULT_DECIMAL_SCALE)
        {
            column.VerifyNotNull(nameof(column));
            if (precision < MIN_DECIMAL_PRECISION || precision > MAX_DECIMAL_PRECISION)
                throw new ArgumentOutOfRangeException("precision");
            if (scale < 0 || scale > precision)
                throw new ArgumentOutOfRangeException("scale");
            column.SetMapper(SqlColumnDescriptor.Decimal(column, precision, scale));
            return column;
        }

        public static _Decimal AsSqlSmallMoney(this _Decimal column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.SmallMoney(column));
            return column;
        }

        public static _Decimal AsSqlMoney(this _Decimal column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.Money(column));
            return column;
        }

        public static _DateTime AsSqlDate(this _DateTime column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.Date(column));
            return column;
        }

        public static _DateTime AsSqlTime(this _DateTime column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.Time(column));
            return column;
        }

        public static _DateTime AsSqlSmallDateTime(this _DateTime column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.SmallDateTime(column));
            return column;
        }

        public static _DateTime AsSqlDateTime(this _DateTime column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.DateTime(column));
            return column;
        }

        public static _DateTime AsSqlDateTime2(this _DateTime column, byte precision)
        {
            column.VerifyNotNull(nameof(column));
            if (precision < MIN_DATETIME2_PRECISION || precision > MAX_DATETIME2_PRECISION)
                throw new ArgumentOutOfRangeException("precision");
            column.SetMapper(SqlColumnDescriptor.DateTime2(column, precision));
            return column;
        }

        public static _String AsSqlNChar(this _String column, int size)
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_CHAR_SIZE || size > MAX_CHAR_SIZE)
                throw new ArgumentOutOfRangeException("size");
            column.SetMapper(SqlColumnDescriptor.NChar(column, size));
            return column;
        }

        public static _String AsSqlNCharMax(this _String column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.NChar(column, -1));
            return column;
        }

        public static _String AsSqlNVarChar(this _String column, int size = MAX_NVARCHAR_SIZE)
        {
            column.VerifyNotNull(nameof(column));
            if ((size < MIN_NVARCHAR_SIZE || size > MAX_NVARCHAR_SIZE) && size != -1)
                throw new ArgumentOutOfRangeException("size");
            column.SetMapper(SqlColumnDescriptor.NVarChar(column, size));
            return column;
        }

        public static _String AsSqlNVarCharMax(this _String column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.NVarChar(column, -1));
            return column;
        }

        public static T IsUnicode<T>(this T column, bool isUnicode)
            where T: Column<Char?>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.SingleChar(column, isUnicode));
            return column;
        }

        public static _String AsSqlChar(this _String column, int size)
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_CHAR_SIZE || size > MAX_CHAR_SIZE)
                throw new ArgumentOutOfRangeException("size");
            column.SetMapper(SqlColumnDescriptor.Char(column, size));
            return column;
        }

        public static _String AsSqlCharMax(this _String column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.Char(column, -1));
            return column;
        }

        public static T AsSqlVarChar<T>(this T column, int size = MAX_VARBINARY_SIZE)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_VARCHAR_SIZE || size > MAX_VARCHAR_SIZE)
                throw new ArgumentOutOfRangeException("size");
            column.SetMapper(SqlColumnDescriptor.VarChar(column, size));
            return column;
        }

        public static _String AsSqlVarCharMax(this _String column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(SqlColumnDescriptor.VarChar(column, -1));
            return column;
        }

        internal static SqlColumnDescriptor GetSqlColumnDescriptor(this Column column)
        {
            var result = column.GetAddon<SqlColumnDescriptor>() ?? column.GetDefaultSqlColumnDescriptor();
            return result;
        }

        private abstract class SqlColumnDescriptorProvider
        {
            public abstract SqlColumnDescriptor GetDefaultSqlColumnDescriptor(Column column);

            public abstract Type DataType { get; }
        }

        private sealed class SqlColumnDescriptorProvider<T> : SqlColumnDescriptorProvider
        {
            public SqlColumnDescriptorProvider(Func<Column<T>, SqlColumnDescriptor> callback)
            {
                Debug.Assert(callback != null);
                _callback = callback;
            }

            readonly Func<Column<T>, SqlColumnDescriptor> _callback;

            public override Type DataType
            {
                get { return typeof(T); }
            }

            public override SqlColumnDescriptor GetDefaultSqlColumnDescriptor(Column column)
            {
                var x = column as Column<T>;
                return x == null ? null : _callback(x);
            }
        }

        private sealed class SqlColumnDescriptorProviderCollection : Dictionary<Type, SqlColumnDescriptorProvider>
        {
            public void Add(SqlColumnDescriptorProvider value)
            {
                base.Add(value.DataType, value);
            }
        }

        private static readonly ConcurrentDictionary<Type, SqlColumnDescriptorProvider> s_defaultSqlColumnDescriptorProviders = new ConcurrentDictionary<Type, SqlColumnDescriptorProvider>(
            new SqlColumnDescriptorProviderCollection()
            {
                new SqlColumnDescriptorProvider<Binary>(x => SqlColumnDescriptor.VarBinary(x, MAX_VARBINARY_SIZE)),
                new SqlColumnDescriptorProvider<Boolean?>(x => SqlColumnDescriptor.Bit(x)),
                new SqlColumnDescriptorProvider<Byte?>(x => SqlColumnDescriptor.TinyInt(x)),
                new SqlColumnDescriptorProvider<Char?>(x => SqlColumnDescriptor.SingleChar(x, false)),
                new SqlColumnDescriptorProvider<DateTime?>(x => SqlColumnDescriptor.DateTime2(x, MAX_DATETIME2_PRECISION)),
                new SqlColumnDescriptorProvider<DateTimeOffset?>(x => SqlColumnDescriptor.DateTimeOffset(x)),
                new SqlColumnDescriptorProvider<Decimal?>(x => SqlColumnDescriptor.Decimal(x, DEFAULT_DECIMAL_PRECISION, DEFAULT_DECIMAL_SCALE)),
                new SqlColumnDescriptorProvider<Double?>(x => SqlColumnDescriptor.Double(x)),
                new SqlColumnDescriptorProvider<Guid?>(x => SqlColumnDescriptor.UniqueIdentifier(x)),
                new SqlColumnDescriptorProvider<Int16?>(x => SqlColumnDescriptor.SmallInt(x)),
                new SqlColumnDescriptorProvider<Int32?>(x => SqlColumnDescriptor.Int(x)),
                new SqlColumnDescriptorProvider<Int64?>(x => SqlColumnDescriptor.BigInt(x)),
                new SqlColumnDescriptorProvider<Single?>(x => SqlColumnDescriptor.Single(x)),
                new SqlColumnDescriptorProvider<String>(x => SqlColumnDescriptor.NVarChar(x, MAX_NVARCHAR_SIZE)),
                new SqlColumnDescriptorProvider<TimeSpan?>(x => SqlColumnDescriptor.TimeSpan(x)),
                new SqlColumnDescriptorProvider<SqlXml>(x => SqlColumnDescriptor.Xml(x))
            });

        private static SqlColumnDescriptor GetDefaultSqlColumnDescriptor(this Column column)
        {
            Debug.Assert(column != null);
            return column.GetDefaultSqlColumnDescriptorProvider().GetDefaultSqlColumnDescriptor(column);
        }

        private static SqlColumnDescriptorProvider GetDefaultSqlColumnDescriptorProvider(this Column column)
        {
            SqlColumnDescriptorProvider result;
            if (s_defaultSqlColumnDescriptorProviders.TryGetValue(column.DataType, out result))
                return result;

            var columnType = column.GetType();
            var columnDataType = column.DataType;
            var enumType = columnDataType.GenericTypeArguments[0];
            if (columnType.IsDerivedFromGeneric(typeof(_ByteEnum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetByteEnumColumnDescriptorProvider));
                return s_defaultSqlColumnDescriptorProviders.GetOrAdd(columnType, BuildSqlColumnDescriptorProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_CharEnum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetCharEnumColumnDescriptorProvider));
                return s_defaultSqlColumnDescriptorProviders.GetOrAdd(columnType, BuildSqlColumnDescriptorProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_Int16Enum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetInt16EnumColumnDescriptorProvider));
                return s_defaultSqlColumnDescriptorProviders.GetOrAdd(columnType, BuildSqlColumnDescriptorProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_Int32Enum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetInt32EnumColumnDescriptorProvider));
                return s_defaultSqlColumnDescriptorProviders.GetOrAdd(columnType, BuildSqlColumnDescriptorProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_Int64Enum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetInt64EnumColumnDescriptorProvider));
                return s_defaultSqlColumnDescriptorProviders.GetOrAdd(columnType, BuildSqlColumnDescriptorProviderFactory(methodInfo, enumType));
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

        private static SqlColumnDescriptorProvider<T?> GetCharEnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new SqlColumnDescriptorProvider<T?>(x => SqlColumnDescriptor.CharEnum<T>((_CharEnum<T>)x));
        }

        private static SqlColumnDescriptorProvider<T?> GetByteEnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new SqlColumnDescriptorProvider<T?>(x => SqlColumnDescriptor.ByteEnum<T>((_ByteEnum<T>)x));
        }

        private static SqlColumnDescriptorProvider<T?> GetInt16EnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new SqlColumnDescriptorProvider<T?>(x => SqlColumnDescriptor.Int16Enum<T>((_Int16Enum<T>)x));
        }

        private static SqlColumnDescriptorProvider<T?> GetInt32EnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new SqlColumnDescriptorProvider<T?>(x => SqlColumnDescriptor.Int32Enum<T>((_Int32Enum<T>)x));
        }

        private static SqlColumnDescriptorProvider<T?> GetInt64EnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new SqlColumnDescriptorProvider<T?>(x => SqlColumnDescriptor.Int64Enum<T>((_Int64Enum<T>)x));
        }

        private static Func<Type, SqlColumnDescriptorProvider> BuildSqlColumnDescriptorProviderFactory(MethodInfo methodInfo, Type columnDataType)
        {
            methodInfo = methodInfo.MakeGenericMethod(columnDataType);
            var call = Expression.Call(methodInfo);
            return Expression.Lambda<Func<Type, SqlColumnDescriptorProvider>>(call, Expression.Parameter(typeof(Type))).Compile();
        }

        internal static SqlDbType GetSqlDbType(this Column column, SqlVersion sqlVersion)
        {
            return column.GetSqlColumnDescriptor().GetSqlParameterInfo(sqlVersion).SqlDbType;
        }

        internal static bool IsUnicode(this Column column, SqlVersion sqlVersion)
        {
            return column.GetSqlDbType(sqlVersion).IsUnicode();
        }
    }
}
