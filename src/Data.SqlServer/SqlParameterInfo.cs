using System;
using System.Data;

namespace DevZest.Data.SqlServer
{
    internal struct SqlParameterInfo
    {
        public static SqlParameterInfo Binary(int size)
        {
            return new SqlParameterInfo(SqlDbType.Binary, size);
        }

        public static SqlParameterInfo VarBinary(int size)
        {
            return new SqlParameterInfo(SqlDbType.VarBinary, size);
        }

        public static SqlParameterInfo Timestamp()
        {
            return new SqlParameterInfo(SqlDbType.Timestamp);
        }

        public static SqlParameterInfo UniqueIdentifier()
        {
            return new SqlParameterInfo(SqlDbType.UniqueIdentifier);
        }

        public static SqlParameterInfo Bit()
        {
            return new SqlParameterInfo(SqlDbType.Bit);
        }

        public static SqlParameterInfo TinyInt()
        {
            return new SqlParameterInfo(SqlDbType.TinyInt);
        }

        public static SqlParameterInfo SmallInt()
        {
            return new SqlParameterInfo(SqlDbType.SmallInt);
        }

        public static SqlParameterInfo Int()
        {
            return new SqlParameterInfo(SqlDbType.Int);
        }

        public static SqlParameterInfo BigInt()
        {
            return new SqlParameterInfo(SqlDbType.BigInt);
        }

        public static SqlParameterInfo SmallMoney()
        {
            return new SqlParameterInfo(SqlDbType.SmallMoney);
        }

        public static SqlParameterInfo Money()
        {
            return new SqlParameterInfo(SqlDbType.Money);
        }

        public static SqlParameterInfo Date()
        {
            return new SqlParameterInfo(SqlDbType.Date);
        }

        public static SqlParameterInfo Time()
        {
            return new SqlParameterInfo(SqlDbType.Time);
        }

        public static SqlParameterInfo DateTime()
        {
            return new SqlParameterInfo(SqlDbType.DateTime);
        }

        public static SqlParameterInfo SmallDateTime()
        {
            return new SqlParameterInfo(SqlDbType.SmallDateTime);
        }

        public static SqlParameterInfo DateTime2(byte precision)
        {
            return new SqlParameterInfo(SqlDbType.DateTime2, precision);
        }

        public static SqlParameterInfo DateTimeOffset()
        {
            return new SqlParameterInfo(SqlDbType.DateTimeOffset);
        }

        public static SqlParameterInfo Single()
        {
            return new SqlParameterInfo(SqlDbType.Float, (byte)24);
        }

        public static SqlParameterInfo Double()
        {
            return new SqlParameterInfo(SqlDbType.Float, (byte)53);
        }

        public static SqlParameterInfo Decimal(byte precision, byte scale)
        {
            return new SqlParameterInfo(SqlDbType.Decimal, precision, scale);
        }

        public static SqlParameterInfo NVarChar(int size)
        {
            return new SqlParameterInfo(SqlDbType.NVarChar, size);
        }

        public static SqlParameterInfo NChar(int size)
        {
            return new SqlParameterInfo(SqlDbType.NChar, size);
        }

        public static SqlParameterInfo VarChar(int size)
        {
            return new SqlParameterInfo(SqlDbType.VarChar, size);
        }

        public static SqlParameterInfo Char(int size)
        {
            return new SqlParameterInfo(SqlDbType.Char, size);
        }

        public static SqlParameterInfo Xml()
        {
            return new SqlParameterInfo(SqlDbType.Xml);
        }

        private SqlParameterInfo(SqlDbType sqlDbType)
        {
            SqlDbType = sqlDbType;
            Size = null;
            Precision = null;
            Scale = null;
            UdtTypeName = null;
        }

        private SqlParameterInfo(SqlDbType sqlDbType, int size)
        {
            SqlDbType = sqlDbType;
            Size = size;
            Precision = null;
            Scale = null;
            UdtTypeName = null;
        }

        private SqlParameterInfo(SqlDbType sqlDbType, byte precision)
        {
            SqlDbType = sqlDbType;
            Precision = precision;
            Size = null;
            Scale = null;
            UdtTypeName = null;
        }

        private SqlParameterInfo(SqlDbType sqlDbType, byte precision, byte scale)
        {
            SqlDbType = sqlDbType;
            Precision = precision;
            Scale = scale;
            Size = null;
            UdtTypeName = null;
        }

#if DEBUG
        // For unit test
        internal SqlParameterInfo(SqlDbType sqlDbType, int? size, byte? precision, byte? scale, string udtTypeName)
        {
            SqlDbType = sqlDbType;
            Size = size;
            Precision = precision;
            Scale = scale;
            UdtTypeName = udtTypeName;
        }
#endif

        public readonly SqlDbType SqlDbType;

        public readonly int? Size;

        public readonly byte? Precision;

        public readonly byte? Scale;

        public readonly string UdtTypeName;
    }
}
