using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private const int MIN_VARCHAR_SIZE = 1;
        internal const int DEFAULT_VARCHAR_SIZE = 4000;
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

        internal const int MAX_TIME_PRECISION = 6;

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

        public static _Decimal AsMySqlMoney(this _Decimal column)
        {
            return column.AsMySqlDecimal(19, 4);
        }

        public static _DateTime AsMySqlDate(this _DateTime column)
        {
            column.VerifyNotNull(nameof(column));
            column.SetMySqlType(MySqlType.Date(column));
            return column;
        }

        public static _DateTime AsMySqlTime(this _DateTime column, int precision = 0)
        {
            column.VerifyNotNull(nameof(column));
            VerifyTimePrecision(precision, nameof(precision));
            column.SetMySqlType(MySqlType.Time(column, precision));
            return column;
        }

        public static _DateTime AsMySqlDateTime(this _DateTime column, int precision = 0)
        {
            column.VerifyNotNull(nameof(column));
            VerifyTimePrecision(precision, nameof(precision));
            column.SetMySqlType(MySqlType.DateTime(column, precision));
            return column;
        }

        public static _DateTime AsMySqlTimestamp(this _DateTime column, int precision = 0)
        {
            column.VerifyNotNull(nameof(column));
            VerifyTimePrecision(precision, nameof(precision));
            column.SetMySqlType(MySqlType.Timestamp(column, precision));
            return column;
        }

        internal static void VerifyTimePrecision(int precision, string paramName)
        {
            if (precision < 0 || precision > MAX_TIME_PRECISION)
                throw new ArgumentOutOfRangeException(paramName);
        }

        public static T AsMySqlChar<T>(this T column, string charSetName = null, string collationName = null)
            where T : Column<Char?>
        {
            column.VerifyNotNull(nameof(column));
            column.SetMySqlType(MySqlType.SingleChar(column, charSetName, collationName));
            return column;
        }

        public static T AsMySqlChar<T>(this T column, int size, string charSetName = null, string collationName = null)
            where T : Column<string>
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_CHAR_SIZE || size > MAX_CHAR_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetMySqlType(MySqlType.Char(column, size, charSetName, collationName));
            return column;
        }

        public static T AsMySqlVarChar<T>(this T column, int size, string charSetName = null, string collationName = null)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_VARCHAR_SIZE || size > MAX_VARCHAR_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetMySqlType(MySqlType.VarChar(column, size, charSetName, collationName));
            return column;
        }

        public static T AsMySqlText<T>(this T column, int size, string charSetName = null, string collationName = null)
            where T : Column<String>
        {
            column.VerifyNotNull(nameof(column));
            if (size < MIN_VARCHAR_SIZE || size > MAX_VARCHAR_SIZE)
                throw new ArgumentOutOfRangeException(nameof(size));
            column.SetMySqlType(MySqlType.Text(column, size, charSetName, collationName));
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
                new MySqlTypeProvider<Char?>(x => MySqlType.SingleChar(x, null, null)),
                new MySqlTypeProvider<DateTime?>(x => MySqlType.DateTime(x, 0)),
                new MySqlTypeProvider<Decimal?>(x => MySqlType.Decimal(x, DEFAULT_DECIMAL_PRECISION, DEFAULT_DECIMAL_SCALE)),
                new MySqlTypeProvider<Double?>(x => MySqlType.Double(x)),
                new MySqlTypeProvider<Guid?>(x => MySqlType.UniqueIdentifier(x)),
                new MySqlTypeProvider<Int16?>(x => MySqlType.SmallInt(x)),
                new MySqlTypeProvider<Int32?>(x => MySqlType.Int(x)),
                new MySqlTypeProvider<Int64?>(x => MySqlType.BigInt(x)),
                new MySqlTypeProvider<Single?>(x => MySqlType.Single(x)),
                new MySqlTypeProvider<String>(x => MySqlType.VarChar(x, DEFAULT_VARCHAR_SIZE, null, null))
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
