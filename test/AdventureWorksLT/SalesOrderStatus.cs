using System;
using DevZest.Data;
using DevZest.Data.Primitives;

namespace DevZest.Samples.AdventureWorksLT
{
    public struct SalesOrderStatus
    {
        public static readonly SalesOrderStatus Null = new SalesOrderStatus();
        public static readonly SalesOrderStatus InProcess = new SalesOrderStatus(1);
        public static readonly SalesOrderStatus Approved = new SalesOrderStatus(2);
        public static readonly SalesOrderStatus Backordered = new SalesOrderStatus(3);
        public static readonly SalesOrderStatus Rejected = new SalesOrderStatus(4);
        public static readonly SalesOrderStatus Shipped = new SalesOrderStatus(5);
        public static readonly SalesOrderStatus Cancelled = new SalesOrderStatus(6);

        public static SalesOrderStatus FromValue(byte? value)
        {
            return value.HasValue ? s_values[GetIndex(value.GetValueOrDefault())] : SalesOrderStatus.Null;
        }

        private static int GetIndex(byte value)
        {
            return value - 1;
        }

        private static SalesOrderStatus[] s_values = new SalesOrderStatus[]
        {
            SalesOrderStatus.InProcess,
            SalesOrderStatus.Approved,
            SalesOrderStatus.Backordered,
            SalesOrderStatus.Rejected,
            SalesOrderStatus.Shipped,
            SalesOrderStatus.Cancelled,
        };

        private static string[] s_names = new string[]
        {
            "InProcess",
            "Approved",
            "Backordered",
            "Rejected",
            "Shipped",
            "Cancelled",
        };

        private SalesOrderStatus(byte value)
        {
            _dbValue = value;
        }

        private byte? _dbValue;
        public Byte? DbValue
        {
            get { return _dbValue; }
        }

        public bool IsNull
        {
            get { return !_dbValue.HasValue; }
        }

        public override int GetHashCode()
        {
            return _dbValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is SalesOrderStatus ? _dbValue == ((SalesOrderStatus)obj)._dbValue : false;
        }

        public override string ToString()
        {
            return _dbValue.HasValue ? s_names[GetIndex(_dbValue.GetValueOrDefault())] : string.Empty;
        }
    }

    public sealed class _SalesOrderStatus : ByteEnum<SalesOrderStatus>
    {
        protected override Column<SalesOrderStatus> CreateParam(SalesOrderStatus value)
        {
            return Param(value, this);
        }

        protected override Column<SalesOrderStatus> CreateConst(SalesOrderStatus value)
        {
            return Const(value);
        }

        public static _SalesOrderStatus Param(SalesOrderStatus x, _SalesOrderStatus sourceColumn = null)
        {
            return new ParamExpression<SalesOrderStatus>(x, sourceColumn).MakeColumn<_SalesOrderStatus>();
        }

        public static _SalesOrderStatus Const(SalesOrderStatus x)
        {
            return new ConstantExpression<SalesOrderStatus>(x).MakeColumn<_SalesOrderStatus>();
        }

        public static implicit operator _SalesOrderStatus(SalesOrderStatus x)
        {
            return Param(x);
        }

        protected override bool IsNull(SalesOrderStatus value)
        {
            return value.IsNull;
        }

        public override byte? ConvertToByte(SalesOrderStatus value)
        {
            return value.DbValue;
        }

        public override SalesOrderStatus ConvertToEnum(byte? value)
        {
            return SalesOrderStatus.FromValue(value);
        }
    }
}
