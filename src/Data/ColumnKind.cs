using System;

namespace DevZest.Data
{
    /// <summary>Specifies the kind of <see cref="Column"/>.</summary>
    [Flags]
    public enum ColumnKind
    {
        /// <summary>The column is a property of <see cref="Model"/>.</summary>
        ModelProperty = 1,
        /// <summary>The column is local column.</summary>
        Local = 2,
        /// <summary>The column is a item of <see cref="ColumnListItem"/>.</summary>
        ColumnListItem = 4,
        /// <summary>The column is a member of <see cref="ColumnGroup"/>.</summary>
        ColumnGroupMember = 8,
        /// <summary>sys_parent_row_id column of sequential key temp table (internal use only).</summary>
        SystemParentRowId = 16,
        /// <summary>sys_row_id column of sequential key temp table (internal use only).</summary>
        SystemRowId = 32,
        /// <summary>Other system column, such as system columns defined by SQL Server implementation (internal use only).</summary>
        SystemCustom = 64,
        /// <summary>Union of <see cref="SystemRowId"/>, <see cref="SystemParentRowId"/> and <see cref="SystemCustom"/>.</summary>
        System = SystemRowId | SystemParentRowId | SystemCustom,
    }
}
