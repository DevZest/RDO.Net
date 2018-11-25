using System;

namespace DevZest.Data
{
    /// <summary>Specifies the kind of <see cref="Column"/>.</summary>
    public enum ColumnKind
    {
        /// <summary>The column is none of the defined <see cref="ColumnKind"/>.</summary>
        None = 0,
        /// <summary>The column is a general column.</summary>
        General = 1,
        /// <summary>The column is local column.</summary>
        Local,
        /// <summary>The column is a item of <see cref="ColumnListItem"/>.</summary>
        ColumnListItem,
        /// <summary>The column is a member of <see cref="Projection"/>.</summary>
        ProjectionMember,
        /// <summary>sys_parent_row_id column of sequential key temp table (internal use only).</summary>
        SystemParentRowId,
        /// <summary>sys_row_id column of sequential key temp table (internal use only).</summary>
        SystemRowId,
        /// <summary>Other system column, such as system columns defined by SQL Server implementation (internal use only).</summary>
        SystemCustom
    }
}
