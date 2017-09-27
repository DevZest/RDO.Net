using System;

namespace DevZest.Data
{
    /// <summary>Specifies the kind of <see cref="Column"/>.</summary>
    [Flags]
    public enum ColumnKind
    {
        /// <summary>The column is defined by user.</summary>
        User = 1,
        /// <summary>The column is defined by extension.</summary>
        Extension = 2,
        /// <summary>sys_parent_row_id column of sequential key temp table (internal use only).</summary>
        SystemParentRowId = 4,
        /// <summary>sys_row_id column of sequential key temp table (internal use only).</summary>
        SystemRowId = 8,
        /// <summary>Other system column, such as system columns defined by SQL Server implementation (internal use only).</summary>
        SystemCustom = 16,
        /// <summary>Union of <see cref="SystemRowId"/>, <see cref="SystemParentRowId"/> and <see cref="SystemCustom"/>.</summary>
        System = SystemRowId | SystemParentRowId | SystemCustom,
        /// <summary>All kinds of column.</summary>
        All = User | Extension | System
    }
}
