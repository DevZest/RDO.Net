using MySql.Data.MySqlClient;

namespace DevZest.Data.MySql
{
    internal struct MySqlParameterInfo
    {
        public static MySqlParameterInfo Binary(int size)
        {
            return new MySqlParameterInfo(MySqlDbType.Binary, size);
        }

        public static MySqlParameterInfo VarBinary(int size)
        {
            return new MySqlParameterInfo(MySqlDbType.VarBinary, size);
        }

        public static MySqlParameterInfo Timestamp()
        {
            return new MySqlParameterInfo(MySqlDbType.Timestamp);
        }

        public static MySqlParameterInfo Guid()
        {
            return new MySqlParameterInfo(MySqlDbType.Guid);
        }

        public static MySqlParameterInfo Bit()
        {
            return new MySqlParameterInfo(MySqlDbType.Bit);
        }

        public static MySqlParameterInfo Byte()
        {
            return new MySqlParameterInfo(MySqlDbType.Byte);
        }

        public static MySqlParameterInfo Int16()
        {
            return new MySqlParameterInfo(MySqlDbType.Int16);
        }

        public static MySqlParameterInfo Int32()
        {
            return new MySqlParameterInfo(MySqlDbType.Int32);
        }

        public static MySqlParameterInfo Int64()
        {
            return new MySqlParameterInfo(MySqlDbType.Int64);
        }

        public static MySqlParameterInfo Date()
        {
            return new MySqlParameterInfo(MySqlDbType.Date);
        }

        public static MySqlParameterInfo Time()
        {
            return new MySqlParameterInfo(MySqlDbType.Time);
        }

        public static MySqlParameterInfo DateTime()
        {
            return new MySqlParameterInfo(MySqlDbType.DateTime);
        }

        public static MySqlParameterInfo Single()
        {
            return new MySqlParameterInfo(MySqlDbType.Float, (byte)24);
        }

        public static MySqlParameterInfo Double()
        {
            return new MySqlParameterInfo(MySqlDbType.Float, (byte)53);
        }

        public static MySqlParameterInfo Decimal(byte precision, byte scale)
        {
            return new MySqlParameterInfo(MySqlDbType.Decimal, precision, scale);
        }

        public static MySqlParameterInfo String(int size)
        {
            return new MySqlParameterInfo(MySqlDbType.String, size);
        }

        public static MySqlParameterInfo VarString(int size)
        {
            return new MySqlParameterInfo(MySqlDbType.VarString, size);
        }

        public static MySqlParameterInfo Json()
        {
            return new MySqlParameterInfo(MySqlDbType.JSON);
        }

        private MySqlParameterInfo(MySqlDbType mySqlDbType)
        {
            MySqlDbType = mySqlDbType;
            Size = null;
            Precision = null;
            Scale = null;
        }

        private MySqlParameterInfo(MySqlDbType mySqlDbType, int size)
        {
            MySqlDbType = mySqlDbType;
            Size = size;
            Precision = null;
            Scale = null;
        }

        private MySqlParameterInfo(MySqlDbType mySqlDbType, byte precision)
        {
            MySqlDbType = mySqlDbType;
            Precision = precision;
            Size = null;
            Scale = null;
        }

        private MySqlParameterInfo(MySqlDbType mySqlDbType, byte precision, byte scale)
        {
            MySqlDbType = mySqlDbType;
            Precision = precision;
            Scale = scale;
            Size = null;
        }

        public readonly MySqlDbType MySqlDbType;

        public readonly int? Size;

        public readonly byte? Precision;

        public readonly byte? Scale;
    }
}
