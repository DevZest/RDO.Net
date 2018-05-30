﻿using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;
using System;

namespace DevZest.Samples.AdventureWorksLT
{
    public abstract class BaseModel<T> : Model<T>
        where T : PrimaryKey
    {
        static BaseModel()
        {
            RegisterColumn((BaseModel<T> x) => x.RowGuid);
            RegisterColumn((BaseModel<T> x) => x.ModifiedDate);
        }

        [Required]
        [AutoGuid(Name = "DF_%_rowguid", Description = "Default constraint value of NEWID()")]
        [Unique(Name = "AK_%_rowguid", Description = "Unique nonclustered constraint. Used to support replication samples.")]
        [DbColumn(Description = "ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")]
        public _Guid RowGuid { get; private set; }

        [Required]
        [AsDateTime]
        [AutoDateTime(Name = "DF_%_ModifiedDate", Description = "Default constraint value of GETDATE()")]
        [DbColumn(Description = "Date and time the record was last updated.")]
        public _DateTime ModifiedDate { get; private set; }

        public void ResetRowIdentifiers()
        {
            var dataSet = DataSet;
            if (dataSet == null)
                return;
            for (int i = 0; i < dataSet.Count; i++)
            {
                RowGuid[i] = Guid.NewGuid();
                ModifiedDate[i] = DateTime.Now;
            }
        }
    }
}