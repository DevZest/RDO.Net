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
using DevZest.Data.MySql.Addons;
using MySql.Data.MySqlClient;

namespace DevZest.Data.MySql
{
    public static class ColumnExtensions
    {
        private const int MIN_VARBINARY_SIZE = 1;
        internal const int MAX_VARBINARY_SIZE = 65535;

        private const int MIN_NVARCHAR_SIZE = 1;
        internal const int MAX_NVARCHAR_SIZE = 21845;

        private const int MIN_VARCHAR_SIZE = 1;
        internal const int MAX_VARCHAR_SIZE = 65535;

        private const int MIN_CHAR_SIZE = 0;
        internal const int MAX_CHAR_SIZE = 255;

        private const int MIN_NCHAR_SIZE = 0;
        internal const int MAX_NCHAR_SIZE = 255;

        private const byte MIN_DECIMAL_PRECISION = 1;
        private const byte MAX_DECIMAL_PRECISION = 65;
        internal const byte DEFAULT_DECIMAL_PRECISION = 10;

        private const byte MIN_DECIMAL_SCALE = 0;
        private const byte MAX_DECIMAL_SCALE = 30;
        internal const byte DEFAULT_DECIMAL_SCALE = 0;

        private const byte MIN_DATETIME2_PRECISION = 1;
        private const byte MAX_DATETIME2_PRECISION = 7;

        private const int MIN_BINARY_SIZE = 0;
        internal const int MAX_BINARY_SIZE = 255;

        private static void SetMySqlType(this Column column, MySqlType mySqlType)
        {
            column.AddOrUpdate(mySqlType);
        }

        public static _Binary AsMySqlBinary(this _Binary column, int size)
        {
            column.VerifyNotNull(nameof(column));

            if (size < MIN_BINARY_SIZE || size > MAX_BINARY_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetMySqlType(MySqlType.Binary(column, size));
            return column;
        }

        public static _Binary AsMySqlVarBinary(this _Binary column, int size = MAX_VARBINARY_SIZE)
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_VARBINARY_SIZE || size > MAX_VARBINARY_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetMySqlType(MySqlType.VarBinary(column, size));
            return column;
        }

        public static _Binary AsMySqlTimestamp(this _Binary column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMySqlType(MySqlType.Timestamp(column));
            return column;
        }

        public static _Decimal AsMySqlDecimal(this _Decimal column, byte precision = DEFAULT_DECIMAL_PRECISION, byte scale = DEFAULT_DECIMAL_SCALE)
        {
            column.VerifyNotNull(nameof(column));
            if (precision < MIN_DECIMAL_PRECISION || precision > MAX_DECIMAL_PRECISION)
                throw new ArgumentOutOfRangeException(nameof(precision));
            if (scale < MIN_DECIMAL_SCALE || scale > MAX_DECIMAL_SCALE)
                throw new ArgumentOutOfRangeException(nameof(scale));
            column.SetMySqlType(MySqlType.Decimal(column, precision, scale));
            return column;
        }

        public static _DateTime AsMySqlDate(this _DateTime column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMySqlType(MySqlType.Date(column));
            return column;
        }

        public static _DateTime AsMySqlTime(this _DateTime column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMySqlType(MySqlType.Time(column));
            return column;
        }

        public static _DateTime AsMySqlDateTime(this _DateTime column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMySqlType(MySqlType.DateTime(column));
            return column;
        }

        public static _String AsMySqlNChar(this _String column, int size)
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_CHAR_SIZE || size > MAX_CHAR_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetMySqlType(MySqlType.NChar(column, size));
            return column;
        }

        public static _String AsMySqlNVarChar(this _String column, int size = MAX_NVARCHAR_SIZE)
        {
            column.VerifyNotNull(nameof(column));
            if ((size < MIN_NVARCHAR_SIZE || size > MAX_NVARCHAR_SIZE))
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetMySqlType(MySqlType.NVarChar(column, size));
            return column;
        }

