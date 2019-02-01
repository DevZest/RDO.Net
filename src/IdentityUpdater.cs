using System;

namespace DevZest.Data.MySql
{
    internal abstract class IdentityUpdater
    {
        public static void Execute(DataSet dataSet, long lastInsertId)
        {
            var identityColumn = dataSet.Model.GetColumns()[dataSet.Model.GetIdentity(false).Column.Ordinal];
            IdentityUpdater updater;
            if (identityColumn is _Int32 int32IdentityColumn)
                updater = new Int32IdentityUpdater(dataSet, int32IdentityColumn, (int)lastInsertId);
            else if (identityColumn is _Int64 int64IdentityColumn)
                updater = new Int64IdentityUpdater(dataSet, int64IdentityColumn, lastInsertId);
            else if (identityColumn is _Int16 int16IdentityColumn)
                updater = new Int16IdentityUpdater(dataSet, int16IdentityColumn, (Int16)lastInsertId);
            else
                updater = null;
            updater.Update();
        }

        protected abstract void Update();

        private abstract class BaseIdentityUpdater<T> : IdentityUpdater
            where T : struct
        {
            protected BaseIdentityUpdater(DataSet dataSet, Column<T?> identityColumn, T lastInsertId)
            {
                DataSet = dataSet;
                IdentityColumn = identityColumn;
                LastInsertId = lastInsertId;
            }

            private DataSet DataSet { get; }

            private Column<T?> IdentityColumn { get; }

            private T LastInsertId { get; }

            protected abstract T Next(T insertId);

            protected sealed override void Update()
            {
                var insertId = LastInsertId;
                for (int i = 0; i < DataSet.Count; i++)
                {
                    IdentityColumn[i] = insertId;
                    insertId = Next(insertId);
                }
            }
        }

        private sealed class Int32IdentityUpdater : BaseIdentityUpdater<int>
        {
            public Int32IdentityUpdater(DataSet dataSet, _Int32 identityColumn, Int32 lastInsertId)
                : base(dataSet, identityColumn, lastInsertId)
            {
            }

            protected override int Next(Int32 insertId)
            {
                return insertId++;
            }
        }

        private sealed class Int64IdentityUpdater : BaseIdentityUpdater<Int64>
        {
            public Int64IdentityUpdater(DataSet dataSet, _Int64 identityColumn, Int64 lastInsertId)
                : base(dataSet, identityColumn, lastInsertId)
            {
            }

            protected override Int64 Next(Int64 insertId)
            {
                return insertId++;
            }
        }

        private sealed class Int16IdentityUpdater : BaseIdentityUpdater<Int16>
        {
            public Int16IdentityUpdater(DataSet dataSet, _Int16 identityColumn, Int16 lastInsertId)
                : base(dataSet, identityColumn, lastInsertId)
            {
            }

            protected override Int16 Next(Int16 insertId)
            {
                return insertId++;
            }
        }
    }
}
