﻿using DevZest.Data;
using DevZest.Data.SqlServer;
using DevZest.Data.Annotations.Primitives;
using System;
using DevZest.Data.Addons;
using DevZest.Data.SqlServer.Addons;

namespace DevZest.Samples.AdventureWorksLT
{
    [ModelMemberAttributeSpec(new Type[] { typeof(ColumnNotNull), typeof(SqlType) }, true, typeof(_String))]
    public sealed class UdtOrderNumber : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
            {
                stringColumn.Nullable(true);
                stringColumn.AsSqlNVarChar(25);
            }
        }
    }
}