        public static T AsMySqlSingleChar<T>(this T column, bool isUnicode)
            where T : Column<Char?>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMySqlType(MySqlType.SingleChar(column, isUnicode));
            return column;
        }

        public static T AsMySqlChar<T>(this T column, int size)
            where T : Column<string>
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_CHAR_SIZE || size > MAX_CHAR_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetMySqlType(MySqlType.Char(column, size));
            return column;
        }

        public static T AsMySqlVarChar<T>(this T column, int size)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_VARCHAR_SIZE || size > MAX_VARCHAR_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetMySqlType(MySqlType.VarChar(column, size));
            return column;
        }

        public static T AsMySqlJson<T>(this T column)
            where T : Column<string>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMySqlType(MySqlType.Json(column));
            return column;
        }

        internal static MySqlType GetMySqlType(this Column column)
        {
            var result = column.GetAddon<MySqlType>() ?? column.GetDefaultMySqlType();
            return result;
        }

        private abstract class MySqlTypeProvider
        {
            public abstract MySqlType GetDefaultMySqlType(Column column);

            public abstract Type DataType { get; }
        }

        private sealed class MySqlTypeProvider<T> : MySqlTypeProvider
        {
            public MySqlTypeProvider(Func<Column<T>, MySqlType> callback)
            {
                Debug.Assert(callback != null);
                _callback = callback;
            }

            readonly Func<Column<T>, MySqlType> _callback;

            public override Type DataType
            {
                get { return typeof(T); }
            }

            public override MySqlType GetDefaultMySqlType(Column column)
            {
                var x = column as Column<T>;
                return x == null ? null : _callback(x);
            }
        }

        private sealed class MySqlTypeProviderCollection : Dictionary<Type, MySqlTypeProvider>
        {
            public void Add(MySqlTypeProvider value)
            {
                base.Add(value.DataType, value);
            }
        }

        private static readonly ConcurrentDictionary<Type, MySqlTypeProvider> s_defaultMySqlTypeProviders = new ConcurrentDictionary<Type, MySqlTypeProvider>(
            new MySqlTypeProviderCollection()
            {
                new MySqlTypeProvider<Binary>(x => MySqlType.VarBinary(x, MAX_VARBINARY_SIZE)),
                new MySqlTypeProvider<Boolean?>(x => MySqlType.Bit(x)),
                new MySqlTypeProvider<Byte?>(x => MySqlType.TinyInt(x)),
                new MySqlTypeProvider<Char?>(x => MySqlType.SingleChar(x, false)),
                new MySqlTypeProvider<DateTime?>(x => MySqlType.DateTime(x)),
                new MySqlTypeProvider<Decimal?>(x => MySqlType.Decimal(x, DEFAULT_DECIMAL_PRECISION, DEFAULT_DECIMAL_SCALE)),
                new MySqlTypeProvider<Double?>(x => MySqlType.Double(x)),
                new MySqlTypeProvider<Guid?>(x => MySqlType.UniqueIdentifier(x)),
                new MySqlTypeProvider<Int16?>(x => MySqlType.SmallInt(x)),
                new MySqlTypeProvider<Int32?>(x => MySqlType.Int(x)),
                new MySqlTypeProvider<Int64?>(x => MySqlType.BigInt(x)),
                new MySqlTypeProvider<Single?>(x => MySqlType.Single(x)),
                new MySqlTypeProvider<String>(x => MySqlType.NVarChar(x, MAX_NVARCHAR_SIZE)),
                new MySqlTypeProvider<TimeSpan?>(x => MySqlType.TimeSpan(x))
            });

        private static MySqlType GetDefaultMySqlType(this Column column)
        {
            Debug.Assert(column != null);
            return column.GetDefaultSqlTypeProvider().GetDefaultMySqlType(column);
        }

        private static MySqlTypeProvider GetDefaultSqlTypeProvider(this Column column)
        {
            MySqlTypeProvider result;
            if (s_defaultMySqlTypeProviders.TryGetValue(column.DataType, out result))
                return result;

            var columnType = column.GetType();
            var columnDataType = column.DataType;
            var enumType = columnDataType.GenericTypeArguments[0];
            if (columnType.IsDerivedFromGeneric(typeof(_ByteEnum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetByteEnumColumnDescriptorProvider));
                return s_defaultMySqlTypeProviders.GetOrAdd(columnType, BuildSqlTypeProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_CharEnum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetCharEnumColumnDescriptorProvider));
                return s_defaultMySqlTypeProviders.GetOrAdd(columnType, BuildSqlTypeProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_Int16Enum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetInt16EnumColumnDescriptorProvider));
                return s_defaultMySqlTypeProviders.GetOrAdd(columnType, BuildSqlTypeProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_Int32Enum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetInt32EnumColumnDescriptorProvider));
                return s_defaultMySqlTypeProviders.GetOrAdd(columnType, BuildSqlTypeProviderFactory(methodInfo, enumType));
            }

            if (columnType.IsDerivedFromGeneric(typeof(_Int64Enum<>)))
            {
                var methodInfo = typeof(ColumnExtensions).GetStaticMethodInfo(nameof(GetInt64EnumColumnDescriptorProvider));
                return s_defaultMySqlTypeProviders.GetOrAdd(columnType, BuildSqlTypeProviderFactory(methodInfo, enumType));
            }

            throw new NotSupportedException(string.Format(DiagnosticMessages.ColumnTypeNotSupported, column.GetType()));
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

        private static MySqlTypeProvider<T?> GetCharEnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new MySqlTypeProvider<T?>(x => MySqlType.CharEnum<T>((_CharEnum<T>)x));
        }

        private static MySqlTypeProvider<T?> GetByteEnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new MySqlTypeProvider<T?>(x => MySqlType.ByteEnum<T>((_ByteEnum<T>)x));
        }

        private static MySqlTypeProvider<T?> GetInt16EnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new MySqlTypeProvider<T?>(x => MySqlType.Int16Enum<T>((_Int16Enum<T>)x));
        }

        private static MySqlTypeProvider<T?> GetInt32EnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new MySqlTypeProvider<T?>(x => MySqlType.Int32Enum<T>((_Int32Enum<T>)x));
        }

        private static MySqlTypeProvider<T?> GetInt64EnumColumnDescriptorProvider<T>()
            where T : struct, IConvertible
        {
            return new MySqlTypeProvider<T?>(x => MySqlType.Int64Enum<T>((_Int64Enum<T>)x));
        }

        private static Func<Type, MySqlTypeProvider> BuildSqlTypeProviderFactory(MethodInfo methodInfo, Type columnDataType)
        {
            methodInfo = methodInfo.MakeGenericMethod(columnDataType);
            var call = Expression.Call(methodInfo);
            return Expression.Lambda<Func<Type, MySqlTypeProvider>>(call, Expression.Parameter(typeof(Type))).Compile();
        }

        internal static MySqlDbType GetMySqlDbType(this Column column, MySqlVersion mySqlVersion)
        {
            return column.GetMySqlType().GetSqlParameterInfo(mySqlVersion).MySqlDbType;
        }
    }
}
