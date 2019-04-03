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

        public static MySqlParameterInfo TinyBlob()
        {
            return new MySqlParameterInfo(MySqlDbType.TinyBlob);
        }

        public static MySqlParameterInfo Blob()
        {
            return new MySqlParameterInfo(MySqlDbType.Blob);
        }

        public static MySqlParameterInfo MediumBlob()
        {
            return new MySqlParameterInfo(MySqlDbType.MediumBlob);
        }

        public static MySqlParameterInfo LongBlob()
        {
            return new MySqlParameterInfo(MySqlDbType.LongBlob);
        }

        public static MySqlParameterInfo TinyText()
        {
            return new MySqlParameterInfo(MySqlDbType.TinyText);
        }

        public static MySqlParameterInfo Text()
        {
            return new MySqlParameterInfo(MySqlDbType.Text);
        }

        public static MySqlParameterInfo MediumText()
        {
            return new MySqlParameterInfo(MySqlDbType.MediumText);
        }

        public static MySqlParameterInfo LongText()
        {
            return new MySqlParameterInfo(MySqlDbType.LongText);
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

        public static MySqlParameterInfo Float()
        {
            return new MySqlParameterInfo(MySqlDbType.Float);
        }

        public static MySqlParameterInfo Double()
        {
            return new MySqlParameterInfo(MySqlDbType.Double);
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

#if DEBUG
        // For unit test
        internal MySqlParameterInfo(MySqlDbType mySqlDbType, int? size, byte? precision, byte? scale)
        {
            MySqlDbType = mySqlDbType;
            Size = size;
            Precision = precision;
            Scale = scale;
        }
#endif

        public readonly MySqlDbType MySqlDbType;

        public readonly int? Size;

        public readonly byte? Precision;

        public readonly byte? Scale;
    }
}
