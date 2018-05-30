using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using DevZest.Data.Primitives;
using System.Data.SqlTypes;
using System.Collections.Concurrent;
using System.Reflection;
using DevZest.Data.Utilities;
using System.Linq.Expressions;

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

        private static void SetMapper(this Column column, ColumnMapper columnMapper)
        {
            column.AddOrUpdateExtension(columnMapper);
        }

        public static T AsBinary<T>(this T column, int size)
            where T : Column<Binary>
        {
            column.VerifyNotNull(nameof(column));

            if (size < MIN_BINARY_SIZE || size > MAX_BINARY_SIZE)
                throw new ArgumentOutOfRangeException("size");
            column.SetMapper(ColumnMapper.Binary(column, size));
            return column;
        }

        public static T AsBinaryMax<T>(this T column)
            where T : Column<Binary>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.Binary(column, -1));
            return column;
        }

        public static T AsVarBinary<T>(this T column, int size = MAX_VARBINARY_SIZE)
            where T : Column<Binary>
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_VARBINARY_SIZE || size > MAX_VARBINARY_SIZE)
                throw new ArgumentOutOfRangeException("size");
            column.SetMapper(ColumnMapper.VarBinary(column, size));
            return column;
        }

        public static T AsVarBinaryMax<T>(this T column)
            where T : Column<Binary>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.VarBinary(column, -1));
            return column;
        }

        public static T AsTimestamp<T>(this T column)
            where T : Column<Binary>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.Timestamp(column));
            return column;
        }

        public static T AsDecimal<T>(this T column, byte precision = DEFAULT_DECIMAL_PRECISION, byte scale = DEFAULT_DECIMAL_SCALE)
            where T : Column<Decimal?>
        {
            column.VerifyNotNull(nameof(column));
            if (precision < MIN_DECIMAL_PRECISION || precision > MAX_DECIMAL_PRECISION)
                throw new ArgumentOutOfRangeException("precision");
            if (scale < 0 || scale > precision)
                throw new ArgumentOutOfRangeException("scale");
            column.SetMapper(ColumnMapper.Decimal(column, precision, scale));
            return column;
        }

        public static T AsSmallMoney<T>(this T column)
            where T : Column<Decimal?>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.SmallMoney(column));
            return column;
        }

        public static T AsMoney<T>(this T column)
            where T : Column<Decimal?>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.Money(column));
            return column;
        }

        public static T AsDate<T>(this T column)
            where T : Column<DateTime?>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.Date(column));
            return column;
        }

        public static T AsTime<T>(this T column)
            where T : Column<DateTime?>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.Time(column));
            return column;
        }

        public static T AsSmallDateTime<T>(this T column)
            where T : Column<DateTime?>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.SmallDateTime(column));
            return column;
        }

        public static T AsDateTime<T>(this T column)
            where T : Column<DateTime?>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.DateTime(column));
            return column;
        }

        public static T AsDateTime2<T>(this T column, byte precision)
            where T : Column<DateTime?>
        {
            column.VerifyNotNull(nameof(column));
            if (precision < MIN_DATETIME2_PRECISION || precision > MAX_DATETIME2_PRECISION)
                throw new ArgumentOutOfRangeException("precision");
            column.SetMapper(ColumnMapper.DateTime2(column, precision));
            return column;
        }

        public static T AsNChar<T>(this T column, int size)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_CHAR_SIZE || size > MAX_CHAR_SIZE)
                throw new ArgumentOutOfRangeException("size");
            column.SetMapper(ColumnMapper.NChar(column, size));
            return column;
        }

        public static T AsNCharMax<T>(this T column)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.NChar(column, -1));
            return column;
        }

        public static T AsNVarChar<T>(this T column, int size = MAX_NVARCHAR_SIZE)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            if ((size < MIN_NVARCHAR_SIZE || size > MAX_NVARCHAR_SIZE) && size != -1)
                throw new ArgumentOutOfRangeException("size");
            column.SetMapper(ColumnMapper.NVarChar(column, size));
            return column;
        }

        public static T AsNVarCharMax<T>(this T column)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.NVarChar(column, -1));
            return column;
        }

        public static T IsUnicode<T>(this T column, bool isUnicode)
            where T: Column<Char?>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.SingleChar(column, isUnicode));
            return column;
        }

        public static T AsChar<T>(this T column, int size)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_CHAR_SIZE || size > MAX_CHAR_SIZE)
                throw new ArgumentOutOfRangeException("size");
            column.SetMapper(ColumnMapper.Char(column, size));
            return column;
        }

        public static T AsCharMax<T>(this T column)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.Char(column, -1));
            return column;
        }

        public static T AsVarChar<T>(this T column, int size = MAX_VARBINARY_SIZE)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_VARCHAR_SIZE || size > MAX_VARCHAR_SIZE)
                throw new ArgumentOutOfRangeException("size");
            column.SetMapper(ColumnMapper.VarChar(column, size));
            return column;
        }

        public static T AsVarCharMax<T>(this T column)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMapper(ColumnMapper.VarChar(column, -1));
            return column;
        }

        internal static ColumnMapper GetMapper(this Column column)
        {
            var result = column.GetExtension<ColumnMapper>() ?? column.GetDefaultMapper();
            return result;
        }

        private abstract class MapperProvider
        {
            public abstract ColumnMapper GetDefaultMapper(Column column);

            public abstract Type DataType { get; }
        }

        private sealed class MapperProvider<T> : MapperProvider
        {
            public MapperProvider(Func<Column<T>, ColumnMapper> callback)
            {
                Debug.Assert(callback != null);
                _callback = callback;
            }

            Func<Column<T>, ColumnMapper> _callback;

            public override Type DataType
            {
                get { return typeof(T); }
            }

            public override ColumnMapper GetDefaultMapper(Column column)
            {
                var x = column as Column<T>;
                return x == null ? null : _callback(x);
            }
        }

        private sealed class MapperProviderCollection : Dictionary<Type, MapperProvider>
        {
            public void Add(MapperProvider value)
            {
                base.Add(value.DataType, value);
            }
        }

        private static readonly ConcurrentDictionary<Type, MapperProvider> s_defaultMapperProviders = new ConcurrentDictionary<Type, MapperProvider>(
            new MapperProviderCollection()
            {
                new MapperProvider<Binary>(x => ColumnMapper.VarBinary(x, MAX_VARBINARY_SIZE)),
                new MapperProvider<Boolean?>(x => ColumnMapper.Bit(x)),
                new MapperProvider<Byte?>(x => ColumnMapper.TinyInt(x)),
                new MapperProvider<Char?>(x => ColumnMapper.SingleChar(x, false)),
                new MapperProvider<DateTime?>(x => ColumnMapper.DateTime2(x, MAX_DATETIME2_PRECISION)),
                new MapperProvider<DateTimeOffset?>(x => ColumnMapper.DateTimeOffset(x)),
                new MapperProvider<Decimal?>(x => ColumnMapper.Decimal(x, DEFAULT_DECIMAL_PRECISION, DEFAULT_DECIMAL_SCALE)),
                new MapperProvider<Double?>(x => ColumnMapper.Double(x)),
                new MapperProvider<Guid?>(x => ColumnMapper.UniqueIdentifier(x)),
                new MapperProvider<Int16?>(x => ColumnMapper.SmallInt(x)),
                new MapperProvider<Int32?>(x => ColumnMapper.Int(x)),
                new MapperProvider<Int64?>(x => ColumnMapper.BigInt(x)),
                new MapperProvider<Single?>(x => ColumnMapper.Single(x)),
                new MapperProvider<String>(x => ColumnMapper.NVarChar(x, MAX_NVARCHAR_SIZE)),
                new MapperProvider<TimeSpan?>(x => ColumnMapper.TimeSpan(x)),
                new MapperProvider<SqlXml>(x => ColumnMapper.Xml(x))
            });

        private static ColumnMapper GetDefaultMapper(this Column column)
        {
            Debug.Assert(column != null);
            return column.GetDefaultMapperProvider().GetDefaultMapper(column);
        }

        private static MapperProvider GetDefaultMapperProvider(this Column column)
        {
            MapperProvider result;
            if (s_defaultMapperProviders.TryGetValue(column.DataType, out result))
                return result;

            var columnType = column.GetType();
            var columnDataType = column.DataType;
            var enumType = columnDataType.GenericTypeArguments[0];
            if (columnType.IsDerivedFromGeneric(typeof(_ByteEnum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetByteEnumMapperProvider));
                return s_defaultMapperProviders.GetOrAdd(columnType, BuildMapperProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_CharEnum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetCharEnumMapperProvider));
                return s_defaultMapperProviders.GetOrAdd(columnType, BuildMapperProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_Int16Enum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetInt16EnumMapperProvider));
                return s_defaultMapperProviders.GetOrAdd(columnType, BuildMapperProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_Int32Enum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetInt32EnumMapperProvider));
                return s_defaultMapperProviders.GetOrAdd(columnType, BuildMapperProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_Int64Enum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetInt64EnumMapperProvider));
                return s_defaultMapperProviders.GetOrAdd(columnType, BuildMapperProviderFactory(methodInfo, enumType));
            }

            throw new NotSupportedException(DiagnosticMessages.ColumnTypeNotSupported(column.GetType()));
        }

        private static MapperProvider<T?> GetCharEnumMapperProvider<T>()
            where T : struct, IConvertible
        {
            return new MapperProvider<T?>(x => ColumnMapper.CharEnum<T>((_CharEnum<T>)x));
        }

        private static MapperProvider<T?> GetByteEnumMapperProvider<T>()
            where T : struct, IConvertible
        {
            return new MapperProvider<T?>(x => ColumnMapper.ByteEnum<T>((_ByteEnum<T>)x));
        }

        private static MapperProvider<T?> GetInt16EnumMapperProvider<T>()
            where T : struct, IConvertible
        {
            return new MapperProvider<T?>(x => ColumnMapper.Int16Enum<T>((_Int16Enum<T>)x));
        }

        private static MapperProvider<T?> GetInt32EnumMapperProvider<T>()
            where T : struct, IConvertible
        {
            return new MapperProvider<T?>(x => ColumnMapper.Int32Enum<T>((_Int32Enum<T>)x));
        }

        private static MapperProvider<T?> GetInt64EnumMapperProvider<T>()
            where T : struct, IConvertible
        {
            return new MapperProvider<T?>(x => ColumnMapper.Int64Enum<T>((_Int64Enum<T>)x));
        }

        private static Func<Type, MapperProvider> BuildMapperProviderFactory(MethodInfo methodInfo, Type columnDataType)
        {
            methodInfo = methodInfo.MakeGenericMethod(columnDataType);
            var call = Expression.Call(methodInfo);
            return Expression.Lambda<Func<Type, MapperProvider>>(call, Expression.Parameter(typeof(Type))).Compile();
        }

        internal static SqlDbType GetSqlDbType(this Column column, SqlVersion sqlVersion)
        {
            return column.GetMapper().GetSqlParameterInfo(sqlVersion).SqlDbType;
        }

        internal static bool IsUnicode(this Column column, SqlVersion sqlVersion)
        {
            return column.GetSqlDbType(sqlVersion).IsUnicode();
        }
    }
}
