using DevZest.Data;
using Microsoft.VisualBasic.CompilerServices;

/// <summary>
/// Provides ModelOf() operator for VB.Net because _ is not a valid identifier in VB.
/// </summary>
[StandardModule]
public sealed class DataSourceModule
{
    /// <exclude />
    public static T ModelOf<T>(DataSet<T> dataSet) where T : Model, new()
    {
        return dataSet._;
    }

    /// <exclude />
    public static T ModelOf<T>(DbSet<T> dbSet) where T : Model, new()
    {
        return dbSet._;
    }
}