﻿using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: null, validOnTypes: new Type[] { typeof(Column) })]
    public sealed class RequiredAttribute : ValidationColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            base.Wireup(column);
            column.Nullable(false);
        }

        protected override bool IsValid(Column column, DataRow dataRow)
        {
            return !column.IsNull(dataRow);
        }

        protected override string DefaultMessageString
        {
            get { return UserMessages.RequiredAttribute; }
        }

        protected override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
